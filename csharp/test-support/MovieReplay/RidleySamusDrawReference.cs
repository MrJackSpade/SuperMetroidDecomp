using System.Buffers.Binary;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>Derives Samus arm-cannon and body draw records from native Ridley movie checkpoints.</summary>
internal static class RidleySamusDrawReference
{
    /// <summary>
    /// Oracle-owned description of one $90:C663 arm-cannon draw: whether the OBJ is visible,
    /// whether the tile upload is queued, and the OAM/DMA words the native routine emits.
    /// </summary>
    internal readonly record struct RidleyNativeCannonDrawResult(
        bool SpriteWritten,
        bool TileUploadQueued,
        ushort Frame,
        byte DirectionSelector = 0,
        ushort Attributes = 0,
        ushort TileSource = 0,
        Position16<ScreenPixels> Screen = default);

    /// <summary>Independent $90:C663 arm-cannon draw/transfer result from native state and ROM.</summary>
    internal static RidleyNativeCannonDrawResult RidleyNativeCannonDraw(
        ISnesAddressSpace rom, NativeCheckpointMemory checkpoint, bool invincibleAtDraw, ushort nmi)
    {
        ushort W(int address) => checkpoint.Word(address);
        if ((W(NativeSnapshotMemory.CannonDrawingMode) & 15) == 0) return default;
        ushort frame = W(NativeSnapshotMemory.CannonFrame);
        if (frame == 0 || (invincibleAtDraw && (nmi & 1) != 0))
            return new RidleyNativeCannonDrawResult(false, false, frame);
        ushort pose = W(NativeSnapshotMemory.Pose), animation = W(NativeSnapshotMemory.Animation);
        int drawing = NativeSnapshotMemory.CannonDefinitionBank | CartridgeImportSource.Require(rom).ReadCartridgeWord((NativeSnapshotMemory.CannonPosePointers + pose * 2));
        byte first = rom.ReadCartridgeByte(drawing);
        bool alternate = (first & 128) != 0;
        byte direction = (byte)((alternate && animation != 0 ? rom.ReadCartridgeByte(drawing + 2) : first) & 127);
        int offsets = drawing + (alternate ? 4 : 2) + animation * 2;
        short x = unchecked((short)(W(NativeSnapshotMemory.X) + (sbyte)rom.ReadCartridgeByte(offsets) - W(NativeSnapshotMemory.CameraX)));
        // C6F4 masks the pose graphics offset to a byte, unlike the body renderer's sign extension.
        short y = unchecked((short)(W(NativeSnapshotMemory.Y) + (sbyte)rom.ReadCartridgeByte(offsets + 1) -
            rom.ReadCartridgeByte(NativeSnapshotMemory.PoseDefinitions + pose * 8 + 4) - W(NativeSnapshotMemory.CameraY)));
        ushort attributes = CartridgeImportSource.Require(rom).ReadCartridgeWord((NativeSnapshotMemory.CannonAttributes + direction * 2));
        int tiles = NativeSnapshotMemory.CannonDefinitionBank | CartridgeImportSource.Require(rom).ReadCartridgeWord((NativeSnapshotMemory.CannonTileLists + direction * 2));
        return new RidleyNativeCannonDrawResult(x >= 0 && x < 256 && y >= 0 && y < 256,
            true, frame, direction, attributes, CartridgeImportSource.Require(rom).ReadCartridgeWord((tiles + frame * 2)), new(unchecked((ushort)x), unchecked((ushort)y)));
    }

    /// <summary>Looks for the expected visible cannon sprite in native low and high OAM tables.</summary>
    internal static bool ContainsMovieCannonSprite(ReadOnlySpan<byte> low, ReadOnlySpan<byte> high,
        RidleyNativeCannonDrawResult draw)
    {
        for (int index = 0; index < 128; index++)
        {
            int offset = index * 4;
            if (low[offset] == (byte)draw.Screen.X && low[offset + 1] == (byte)draw.Screen.Y &&
                BinaryPrimitives.ReadUInt16LittleEndian(low.Slice(offset + 2, 2)) == draw.Attributes &&
                ((high[index / 4] >> ((index % 4) * 2)) & 3) == 0)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Read-only cartridge oracle for $90:85E2/$864E/$8C1F. Uses checkpoint physics
    /// and original ROM definitions, never production draw methods or extracted art.
    /// Validated against visible native records before covering opposite NMI phases.
    /// </summary>
    internal static RidleyNativeBodyDraw RidleyNativeBodyRecord(
        ISnesAddressSpace rom, NativeCheckpointMemory checkpoint)
    {
        ushort W(int address) => checkpoint.Word(address);
        if ((W(NativeSnapshotMemory.CeresStatus) & 0x8000) != 0)
            throw new InvalidDataException("Movie body oracle requires a native Mode 7 transform mapping.");
        ushort pose = W(NativeSnapshotMemory.Pose), frame = W(NativeSnapshotMemory.Animation);
        SamusPoseId poseId = SamusPoseNumbering.FromNumber(pose);
        SamusMovementType movement = SamusMovementTypeNumbers.Of(checkpoint[NativeSnapshotMemory.SamusMovementType]);
        int yOffset = -unchecked((sbyte)rom.ReadCartridgeByte(NativeSnapshotMemory.PoseDefinitions + pose * 8 + 4));
        // A landing frame's position word holds the next table byte above its own; the drawn origin shows its own byte.
        int? landingDrawnYOffset = null;
        if (movement == SamusMovementType.Standing)
        {
            if (poseId is SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose)
            {
                if (frame >= 2) yOffset = -1;
            }
            else if (NativePoseNumberRanges.InRange(poseId, SamusPoseId.NormalLandingRightPose, SamusPoseId.SpinLandingLeftPose))
            {
                int landingWord = CartridgeImportSource.Require(rom).ReadCartridgeWord((NativeSnapshotMemory.LandingDrawOffsets +
                    NativePoseNumberRanges.Offset(poseId, SamusPoseId.NormalLandingRightPose) * 4 + frame));
                yOffset = -landingWord;
                landingDrawnYOffset = -LittleEndianBytes.LowByte(unchecked((ushort)landingWord));
            }
        }
        else if (movement == SamusMovementType.PostureTransition &&
            !NativePoseNumberRanges.Precedes(poseId, SamusPoseId.CrouchingTransitionRightPose) &&
            NativePoseNumberRanges.Precedes(poseId, SamusPoseId.MorphBallGroundLeftPose))
            yOffset = unchecked((sbyte)rom.ReadCartridgeByte(NativeSnapshotMemory.PostureDrawOffsets +
                NativePoseNumberRanges.Offset(poseId, SamusPoseId.CrouchingTransitionRightPose) * 2 + frame));
        else if (movement == SamusMovementType.Special)
        {
            if (poseId is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
                yOffset = unchecked((sbyte)rom.ReadCartridgeByte(NativeSnapshotMemory.DrainedDrawOffsets + frame));
            else if (poseId is SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose && frame >= 5)
                yOffset = -3;
        }

        bool bottom = movement switch
        {
            SamusMovementType.MorphBallGround or SamusMovementType.GlitchBall or
                SamusMovementType.MorphBallFalling or SamusMovementType.GlitchBallKnockback or
                SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling => false,
            SamusMovementType.SpinJumping => frame == 0 || frame >= 11 ||
                poseId is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose or
                    SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose,
            SamusMovementType.Knockback => frame >= 3 ||
                poseId is not (SamusPoseId.DeathSequenceRightPose or SamusPoseId.DeathSequenceLeftPose),
            SamusMovementType.PostureTransition => !NativePoseNumberRanges.Precedes(poseId, SamusPoseId.CrouchingTransitionAimUpRightPose) ||
                (!NativePoseNumberRanges.Precedes(poseId, SamusPoseId.CompactStandingUpRightPose) ? frame == 2 :
                 !NativePoseNumberRanges.Precedes(poseId, SamusPoseId.CompactMorphingRightPose) ? frame == 0 :
                 poseId is SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or
                    SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose),
            SamusMovementType.CompactAirborne => frame == 0 || poseId is not (SamusPoseId.CompactAirborneFallingRightPose or SamusPoseId.CompactAirborneFallingLeftPose),
            SamusMovementType.WallJumping => frame is < 3 or >= 13,
            SamusMovementType.DamageBoost => frame is < 2 or >= 9,
            SamusMovementType.Special =>
                poseId is not (SamusPoseId.ShinesparkVerticalRightPose or SamusPoseId.ShinesparkVerticalLeftPose) &&
                (frame >= 2 || poseId is not (SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)),
            _ when Enum.IsDefined(movement) => true,
            SamusMovementType.Standing or
                SamusMovementType.Running or
                SamusMovementType.NormalJumping or
                SamusMovementType.Crouching or
                SamusMovementType.Falling or
                SamusMovementType.TurningOnGround or
                SamusMovementType.Moonwalking or
                SamusMovementType.RanIntoWall or
                SamusMovementType.Grappling or
                SamusMovementType.TurningWhileJumping or
                SamusMovementType.TurningWhileFalling or
                SamusMovementType.DraygonHeld => throw new InvalidDataException($"Unknown native body movement type {movement}"),
            _ => throw new InvalidDataException($"Unknown native body movement type {movement}"),
        };
        var records = new RidleyNativeBodyRecordResult(unchecked((ushort)(CartridgeImportSource.Require(rom).ReadCartridgeWord((NativeSnapshotMemory.TopSpritemapBases + pose * 2)) + frame)),
            bottom ? unchecked((ushort)(CartridgeImportSource.Require(rom).ReadCartridgeWord((NativeSnapshotMemory.BottomSpritemapBases + pose * 2)) + frame)) : (ushort)0,
            unchecked((ushort)(W(NativeSnapshotMemory.X) - W(NativeSnapshotMemory.CameraX))),
            unchecked((ushort)(W(NativeSnapshotMemory.Y) + yOffset - W(NativeSnapshotMemory.CameraY))));
        return new RidleyNativeBodyDraw(records,
            unchecked((ushort)(W(NativeSnapshotMemory.Y) + (landingDrawnYOffset ?? yOffset) - W(NativeSnapshotMemory.CameraY))));
    }
}
