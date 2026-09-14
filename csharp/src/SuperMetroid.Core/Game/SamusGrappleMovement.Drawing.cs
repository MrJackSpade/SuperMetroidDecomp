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
        ushort layer1Y,
        Assets.GrappleTileAtlas? artwork = null,
        byte samusPose = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(grapple);
        ArgumentNullException.ThrowIfNull(oam);
        ArgumentNullException.ThrowIfNull(vramWrites);
        if (!UsesBeamSpecificDrawingPath(grapple.Phase))
            return;

        // The native pair is a begin/end range, not two animation frames. Each
        // six-call interval advances the source by $200 through four endpoint tiles.
        GrapplePointAnimationDefinitions.Advance(grapple);
        if (artwork is not null && grapple.PointAnimationFrame < GrapplePointAnimationDefinitions.FrameCount)
            artwork.QueuePoint(vramWrites, grapple.PointAnimationFrame);
        else
        {
            vramWrites.Enqueue(SamusGrappleRomData.Rendering.PointTileByteCount,
                GrapplePointAnimationDefinitions.SourceAddress(grapple.PointAnimationFrame),
                SamusGrappleRomData.Rendering.PointTileVramDestination);
        }

        // Both installed artwork and legacy bus-backed pixels use the same compiled
        // native sector selection. Keeping a second ROM table reader here would make
        // default/debug drawing depend on data that installed drawing no longer needs.
        if (artwork is not null)
            artwork.QueueSegments(vramWrites, grapple.Angle.RawValue);
        else
        {
            var transfer = Assets.GrappleTileDefinitions.TransferFor(
                Assets.GrappleTileDefinitions.SegmentAssetFor(grapple.Angle.RawValue));
            vramWrites.Enqueue(transfer.ByteCount, transfer.SourceAddress,
                Assets.GrappleTileDefinitions.SegmentDestination);
        }

        // `$9B:BFA5` increments the shared flare counter after both tile records, saturating
        // at 120 by a signed comparison. It happens even at zero rope length; only the OAM
        // rope renderer below is conditional. This is why a newly fired beam can animate
        // its muzzle flare before its first eight-pixel body segment exists.
        if (unchecked((short)(grapple.FlareCounter - 120)) < 0)
            grapple.FlareCounter = unchecked((ushort)(grapple.FlareCounter + 1));
        if (unchecked((short)grapple.RopeLength) <= 0)
            return;

        // $94:AFCF recalculates the draw vector from endpoint-minus-flare geometry. This
        // is observably different from merely reversing EndAngle while firing, and it also
        // keeps the rope visually attached after pose-specific art-origin correction.
        int beamDeltaX = unchecked((short)(grapple.AnchorX - grapple.BeamStartX));
        int beamDeltaY = unchecked((short)(grapple.AnchorY - grapple.BeamStartY));
        SnesAngle drawAngle = CalculateAngleFromXY(beamDeltaX, beamDeltaY);
        // Preserve the fractional word across segments. Rounding each individual
        // displacement shortens diagonals and rounds negative steps incorrectly.
        int stepX = ReadSignedSine(drawAngle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex)
            * SamusGrappleRomData.Rendering.SegmentSampleToFixedPoint;
        int stepY = ReadSignedSine(drawAngle.TableIndex)
            * SamusGrappleRomData.Rendering.SegmentSampleToFixedPoint;

        // $94:AFDE derives X/Y flip bits from the grapple angle while retaining the packed
        // palette-five/priority-three instruction word. Tile $20 is the connected endpoint.
        int angleHigh = grapple.Angle.TableIndex;
        int flipBits = (angleHigh & 0x80) >> 1;
        flipBits |= 2 * ((((angleHigh ^ flipBits) & 0x40) ^ 0x40));
        ushort flipAttributes = unchecked((ushort)(flipBits << 8));
        int screenXFixed = unchecked((grapple.BeamStartX - layer1X - SamusGrappleRomData.Rendering.CharacterCenterOffset) << 16);
        int screenYFixed = unchecked((grapple.BeamStartY - layer1Y - SamusGrappleRomData.Rendering.CharacterCenterOffset) << 16);
        // Native enters the loop before decrementing quotient-minus-one: even a
        // positive length whose masked quotient is zero visits one segment.
        int segmentCount = Math.Max(1, (grapple.RopeLength / SamusGrappleRomData.Rendering.SegmentSpacing)
            & SamusGrappleRomData.Rendering.SegmentCountMask);
        for (int segment = 0; segment < segmentCount; segment++)
        {
            // $94:AFBA walks instruction slots from 15 downward regardless of rope length.
            // The first timer expiry reads the already-selected initial record; later
            // expiries advance through $21,$22,$23,$24 and $94:B0F4's goto back to $21.
            // Keeping the one-time first expiry explicit preserves the native five-draw
            // interval without pretending that all rope pieces share one animation frame.
            int instructionSlot = SamusGrappleRomData.Rendering.FirstSegmentSlot - segment;
            if (grapple.SegmentAnimationTimers[instructionSlot]-- == 1)
            {
                grapple.SegmentAnimationTimers[instructionSlot] = SamusGrappleRomData.Rendering.SegmentAnimationDelay;
                if (grapple.SegmentAnimationStarted[instructionSlot])
                {
                    grapple.SegmentAnimationFrames[instructionSlot] = unchecked((byte)(
                        (grapple.SegmentAnimationFrames[instructionSlot] + 1) % SamusGrappleRomData.Rendering.SegmentAnimationFrameCount));
                }
                else
                {
                    grapple.SegmentAnimationStarted[instructionSlot] = true;
                }
            }

            ushort screenX = unchecked((ushort)(screenXFixed >> 16));
            ushort screenY = unchecked((ushort)(screenYFixed >> 16));
            // Native ticks this slot before the visibility check, then abandons
            // the remaining rope slots entirely at the first off-screen segment.
            if (((screenX | screenY) & SamusGrappleRomData.Rendering.SegmentOutsideViewportMask) != 0)
                break;
            ushort appearance = artwork?.Sprites?.Segment(grapple.SegmentAnimationFrames[instructionSlot])
                ?? unchecked((ushort)(SamusGrappleRomData.Rendering.FirstSegmentAttributes + grapple.SegmentAnimationFrames[instructionSlot]));
            ushort segmentAttributes = unchecked((ushort)(appearance | flipAttributes));
            oam.AddRawSmallSprite(
                unchecked((ushort)screenX),
                unchecked((ushort)screenY),
                segmentAttributes);
            screenXFixed = unchecked(screenXFixed + stepX);
            screenYFixed = unchecked(screenYFixed + stepY);
        }

        DrawBeamEndpoint(grapple, oam, layer1X, layer1Y, samusPose,
            artwork?.Sprites?.Endpoint ?? SamusGrappleRomData.Rendering.EndpointAttributes);
    }

    /// <summary>Preserves the pose-selected endpoint routines at $94:B0F9/B14B.</summary>
    private static void DrawBeamEndpoint(SamusGrappleState grapple, OamBuffer oam,
        ushort layer1X, ushort layer1Y, byte samusPose, ushort attributes)
    {
        bool swinging = (SamusPoseId)samusPose is SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose;
        ushort relativeY = unchecked((ushort)(grapple.AnchorY - layer1Y));
        // The ordinary endpoint tests its center before subtracting the sprite
        // half-width. Swinging deliberately bypasses this vertical rejection.
        if (!swinging && (relativeY & SamusGrappleRomData.Rendering.SegmentOutsideViewportMask) != 0)
            return;

        // Only the swinging routine omits SEC between its two SBC operations.
        // Consequently unsigned camera subtraction can carry a borrow into the
        // half-character offset, even when wrapped coordinates look adjacent.
        int borrowX = swinging && grapple.AnchorX < layer1X ? 1 : 0;
        int borrowY = swinging && grapple.AnchorY < layer1Y ? 1 : 0;
        oam.AddRawSmallSprite(
            unchecked((ushort)(grapple.AnchorX - layer1X - SamusGrappleRomData.Rendering.CharacterCenterOffset - borrowX)),
            unchecked((ushort)(relativeY - SamusGrappleRomData.Rendering.CharacterCenterOffset - borrowY)),
            attributes);
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
        // The held route takes $90:F41E's no-op, not a connected-pose collision exit.
        // Draygon, rather than terrain ejection, still owns Samus's coordinates.
        if (!samus.DraygonGrabbed.IsActive)
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
        grapple.PoseChangeAutoFireTimer = 0;
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
