using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How the grapple beam draws: its flare, the connected beam's segments and its endpoint.</summary>
public static class SamusGrappleBeamDrawing
{

    /// <summary>
    /// Reports whether grapple's replacement Samus draw handler is still installed.
    /// </summary>
    /// <remarks>
    /// Cancellation, release, drop, and grapple wall-jump are one-call teardown functions.
    /// On the frame that installs one of them, the grapple draw handler is still the selected handler even
    /// though its signed function-range test falls back to the ordinary body/echo path.
    /// Keeping that distinction explicit prevents the normal charge flare from leaking into
    /// the teardown frame; the ordinary draw handler, which owns that flare, has not been restored yet.
    /// </remarks>
    public static bool UsesGrappleDrawingHandler(GrapplePhase phase) =>
        phase != GrapplePhase.Inactive;


    /// <summary>
    /// Mirrors the grapple draw handler's signed function-range test for the beam-specific path.
    /// </summary>
    public static bool UsesBeamSpecificDrawingPath(GrapplePhase phase) => phase is
        GrapplePhase.Firing or
        GrapplePhase.ConnectedSwinging or
        GrapplePhase.ConnectedLocked or
        GrapplePhase.WallGrab or
        GrapplePhase.WallGrabRelease;


    /// <summary>
    /// Draws the grapple beam's flare. Call before atmosphere and Samus,
    /// exactly where the active half of the grapple draw handler calls it.
    /// </summary>
    /// <returns>True when the flare is drawn: the grapple is firing or connected with a flare to show.</returns>
    public static bool DrawFlareBeforeSamus(
        SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork objects)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(sprites);

        SamusGrappleState grapple = samus.Grapple;
        if (!UsesBeamSpecificDrawingPath(grapple.Phase) || grapple.FlareStage == GrappleFlareStage.None)
            return false;

        // While firing, the grapple draw handler recomputes the hand origins immediately before the flare. Grapple
        // physics ran before ordinary Samus movement, so the renderer must reconstruct both
        // hand origins from the final body position or a running shot visibly lags one frame.
        // Connected functions already publish their exact rope/flare pair during movement.
        if (grapple.Phase == GrapplePhase.Firing)
            RefreshFiringDrawOrigins(samus, grapple);

        // A just-fired flare restarts on its opening frame and delay on every such call, then
        // performs the usual decrement and negative test. The first visible frame therefore stores two.
        if (grapple.FlareStage == GrappleFlareStage.JustFired)
        {
            grapple.FlareAnimationFrame = ChargeFlareAnimationDefinitions.GrappleInitialFrame;
            grapple.FlareAnimationTimer = WrappingCountdown.Start(ChargeFlareAnimationDefinitions.GrappleInitialDelay);
        }

        grapple.FlareAnimationTimer = grapple.FlareAnimationTimer.Decrement().Remaining;
        if (grapple.FlareAnimationTimer.IsNegative)
        {
            grapple.FlareAnimationFrame = StepFlareFrame(grapple.FlareAnimationFrame, 1);
            int delay = ChargeFlareAnimations.ReadMainFlareDelay(grapple.FlareAnimationFrame);
            if (ChargeFlareAnimations.TryDecodeCommand(delay, out ChargeFlareDelayCommand command) &&
                command == ChargeFlareDelayCommand.Rewind)
            {
                // The rewind command is the compact loop of the shared delay stream. The
                // subtraction applies to the already-incremented frame.
                int rewind = ChargeFlareAnimations.ReadMainFlareDelay(StepFlareFrame(grapple.FlareAnimationFrame, 1));
                grapple.FlareAnimationFrame = StepFlareFrame(grapple.FlareAnimationFrame, -rewind);
                delay = ChargeFlareAnimations.ReadMainFlareDelay(grapple.FlareAnimationFrame);
            }

            grapple.FlareAnimationTimer = WrappingCountdown.FromRaw(delay);
        }

        int screenX = CoordinateAxis.Wrap(grapple.BeamStart.X - cameraPosition.X);
        int screenY = CoordinateAxis.Wrap(grapple.BeamStart.Y - cameraPosition.Y);
        // The flare is published wherever it lies; the display clips it to the view (#1541).

        // The first word of both facing rows is zero. Grapple uses the
        // shared main flare only; charge-spark direction offsets do not apply.
        int tableIndex = CoordinateAxis.Wrap(
            grapple.FlareAnimationFrame + SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteDefinitions.MainFlareSelectorOffset);
        // The flare is Samus's own, so it shows the sheet of the beams she has equipped (#1586).
        if (!SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteSelection.Contains(tableIndex))
            throw new InvalidDataException($"Unknown charge-flare sprite selector {tableIndex}.");
        SpriteSheetFrame image = objects.SheetOf(SamusBeamCombinations.Of(samus.EquippedBeams)).Frame(SuperMetroid.Core.Samus.Artwork.ProjectileSheetFrames.FlareFrame(
            SuperMetroid.Core.Samus.Artwork.ChargeFlareSpriteSelection.Selectors[tableIndex]));
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusGrappleFlare), image,
            Position16<ScreenPixels>.Of(screenX, screenY), SpriteMirror.None));
        return true;
    }


    /// <summary>Seeds the first flare and point frames and all sixteen independently timed rope segment slots.</summary>
    internal static void InitializeBeamAnimation(SamusGrappleState grapple)
    {
        // Firing marks the flare just fired after initializing the rope animation slots.
        // A beam that starts already connected has no preceding firing history, so it
        // begins at the same first flare state rather than inventing a static flare.
        grapple.FlareStage = GrappleFlareStage.JustFired;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = WrappingCountdown.Start(0);
        grapple.PointAnimationTimer = WrappingCountdown.Start(GrapplePointAnimationDefinitions.Delay);
        grapple.PointAnimationFrame = 0;
        // Firing seeds sixteen independent segment instruction
        // slots. Slot 15 starts on the fourth rope tile, slot 14 on the third, slot 13 on the second, slot 12 on
        // the first, and that four-phase pattern repeats down to zero. Timer one makes the rope draw
        // consume each initial record on its first draw.
        for (int slot = 0; slot < grapple.SegmentAnimationFrames.Length; slot++)
        {
            grapple.SegmentAnimationTimers[slot] = WrappingCountdown.Start(1);
            grapple.SegmentAnimationFrames[slot] = slot & 3;
            grapple.SegmentAnimationStarted[slot] = false;
        }
    }


    /// <summary>Recomputes visual beam origins from the final pose while retaining the separate physical endpoint offset.</summary>
    internal static void RefreshFiringDrawOrigins(
        SamusState samus,
        SamusGrappleState grapple)
    {
        bool useRunOffsets = SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Running;
        var origin = GrappleFiring.Origin(grapple.FireDirection, useRunOffsets);
        var flare = SamusGrappleMovement.ReadFlareOrigin(grapple, grapple.FireDirection, useRunOffsets);
        int graphicsYOffset = SamusPoseMetadata.ReadGraphicsYOffset(samus);
        int physicalYOffset = SamusPoseProjectileOrigins.ReadYOffset(samus.Pose);

        // Recompute from the final pose/position, not cached launch offsets. Physical
        // Start and presentation Flare remain separate coordinate pairs.
        grapple.RopeStart = samus.Position.Whole + new Offset16<RoomPixels>(
            origin.X, CoordinateAxis.Difference(origin.Y, physicalYOffset));
        grapple.BeamStart = samus.Position.Whole + new Offset16<RoomPixels>(
            flare.X, CoordinateAxis.Difference(flare.Y, graphicsYOffset));
    }


    /// <summary>
    /// Moves the main-flare stream position by <paramref name="delta"/>. The authored rewinds stay inside the main
    /// flare's program, and a position outside it fails its delay read.
    /// </summary>
    private static int StepFlareFrame(int frame, int delta) => frame + delta;


    /// <summary>
    /// Ports the post-Samus tile-selection and small-sprite portion of the grapple draw: the endpoint
    /// animation and rope orientation select the beam's characters, which its draws bind.
    /// </summary>
    public static void DrawConnectedBeam(
        SamusGrappleState grapple,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        SpriteSheet? sheet = null,
        SamusPoseId samusPose = 0)
    {
        Ensure.NotNull(grapple);
        Ensure.NotNull(sprites);
        if (!UsesBeamSpecificDrawingPath(grapple.Phase))
            return;

        // The pair is a begin/end range, not two animation frames. Each
        // six-call interval advances the source by 512 through four endpoint tiles.
        GrapplePointAnimation.Advance(grapple);
        // The game loads the endpoint frame and the four rope characters for the angle's sector into
        // fixed sprite characters; the draws below bind that selection over the standard sprite page.
        SpriteSheet installed = sheet ?? throw new InvalidOperationException("The grapple beam requires the installed grapple sheet.");
        SuperMetroid.Core.Samus.Artwork.GrappleRopeOrientation orientation =
            SuperMetroid.Core.Samus.Artwork.GrappleRopeOrientations.For(grapple.Angle);

        // The rope tile selection moves the flare on a stage after both tile records. It happens even
        // at zero rope length; only the rope renderer below is conditional. This is why a newly fired
        // beam can animate its muzzle flare before its first eight-pixel body segment exists.
        grapple.FlareStage = grapple.FlareStage switch
        {
            GrappleFlareStage.None => GrappleFlareStage.JustFired,
            GrappleFlareStage.JustFired or GrappleFlareStage.Shown => GrappleFlareStage.Shown,
            _ => throw new InvalidOperationException($"Undefined {nameof(GrappleFlareStage)} {grapple.FlareStage}."),
        };
        if (grapple.RopeLength <= 0)
            return;

        // The rope draw recalculates the draw vector from endpoint-minus-flare geometry. This
        // is observably different from merely reversing EndAngle while firing, and it also
        // keeps the rope visually attached after pose-specific art-origin correction.
        RotationAngle drawAngle = SamusGrappleMovement.CalculateAngleFromXY(grapple.Anchor - grapple.BeamStart);
        // Preserve the fractional word across segments. Rounding each individual
        // displacement shortens diagonals and rounds negative steps incorrectly.
        int stepX = SamusGrappleSwing.ReadSignedSine(drawAngle + RotationAngle.QuarterTurn)
            * GrappleBeamDefinitions.Rendering.SegmentSampleToFixedPoint;
        int stepY = SamusGrappleSwing.ReadSignedSine(drawAngle)
            * GrappleBeamDefinitions.Rendering.SegmentSampleToFixedPoint;

        // The rope's tiles mirror by the quarter of the turn the grapple angle lies in: horizontally in the second half
        // of the turn, vertically in the first and last quarters. The rope's end tile marks the connected endpoint.
        int quarter = grapple.Angle.Sector(4);
        SpriteMirror mirror = (quarter >= 2 ? SpriteMirror.Horizontal : SpriteMirror.None) |
            (quarter is 0 or 3 ? SpriteMirror.Vertical : SpriteMirror.None);
        int screenXFixed = unchecked(SixteenSixteen.FromWhole(grapple.BeamStart.X - cameraPosition.X - GrappleBeamDefinitions.Rendering.CharacterCenterOffset));
        int screenYFixed = unchecked(SixteenSixteen.FromWhole(grapple.BeamStart.Y - cameraPosition.Y - GrappleBeamDefinitions.Rendering.CharacterCenterOffset));
        // The game enters the loop before decrementing quotient-minus-one: even a
        // positive length whose masked quotient is zero visits one segment.
        int segmentCount = Math.Max(1, (grapple.RopeLength / GrappleBeamDefinitions.Rendering.SegmentSpacing)
            % GrappleBeamDefinitions.Rendering.SegmentCountCycle);
        for (int segment = 0; segment < segmentCount; segment++)
        {
            // The rope draw walks animation slots from 15 downward regardless of rope length.
            // The first timer expiry reads the already-selected initial record; later
            // expiries advance through the four rope tiles and the goto back to the first.
            // Keeping the one-time first expiry explicit preserves the five-draw
            // interval without pretending that all rope pieces share one animation frame.
            int instructionSlot = GrappleBeamDefinitions.Rendering.FirstSegmentSlot - segment;
            if ((grapple.SegmentAnimationTimers[instructionSlot] = grapple.SegmentAnimationTimers[instructionSlot].Decrement().Remaining).IsZero)
            {
                grapple.SegmentAnimationTimers[instructionSlot] = WrappingCountdown.Start(GrappleBeamDefinitions.Rendering.SegmentAnimationDelay);
                if (grapple.SegmentAnimationStarted[instructionSlot])
                {
                    grapple.SegmentAnimationFrames[instructionSlot] =
                        (grapple.SegmentAnimationFrames[instructionSlot] + 1) % GrappleBeamDefinitions.Rendering.SegmentAnimationFrameCount;
                }
                else
                {
                    grapple.SegmentAnimationStarted[instructionSlot] = true;
                }
            }

            // Every segment is published wherever it lies, off the screen included; the display clips it (#1587).
            int screenX = FixedCoordinate.WholeOf(screenXFixed);
            int screenY = FixedCoordinate.WholeOf(screenYFixed);
            // The rope draw's direction flips are ORed into the segment's own; the sheet holds each combination.
            SpriteSheetFrame image = installed.Frame(SuperMetroid.Core.Samus.Artwork.GrappleSheets.RopeFrame(
                orientation, grapple.SegmentAnimationFrames[instructionSlot], mirror));
            sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusGrappleRope, segment), image,
                Position16<ScreenPixels>.Of(screenX, screenY), SpriteMirror.None));
            screenXFixed = unchecked(screenXFixed + stepX);
            screenYFixed = unchecked(screenYFixed + stepY);
        }

        DrawBeamEndpoint(grapple, sprites, installed.Frame(SuperMetroid.Core.Samus.Artwork.GrappleSheets.EndpointFrame(
            grapple.PointAnimationFrame)), cameraPosition, samusPose);
    }


    /// <summary>Preserves the pose-selected endpoint routines.</summary>
    private static void DrawBeamEndpoint(SamusGrappleState grapple, SpriteFrameBuffer sprites, SpriteSheetFrame image,
        Position16<RoomPixels> cameraPosition, SamusPoseId samusPose)
    {
        bool swinging = samusPose is SamusPoseId.GrappleSwingRightPose or SamusPoseId.GrappleSwingLeftPose;
        int relativeY = CoordinateAxis.Wrap(grapple.Anchor.Y - cameraPosition.Y);
        // The endpoint is published wherever it lies; the display clips it to the view (#1541).
        // Only the swinging routine lets the first of its two subtractions borrow from the second.
        // Consequently unsigned camera subtraction can carry a borrow into the
        // half-character offset, even when wrapped coordinates look adjacent.
        int borrowX = swinging && grapple.Anchor.X < cameraPosition.X ? 1 : 0;
        int borrowY = swinging && grapple.Anchor.Y < cameraPosition.Y ? 1 : 0;
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusGrappleEndpoint), image,
            new Position16<ScreenPixels>(CoordinateAxis.Wrap(grapple.Anchor.X - cameraPosition.X - GrappleBeamDefinitions.Rendering.CharacterCenterOffset - borrowX), CoordinateAxis.Wrap(relativeY - GrappleBeamDefinitions.Rendering.CharacterCenterOffset - borrowY)),
            SpriteMirror.None));
    }


    /// <summary>Clears the three charge-flare words shared by grapple cancellation and connection cleanup.</summary>
    internal static void ClearFlareAnimation(SamusGrappleState grapple)
    {
        // Every grapple teardown clears these same shared charge/grapple
        // words before restoring the ordinary draw handler and projectile palette.
        grapple.FlareStage = GrappleFlareStage.None;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = WrappingCountdown.Start(0);
    }
}
