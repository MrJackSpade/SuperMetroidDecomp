using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Connected-beam rendering and completion of queued firing cancellation.
/// </summary>
public static partial class SamusGrappleMovement
{
    /// <summary>
    /// Ports the post-Samus tile-upload and small-OBJ portion of
    /// `$90:EB86/$9B:BFA5/$94:AFBA`.
    /// </summary>
    public static void DrawConnectedBeam(
        ISnesAddressSpace bus,
        SamusGrappleState grapple,
        OamBuffer oam,
        VramWriteQueue vramWrites,
        ushort layer1X,
        ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(grapple);
        ArgumentNullException.ThrowIfNull(oam);
        ArgumentNullException.ThrowIfNull(vramWrites);
        if (!UsesBeamSpecificDrawingPath(grapple.Phase))
            return;

        // $9B:BFBD alternates the 32-byte grapple-point tile every six calls (timer five
        // decrements through zero and wraps). The two pointers live in bank $9B but name
        // graphics in bank $9A, exactly as the seven-byte VRAM queue record does.
        if (grapple.PointAnimationTimer == 0)
        {
            grapple.PointAnimationTimer = 5;
            grapple.PointAnimationFrame ^= 1;
        }
        else
        {
            grapple.PointAnimationTimer--;
        }
        ushort pointPointer = ReadWord(
            bus,
            SamusGrappleRomData.Rendering.PointTilePointers +
                grapple.PointAnimationFrame * 2);
        vramWrites.Enqueue(
            SamusGrappleRomData.Rendering.PointTileByteCount,
            SamusGrappleRomData.Banks.CharacterData | pointPointer,
            SamusGrappleRomData.Rendering.PointTileVramDestination);

        // The 128-byte rope body source is selected by the same folded angle byte used in
        // $9B:BFFD. Horizontal, diagonal, and vertical source blocks therefore remain ROM
        // policy rather than a host renderer choosing a plausible rotated sprite.
        // $9B:C005 takes the high angle byte, shifts it right once, then clears bit zero.
        // The result is already an even byte offset into the word table. Multiplying that
        // value by two again selects unrelated data for half the angles (notably firing
        // right at $C000), producing a correctly positioned but visually blank rope.
        int foldedAngleOffset = (grapple.Angle.RawValue >> 9) & 0xfe;
        ushort segmentPointer = ReadWord(
            bus,
            SamusGrappleRomData.Rendering.SegmentTilePointers + foldedAngleOffset);
        vramWrites.Enqueue(
            SamusGrappleRomData.Rendering.SegmentTileByteCount,
            SamusGrappleRomData.Banks.CharacterData | segmentPointer,
            SamusGrappleRomData.Rendering.SegmentTileVramDestination);

        // `$9B:BFA5` increments the shared flare counter after both tile records, saturating
        // at 120 by a signed comparison. It happens even at zero rope length; only the OAM
        // rope renderer below is conditional. This is why a newly fired beam can animate
        // its muzzle flare before its first eight-pixel body segment exists.
        if (unchecked((short)(grapple.FlareCounter - 120)) < 0)
            grapple.FlareCounter = unchecked((ushort)(grapple.FlareCounter + 1));
        if (grapple.RopeLength == 0)
            return;

        // $94:AFCF recalculates the draw vector from endpoint-minus-flare geometry. This
        // is observably different from merely reversing EndAngle while firing, and it also
        // keeps the rope visually attached after pose-specific art-origin correction.
        int beamDeltaX = unchecked((short)(grapple.AnchorX - grapple.BeamStartX));
        int beamDeltaY = unchecked((short)(grapple.AnchorY - grapple.BeamStartY));
        SnesAngle drawAngle = CalculateAngleFromXY(beamDeltaX, beamDeltaY);
        int stepX = ScaleCoordinate(
            ReadSignedSine(bus, drawAngle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex),
            8);
        int stepY = ScaleCoordinate(ReadSignedSine(bus, drawAngle.TableIndex), 8);

        // $94:AFDE derives X/Y flip bits from the grapple angle while retaining the packed
        // palette-five/priority-three instruction word. Tile $20 is the connected endpoint.
        int angleHigh = grapple.Angle.TableIndex;
        int flipBits = (angleHigh & 0x80) >> 1;
        flipBits |= 2 * ((((angleHigh ^ flipBits) & 0x40) ^ 0x40));
        ushort flipAttributes = unchecked((ushort)(flipBits << 8));
        int screenX = unchecked((short)(grapple.BeamStartX - layer1X)) - 4;
        int screenY = unchecked((short)(grapple.BeamStartY - layer1Y)) - 4;
        int segmentCount = (grapple.RopeLength / 8) & 0x0f;
        for (int segment = 0; segment < segmentCount; segment++)
        {
            // $94:AFBA walks instruction slots from 15 downward regardless of rope length.
            // The first timer expiry reads the already-selected initial record; later
            // expiries advance through $21,$22,$23,$24 and $94:B0F4's goto back to $21.
            // Keeping the one-time first expiry explicit preserves the native six-draw
            // interval without pretending that all rope pieces share one animation frame.
            int instructionSlot = 15 - segment;
            if (grapple.SegmentAnimationTimers[instructionSlot]-- == 1)
            {
                grapple.SegmentAnimationTimers[instructionSlot] = 5;
                if (grapple.SegmentAnimationStarted[instructionSlot])
                {
                    grapple.SegmentAnimationFrames[instructionSlot] = unchecked((byte)(
                        (grapple.SegmentAnimationFrames[instructionSlot] + 1) & 3));
                }
                else
                {
                    grapple.SegmentAnimationStarted[instructionSlot] = true;
                }
            }

            ushort segmentAttributes = unchecked((ushort)(
                0x3a21 + grapple.SegmentAnimationFrames[instructionSlot] | flipAttributes));
            oam.AddRawSmallSprite(
                unchecked((ushort)screenX),
                unchecked((ushort)screenY),
                segmentAttributes);
            screenX += stepX;
            screenY += stepY;
        }

        oam.AddRawSmallSprite(
            unchecked((ushort)(grapple.AnchorX - layer1X - 4)),
            unchecked((ushort)(grapple.AnchorY - layer1Y - 4)),
            0x3a20);
    }

    /// <summary>
    /// Completes the one-frame cancellation function installed by firing. The native
    /// routine also clears palette/sound bookkeeping not yet modeled by this runtime;
    /// every gameplay-visible firing word owned by this class is cleared here.
    /// </summary>
    public static GrappleMovementResult CompleteFiringCancellation(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(samus);
        SamusGrappleState grapple = samus.Grapple;
        if (grapple.Phase != GrapplePhase.CancelPending)
            throw new InvalidOperationException("A grapple firing cancellation is not queued.");

        bool cancelledConnectedPose = grapple.CancelFromConnectedPose;
        QueueGrappleSound(samus, SamusGrappleRomData.Sounds.Stop);
        SamusBlockCollision.EjectAfterGrapple(bus, level, samus.Kinematics);
        if (cancelledConnectedPose)
        {
            // `$9B:C856` calls `$91:82D9` while the current movement type is `$16`.
            // Its command-six table entry kills all X/run momentum and the pose-definition
            // byte selects the exact standing/crouching body that existed before locking.
            byte fallback = samus.ReadNoInputFallbackPose(bus);
            if (fallback == 0xff)
            {
                throw new InvalidDataException(
                    $"Connected grapple pose ${samus.Pose:X2} has no cancellation fallback.");
            }

            samus.Pose = fallback;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus, initialFrame: 0);
            samus.HorizontalSpeed.AccelerationMode = 0;
            samus.HorizontalSpeed.BaseSpeed = 0;
            samus.HorizontalSpeed.BaseSubspeed = 0;
            samus.HorizontalSpeed.ExtraRunSpeed = 0;
            samus.HorizontalSpeed.ExtraRunSubspeed = 0;
        }

        grapple.Phase = GrapplePhase.Inactive;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = 0;
        grapple.ExtensionXVelocity = 0;
        grapple.ExtensionYVelocity = 0;
        grapple.EndpointXOffsetFixed = 0;
        grapple.EndpointYOffsetFixed = 0;
        grapple.CancelFromConnectedPose = false;
        ClearFlareAnimation(grapple);
        return new GrappleMovementResult(
            GrapplePhase.Inactive,
            Released: false,
            ReleaseQueued: false,
            Fired: false,
            Connected: false,
            CancelQueued: false,
            Cancelled: true,
            OwnsMovement: cancelledConnectedPose,
            LockedInPlace: cancelledConnectedPose);
    }

}
