using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Firing cancellation, endpoint block reactions, accepted connections, and beam geometry.
/// </summary>
public static partial class SamusGrappleMovement
{
    private static GrappleMovementResult QueueFiringCancellation(SamusGrappleState grapple)
    {
        grapple.CancelFromConnectedPose = false;
        grapple.Phase = GrapplePhase.CancelPending;
        return new GrappleMovementResult(
            grapple.Phase,
            Released: false,
            ReleaseQueued: false,
            Fired: false,
            Connected: false,
            CancelQueued: true,
            Cancelled: false,
            OwnsMovement: false);
    }

    private static void PublishFiringGeometry(SamusState samus, SamusGrappleState grapple)
    {
        // Whole endpoint offsets are the signed upper words of the two 16.16 accumulators.
        // Arithmetic shift is intentional: a left/up beam must retain sign after fractional
        // accumulation, exactly like reading `$0DCE/$0DD2` as signed displacement words.
        int endpointOffsetX = grapple.EndpointXOffsetFixed >> 16;
        int endpointOffsetY = grapple.EndpointYOffsetFixed >> 16;
        grapple.RopeStartX = unchecked((ushort)(samus.XPosition + grapple.OriginXOffset));
        grapple.RopeStartY = unchecked((ushort)(samus.YPosition + grapple.OriginYOffset));
        grapple.AnchorX = unchecked((ushort)(
            grapple.RopeStartX + endpointOffsetX));
        grapple.AnchorY = unchecked((ushort)(
            grapple.RopeStartY + endpointOffsetY));
        grapple.BeamStartX = unchecked((ushort)(samus.XPosition + grapple.FlareXOffset));
        grapple.BeamStartY = unchecked((ushort)(samus.YPosition + grapple.FlareYOffset));
    }

    private static GrappleBlockReaction ReactAtEndpoint(
        RoomLevelData level,
        SamusState samus,
        ushort endpointX,
        ushort endpointY,
        RoomPlmSystem? plms = null)
    {
        int blockX = endpointX >> 4;
        int blockY = endpointY >> 4;
        RoomCollisionBlock initialBlock =
            level.GetCollisionBlockOrPrefilledSolid(blockX, blockY);
        if (initialBlock.Index < 0)
        {
            // The prefilled `$8000` word dispatches as an ordinary solid endpoint: carry
            // set, overflow clear, which queues grapple cancellation rather than connecting.
            return new GrappleBlockReaction(Carry: true, Overflow: false);
        }

        int index = initialBlock.Index;
        for (int extensionDepth = 0; extensionDepth <= 16; extensionDepth++)
        {
            RoomCollisionBlock block =
                level.GetCollisionBlockByIndexOrPrefilledSolid(index);
            int resolvedX = block.Index < 0 ? -1 : block.Index % level.WidthInBlocks;
            int resolvedY = block.Index < 0 ? -1 : block.Index / level.WidthInBlocks;
            switch (block.CollisionType)
            {
                // These four dispatcher entries return clear carry: the beam remains live.
                case 0:
                case 2:
                case 3:
                case 6:
                    return new GrappleBlockReaction(Carry: false, Overflow: false);

                // Slopes and these solid-family entries return carry with overflow clear,
                // which makes $9B:C703 queue cancellation rather than establish a rope.
                case 1:
                case 8:
                case 9:
                case 0x0b:
                    return new GrappleBlockReaction(Carry: true, Overflow: false);

                case 0x0a:
                    // `$94:A7FD` selects a bank-$84 grapple-reaction PLM by the low seven
                    // BTS bits. A negative BTS rejects the beam. Every ordinary entry uses
                    // `$84:CFD1` (carry set, overflow clear) and immediately deletes; BTS
                    // three alone uses Draygon's broken-turret setup `$84:CFD5`, which adds
                    // one whole periodic-damage unit and returns carry+overflow to connect.
                    if ((block.Behavior & 0x80) != 0)
                        return new GrappleBlockReaction(Carry: false, Overflow: false);
                    if (block.Behavior == 3)
                    {
                        samus.LiquidPhysics.AccumulatePeriodicDamage(
                            subDamage: 0,
                            wholeDamage: 1);
                        return new GrappleBlockReaction(Carry: true, Overflow: true);
                    }

                    return new GrappleBlockReaction(Carry: true, Overflow: false);

                case 5:
                    // Horizontal extensions interpret BTS as a signed block-index delta.
                    // BTS zero is the native terminator and behaves as air.
                    if (block.Behavior == 0)
                        return new GrappleBlockReaction(Carry: false, Overflow: false);
                    index += unchecked((sbyte)block.Behavior);
                    continue;

                case 0x0d:
                    // Vertical extension uses the same signed BTS but scales by room width.
                    if (block.Behavior == 0)
                        return new GrappleBlockReaction(Carry: false, Overflow: false);
                    index += unchecked((sbyte)block.Behavior) * level.WidthInBlocks;
                    continue;

                case 0x0e:
                    // $94:A7D1 rejects bit-seven BTS. Values zero and three spawn persistent
                    // grapple PLM $D0D8, whose setup returns processor flags $41 (C=1,V=1).
                    // The persistent PLM has no level mutation, so this result is complete.
                    if ((block.Behavior & 0x80) != 0)
                        return new GrappleBlockReaction(Carry: false, Overflow: false);
                    if (block.Behavior is 0 or 3)
                        return new GrappleBlockReaction(Carry: true, Overflow: true);

                    // BTS one/two spawn $D0DC/$D0E0. Setup_CFB5 synchronously saves the
                    // complete level word and clears BTS before returning C+V; the new PLM
                    // executes its first timer/draw record later in this same gameplay frame.
                    // A caller which omitted the independent room owner cannot honestly
                    // preserve that lifecycle, so keep the missing integration seam explicit.
                    if (block.Behavior is 1 or 2)
                    {
                        if (plms is null)
                        {
                            throw new InvalidOperationException(
                                "Breakable grapple acquisition requires a RoomPlmSystem.");
                        }
                        if (!plms.TrySpawnBreakableGrappleBlock(level, block.Index, block.Behavior))
                        {
                            throw new InvalidOperationException(
                                "All 40 native PLM slots are occupied during grapple acquisition.");
                        }

                        return new GrappleBlockReaction(Carry: true, Overflow: true);
                    }

                    // Nonnegative BTS values beyond the four authored grapple reactions
                    // would index outside $D0D8-$D0E0 in the native selection table.
                    throw new InvalidDataException(
                        $"Invalid grapple block BTS ${block.Behavior:X2} at ({resolvedX},{resolvedY}).");

                case 4:
                case 0x0c:
                    // `$94:9E55/$9E73` use the same shootable reaction table as ordinary
                    // projectiles. Grapple owns no projectile family, so weapon-gated
                    // entries reject it while BTS 0..7 retain their unconditional block
                    // animation. The collision nibble—not PLM setup—owns carry: type 4 is
                    // air and type C is solid.
                    if (plms is null)
                    {
                        throw new InvalidOperationException(
                            "Shootable grapple reaction requires a RoomPlmSystem.");
                    }
                    plms.TrySpawnProjectileShotBlock(
                        level,
                        block.Index,
                        block.Behavior,
                        projectileType: 0,
                        solidBlock: block.CollisionType == 0x0c);
                    return new GrappleBlockReaction(
                        Carry: block.CollisionType == 0x0c,
                        Overflow: false);

                case 7:
                    // Bombable-air setup `$84:CEDA` immediately deletes its provisional
                    // PLM for grapple's zero projectile family. Carry remains clear.
                    return new GrappleBlockReaction(Carry: false, Overflow: false);

                case 0x0f:
                    // The solid twin performs the same rejected setup but independently
                    // returns carry set from `$94:9FF4`.
                    return new GrappleBlockReaction(Carry: true, Overflow: false);

                default:
                    throw new InvalidDataException(
                        $"Grapple block type ${block.CollisionType:X1} escaped the complete " +
                        $"sixteen-entry dispatcher at ({resolvedX},{resolvedY}).");
            }
        }

        throw new InvalidDataException("Grapple extension chain exceeded sixteen blocks.");
    }

    private static GrappleMovementResult ConnectAcceptedFiring(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple,
        ushort previousXPosition,
        ushort previousYPosition,
        bool validateAnchorBlock = true,
        bool validateAnchorEnemy = false)
    {
        // Movement type $1A is the Draygon-held actor route at $9B:B98C. It bypasses all
        // three direction tables and depends on untranslated enemy ownership/positioning.
        // A room-block connection should never normally arrive here in that pose, but an
        // explicit boundary is safer than fabricating either of the ordinary routes.
        byte sourceMovementType = samus.ReadMovementType(bus);
        if ((SamusMovementType)sourceMovementType == SamusMovementType.DraygonHeld)
        {
            throw new InvalidOperationException(
                "A room-block grapple connection cannot be installed while Draygon owns Samus.");
        }

        bool movingVertically =
            samus.Kinematics.YSpeed != 0 || samus.Kinematics.YSubspeed != 0;
        int connectionTable = movingVertically
            ? MovingVerticallyConnectionTable
            : sourceMovementType == 5
                ? CrouchingConnectionTable
                : DefaultConnectionTable;
        int recordAddress = connectionTable + grapple.FireDirection * 4;
        ushort nextFunction = ReadWord(bus, recordAddress);
        ushort handler = ReadWord(bus, recordAddress + 2);

        // Each tiny native handler installs one prospective type-$16 pose and then jumps
        // to either BA61 (swinging) or BA9B (stuck). Keep the handler addresses visible:
        // pose alone is insufficient to distinguish malformed table data from retail data.
        (byte pose, bool swinging) = handler switch
        {
            ConnectSwingClockwiseHandler => (SamusState.GrappleSwingRightPose, true),
            ConnectSwingAnticlockwiseHandler => (SamusState.GrappleSwingLeftPose, true),
            ConnectStandingUpRightHandler => ((byte)0xa8, false),
            ConnectStandingRightHandler => ((byte)0xaa, false),
            ConnectStandingDownHandler => ((byte)0xab, false),
            ConnectStandingUpLeftHandler => ((byte)0xa9, false),
            ConnectCrouchingUpRightHandler => ((byte)0xb4, false),
            ConnectCrouchingRightHandler => ((byte)0xb6, false),
            ConnectCrouchingDownLeftHandler => ((byte)0xb7, false),
            ConnectCrouchingUpLeftHandler => ((byte)0xb5, false),
            _ => throw new InvalidDataException(
                $"Grapple connection direction {grapple.FireDirection} names unknown handler ${handler:X4}."),
        };
        ushort expectedFunction = swinging ? SwingingFunction : LockedInPlaceFunction;
        if (nextFunction != expectedFunction)
        {
            throw new InvalidDataException(
                $"Grapple connection handler ${handler:X4} requires function ${expectedFunction:X4}, " +
                $"but the ROM record names ${nextFunction:X4}.");
        }

        // BA61 and BA9B both derive the angle from the body position that fired the beam,
        // before command 9/10 changes pose and snaps Samus to the rope. The angle is stored
        // as an integer byte in the high half of the native word; its fraction becomes zero.
        int deltaX = unchecked((short)(samus.XPosition - grapple.AnchorX));
        int deltaY = unchecked((short)(samus.YPosition - grapple.AnchorY));
        byte angleByte = CalculateAngleFromXY(deltaX, deltaY);
        grapple.Angle = unchecked((ushort)(angleByte << 8));
        grapple.MirroredAngle = grapple.Angle;
        grapple.AngularVelocity = 0;
        grapple.RopeLengthDelta = 0;
        if (grapple.RopeLength >= 64)
            grapple.RopeLength = unchecked((ushort)(grapple.RopeLength - 24));

        // $94:AC11 publishes the native grapple-beam Start pair from the accepted endpoint,
        // angle, and possibly shortened rope. This is the hand attachment point, not the
        // independently authored Flare pair from the firing tables.
        GrappleCollisionPoint ropeStart = CalculateCollisionPoint(
            bus,
            grapple,
            angleByte,
            grapple.RopeLength);
        grapple.RopeStartX = ropeStart.X;
        grapple.RopeStartY = ropeStart.Y;

        samus.Pose = pose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);

        if (swinging)
        {
            // Special pose command 9 runs $9B:BD95. Swinging copies Start into Flare, then
            // chooses the angle-authored animation frame and body offsets.
            grapple.Phase = GrapplePhase.ConnectedSwinging;
            PositionSamusFromPendulum(bus, samus, grapple);
        }
        else
        {
            // Special pose command 10 runs $9B:BEEB. The locked body is positioned from
            // Start minus the raw NO-RUN origin table, then Flare is independently rebuilt
            // from the raw no-run flare table. Graphics-Y correction does not participate.
            int tableOffset = grapple.FireDirection * 2;
            short originX = unchecked((short)ReadWord(bus, NoRunOriginXTable + tableOffset));
            short originY = unchecked((short)ReadWord(bus, NoRunOriginYTable + tableOffset));
            short flareX = unchecked((short)ReadWord(bus, NoRunFlareXTable + tableOffset));
            short flareY = unchecked((short)ReadWord(bus, NoRunFlareYTable + tableOffset));
            samus.XPosition = unchecked((ushort)(grapple.RopeStartX - originX));
            samus.YPosition = unchecked((ushort)(grapple.RopeStartY - originY));
            grapple.BeamStartX = unchecked((ushort)(samus.XPosition + flareX));
            grapple.BeamStartY = unchecked((ushort)(samus.YPosition + flareY));
            grapple.Phase = GrapplePhase.ConnectedLocked;
        }

        // $91:EF53 is shared by connection commands 9 and 10. Speed-booster bookkeeping
        // has no host field yet, but every modeled speed word cleared there is reset here.
        // AccelerationMode is deliberately retained: the native common tail does not write it.
        samus.HorizontalSpeed.BaseSpeed = 0;
        samus.HorizontalSpeed.BaseSubspeed = 0;
        samus.HorizontalSpeed.ExtraRunSpeed = 0;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;

        // Block and enemy acquisition share the same connection/pose machinery, but their
        // following-frame validators have different owners. Keep those origins mutually
        // exclusive so a moving Powamp anchor is never reinterpreted as room terrain.
        if (validateAnchorBlock == validateAnchorEnemy)
        {
            throw new ArgumentException(
                "A live grapple connection must have exactly one anchor validator.");
        }
        grapple.ValidateAnchorBlock = validateAnchorBlock;
        grapple.ValidateAnchorEnemy = validateAnchorEnemy;
        grapple.SpecialAngleHandling = false;
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;

        ushort cameraPreviousX = ClampPreviousPosition(samus.XPosition, previousXPosition);
        ushort cameraPreviousY = ClampPreviousPosition(samus.YPosition, previousYPosition);
        return new GrappleMovementResult(
            grapple.Phase,
            Released: false,
            ReleaseQueued: false,
            Connected: true,
            OwnsMovement: true,
            LockedInPlace: !swinging,
            CameraPreviousX: cameraPreviousX,
            CameraPreviousY: cameraPreviousY);
    }

    /// <summary>
    /// Integer octant translation of <c>CalculateAngleFromXY</c> at <c>$A0:C0B1</c>.
    /// The return byte is measured clockwise from negative Y: $00 up, $40 right, $80 down,
    /// and $C0 left. Division truncates exactly as the SNES unsigned divide registers do.
    /// </summary>
    internal static byte CalculateAngleFromXY(int x, int y)
    {
        bool xNegative = x < 0;
        bool yNegative = y < 0;
        int absoluteX = Math.Abs(x);
        int absoluteY = Math.Abs(y);
        if (absoluteX == 0 && absoluteY == 0)
            return 0;

        // The assembly advances its pointer byte offset by four for negative X and two
        // for negative Y, then indexes a word table. Converted to a zero-based entry index,
        // those are bit one for X and bit zero for Y—not the more tempting reverse order.
        int quadrant = (xNegative ? 2 : 0) | (yNegative ? 1 : 0);
        if (absoluteY < absoluteX)
        {
            int eighths = ((absoluteY << 8) / absoluteX) >> 3;
            return quadrant switch
            {
                0 => unchecked((byte)(eighths + 0x40)),
                1 => unchecked((byte)(0x40 - eighths)),
                2 => unchecked((byte)(0xc0 - eighths)),
                _ => unchecked((byte)(0xc0 + eighths)),
            };
        }

        int reciprocalEighths = ((absoluteX << 8) / absoluteY) >> 3;
        return quadrant switch
        {
            0 => unchecked((byte)(0x80 - reciprocalEighths)),
            1 => unchecked((byte)reciprocalEighths),
            2 => unchecked((byte)(reciprocalEighths + 0x80)),
            _ => unchecked((byte)(0x100 - reciprocalEighths)),
        };
    }

    private static void InitializeBeamAnimation(SamusGrappleState grapple)
    {
        // `$9B:C51E` installs counter one after initializing the rope instruction slots.
        // The debugger's already-connected seam has no preceding firing history, so it
        // begins at the same authentic first flare state rather than inventing a static OBJ.
        grapple.FlareCounter = 1;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = 0;
        grapple.PointAnimationTimer = 5;
        grapple.PointAnimationFrame = 0;
        // GrappleFunc_AF87 at $94:AF87 seeds sixteen independent segment instruction
        // slots. Slot 15 starts on tile $24, slot 14 on $23, slot 13 on $22, slot 12 on
        // $21, and that four-phase pattern repeats down to zero. Timer one makes $94:AFBA
        // consume each initial record on its first draw.
        for (int slot = 0; slot < grapple.SegmentAnimationFrames.Length; slot++)
        {
            grapple.SegmentAnimationTimers[slot] = 1;
            grapple.SegmentAnimationFrames[slot] = unchecked((byte)(slot & 3));
            grapple.SegmentAnimationStarted[slot] = false;
        }
    }

    private static void RefreshFiringDrawOrigins(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        int tableOffset = grapple.FireDirection * 2;
        bool useRunOffsets = samus.ReadMovementKind(bus) == SamusMovementType.Running;
        int originXTable = useRunOffsets ? RunOriginXTable : NoRunOriginXTable;
        int originYTable = useRunOffsets ? RunOriginYTable : NoRunOriginYTable;
        int flareXTable = useRunOffsets ? RunFlareXTable : NoRunFlareXTable;
        int flareYTable = useRunOffsets ? RunFlareYTable : NoRunFlareYTable;
        sbyte graphicsYOffset = samus.ReadGraphicsYOffset(bus);

        // `$9B:BF1B` rereads ROM instead of trusting `$0D0A/$0D0C`, then publishes Start
        // and previous-frame/Flare as separate coordinate pairs. Retain the same behavior
        // so a debugger edit to the offset tables is visible immediately in presentation.
        grapple.RopeStartX = unchecked((ushort)(
            samus.XPosition + (short)ReadWord(bus, originXTable + tableOffset)));
        grapple.RopeStartY = unchecked((ushort)(
            samus.YPosition + (short)ReadWord(bus, originYTable + tableOffset) -
            graphicsYOffset));
        grapple.BeamStartX = unchecked((ushort)(
            samus.XPosition + (short)ReadWord(bus, flareXTable + tableOffset)));
        grapple.BeamStartY = unchecked((ushort)(
            samus.YPosition + (short)ReadWord(bus, flareYTable + tableOffset) -
            graphicsYOffset));
    }

}
