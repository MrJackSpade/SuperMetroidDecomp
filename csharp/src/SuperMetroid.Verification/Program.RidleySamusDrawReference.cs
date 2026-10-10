using System.Buffers.Binary;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Oracle-owned description of one $90:C663 arm-cannon draw: whether the OBJ is visible,
    /// whether the tile upload is queued, and the OAM/DMA words the native routine emits.
    /// </summary>
    private readonly record struct RidleyNativeCannonDrawResult(
        bool SpriteWritten,
        bool TileUploadQueued,
        ushort Frame,
        byte DirectionSelector = 0,
        ushort Attributes = 0,
        ushort TileSource = 0,
        short ScreenX = 0,
        short ScreenY = 0);

    /// <summary>Independent $90:C663 arm-cannon draw/transfer result from native state and ROM.</summary>
    private static RidleyNativeCannonDrawResult RidleyNativeCannonDraw(
        ISnesAddressSpace rom, byte[] checkpoint, bool invincibleAtDraw, ushort nmi)
    {
        ushort W(int address) => BinaryPrimitives.ReadUInt16LittleEndian(checkpoint.AsSpan(address, 2));
        ushort R(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        if ((W(NativeSnapshotMemory.CannonDrawingMode) & 15) == 0) return default;
        ushort frame = W(NativeSnapshotMemory.CannonFrame);
        if (frame == 0 || (invincibleAtDraw && (nmi & 1) != 0))
            return new RidleyNativeCannonDrawResult(false, false, frame);
        ushort pose = W(NativeSnapshotMemory.Pose), animation = W(NativeSnapshotMemory.Animation);
        int drawing = NativeSnapshotMemory.CannonDefinitionBank | R(NativeSnapshotMemory.CannonPosePointers + pose * 2);
        byte first = rom.ReadByte(drawing);
        bool alternate = (first & 128) != 0;
        byte direction = (byte)((alternate && animation != 0 ? rom.ReadByte(drawing + 2) : first) & 127);
        int offsets = drawing + (alternate ? 4 : 2) + animation * 2;
        short x = unchecked((short)(W(NativeSnapshotMemory.X) + (sbyte)rom.ReadByte(offsets) - W(NativeSnapshotMemory.CameraX)));
        // C6F4 masks the pose graphics offset to a byte, unlike the body renderer's sign extension.
        short y = unchecked((short)(W(NativeSnapshotMemory.Y) + (sbyte)rom.ReadByte(offsets + 1) -
            rom.ReadByte(NativeSnapshotMemory.PoseDefinitions + pose * 8 + 4) - W(NativeSnapshotMemory.CameraY)));
        ushort attributes = R(NativeSnapshotMemory.CannonAttributes + direction * 2);
        int tiles = NativeSnapshotMemory.CannonDefinitionBank | R(NativeSnapshotMemory.CannonTileLists + direction * 2);
        return new RidleyNativeCannonDrawResult(x >= 0 && x < 256 && y >= 0 && y < 256,
            true, frame, direction, attributes, R(tiles + frame * 2), x, y);
    }

    private static bool ContainsMovieCannonSprite(ReadOnlySpan<byte> low, ReadOnlySpan<byte> high,
        RidleyNativeCannonDrawResult draw)
    {
        for (int index = 0; index < 128; index++)
        {
            int offset = index * 4;
            if (low[offset] == (byte)draw.ScreenX && low[offset + 1] == (byte)draw.ScreenY &&
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
    private static (ushort Top, ushort Bottom, ushort X, ushort Y) RidleyNativeBodyRecord(
        ISnesAddressSpace rom, byte[] checkpoint)
    {
        ushort W(int address) => BinaryPrimitives.ReadUInt16LittleEndian(checkpoint.AsSpan(address, 2));
        ushort R(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        if ((W(NativeSnapshotMemory.CeresStatus) & 0x8000) != 0)
            throw new InvalidDataException("Movie body oracle requires a native Mode 7 transform mapping.");
        ushort pose = W(NativeSnapshotMemory.Pose), frame = W(NativeSnapshotMemory.Animation);
        SamusPoseId poseId = (SamusPoseId)pose;
        var movement = (SamusMovementType)checkpoint[NativeSnapshotMemory.SamusMovementType];
        int yOffset = -unchecked((sbyte)rom.ReadByte(NativeSnapshotMemory.PoseDefinitions + pose * 8 + 4));
        if (movement == SamusMovementType.Standing)
        {
            if (poseId is SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose)
            {
                if (frame >= 2) yOffset = -1;
            }
            else if (poseId is >= SamusPoseId.NormalLandingRightPose and <= SamusPoseId.SpinLandingLeftPose)
                yOffset = -R(NativeSnapshotMemory.LandingDrawOffsets +
                    (poseId - SamusPoseId.NormalLandingRightPose) * 4 + frame);
        }
        else if (movement == SamusMovementType.PostureTransition &&
            poseId >= SamusPoseId.CrouchingTransitionRightPose && poseId < SamusPoseId.MorphBallGroundLeftPose)
            yOffset = unchecked((sbyte)rom.ReadByte(NativeSnapshotMemory.PostureDrawOffsets +
                (poseId - SamusPoseId.CrouchingTransitionRightPose) * 2 + frame));
        else if (movement == SamusMovementType.Special)
        {
            if (poseId is SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)
                yOffset = unchecked((sbyte)rom.ReadByte(NativeSnapshotMemory.DrainedDrawOffsets + frame));
            else if (poseId is SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose && frame >= 5)
                yOffset = -3;
        }

        bool bottom = movement switch
        {
            SamusMovementType.MorphBallGround or SamusMovementType.UnusedGlitchBall or
                SamusMovementType.MorphBallFalling or SamusMovementType.UnusedGlitchBallAlternate or
                SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                SamusMovementType.SpringBallFalling => false,
            SamusMovementType.SpinJumping => frame == 0 || frame >= 11 ||
                poseId is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose or
                    SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose,
            SamusMovementType.Knockback => frame >= 3 ||
                poseId is not (SamusPoseId.DeathSequenceRightPose or SamusPoseId.DeathSequenceLeftPose),
            SamusMovementType.PostureTransition => poseId >= SamusPoseId.CrouchingTransitionAimUpRightPose ||
                (poseId >= SamusPoseId.UnusedPoseDd ? frame == 2 :
                 poseId >= SamusPoseId.UnusedPoseDb ? frame == 0 :
                 poseId is SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or
                    SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose),
            SamusMovementType.Unused0D => frame == 0 || poseId is not (SamusPoseId.UnusedPose65 or SamusPoseId.UnusedPose66),
            SamusMovementType.WallJumping => frame is < 3 or >= 13,
            SamusMovementType.DamageBoost => frame is < 2 or >= 9,
            SamusMovementType.Special =>
                poseId is not (SamusPoseId.ShinesparkVerticalRightPose or SamusPoseId.ShinesparkVerticalLeftPose) &&
                (frame >= 2 || poseId is not (SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose)),
            _ when (ushort)movement < 28 => true,
            _ => throw new InvalidDataException($"Unknown native body movement type {movement}"),
        };
        return (unchecked((ushort)(R(NativeSnapshotMemory.TopSpritemapBases + pose * 2) + frame)),
            bottom ? unchecked((ushort)(R(NativeSnapshotMemory.BottomSpritemapBases + pose * 2) + frame)) : (ushort)0,
            unchecked((ushort)(W(NativeSnapshotMemory.X) - W(NativeSnapshotMemory.CameraX))),
            unchecked((ushort)(W(NativeSnapshotMemory.Y) + yOffset - W(NativeSnapshotMemory.CameraY))));
    }
}
