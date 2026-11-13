using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How the grapple beam draws: its flare, the connected beam's segments and its endpoint.</summary>
public static class SamusGrappleBeamDrawing
{
    /// <summary>The rope's cycle of pictures.</summary>
    private const int RopeCycle = GrappleBeamDefinitions.Rendering.SegmentAnimationFrameCount;

    /// <summary>How Samus draws during <paramref name="phase"/>.</summary>
    /// <remarks>
    /// Cancellation, release, drop and the grapple wall jump each last one call. On the frame one of them begins, the
    /// grapple still owns Samus's drawing, so the ordinary charge flare stays hidden, but neither the grapple's flare nor
    /// its beam draws: Samus and her echoes draw alone.
    /// </remarks>
    public static SamusGrappleDrawing DrawingFor(GrapplePhase phase) => phase switch
    {
        GrapplePhase.Inactive => SamusGrappleDrawing.Ordinary,
        GrapplePhase.Firing or GrapplePhase.ConnectedSwinging or GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab or
            GrapplePhase.WallGrabRelease => SamusGrappleDrawing.Beam,
        GrapplePhase.CancelPending or GrapplePhase.ReleaseFromSwing or GrapplePhase.WallJumping or GrapplePhase.Dropped =>
            SamusGrappleDrawing.Teardown,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, $"Undefined {nameof(GrapplePhase)}."),
    };

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
        if (DrawingFor(grapple.Phase) != SamusGrappleDrawing.Beam || grapple.FlareStage == GrappleFlareStage.None)
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


    /// <summary>Seeds the first flare and point frames and the rope's animation.</summary>
    internal static void InitializeBeamAnimation(SamusGrappleState grapple)
    {
        // Firing marks the flare just fired after starting the rope's animation.
        // A beam that starts already connected has no preceding firing history, so it
        // begins at the same first flare state rather than inventing a static flare.
        grapple.FlareStage = GrappleFlareStage.JustFired;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = WrappingCountdown.Start(0);
        grapple.PointAnimationTimer = WrappingCountdown.Start(GrapplePointAnimationDefinitions.Delay);
        grapple.PointAnimationFrame = 0;
        // The rope starts on its opening pattern: the muzzle's segment on the cycle's last picture, each segment beyond
        // on the one before. Its first step waits one draw longer than the rest.
        grapple.RopeAnimationPhase = 0;
        grapple.RopeAnimationTimer = GrappleBeamDefinitions.Rendering.FirstSegmentAnimationDelay;
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
    /// Draws the grapple beam after Samus: the rope as a chain of rope pictures from the muzzle toward the anchor,
    /// animated as one, and the beam's endpoint at the anchor. Every picture is published wherever it lies, and the
    /// display clips it to the view.
    /// </summary>
    public static void DrawConnectedBeam(
        SamusGrappleState grapple,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        SpriteSheet? sheet = null)
    {
        Ensure.NotNull(grapple);
        Ensure.NotNull(sprites);
        if (DrawingFor(grapple.Phase) != SamusGrappleDrawing.Beam)
            return;

        // The pair is a begin/end range, not two animation frames. Each
        // six-call interval advances the source by 512 through four endpoint tiles.
        GrapplePointAnimation.Advance(grapple);
        SpriteSheet installed = sheet ?? throw new InvalidOperationException("The grapple beam requires the installed grapple sheet.");
        SuperMetroid.Core.Samus.Artwork.GrappleRopeOrientation orientation =
            SuperMetroid.Core.Samus.Artwork.GrappleRopeOrientations.For(grapple.Angle);

        // The beam's draw moves the flare on a stage before it draws any rope. It happens even
        // at zero rope length; only the rope below is conditional. This is why a newly fired
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

        // The rope's pictures mirror by the quarter of the turn the grapple angle lies in: horizontally in the second half
        // of the turn, vertically in the first and last quarters.
        int quarter = grapple.Angle.Sector(4);
        SpriteMirror mirror = (quarter >= 2 ? SpriteMirror.Horizontal : SpriteMirror.None) |
            (quarter is 0 or 3 ? SpriteMirror.Vertical : SpriteMirror.None);
        AdvanceRopeAnimation(grapple);

        // Each rope picture is placed by its centre, the first at the muzzle and each after it one segment's spacing on
        // toward the anchor. A positive rope shorter than one spacing still shows its first picture.
        int centreXFixed = unchecked(SixteenSixteen.FromWhole(grapple.BeamStart.X - cameraPosition.X));
        int centreYFixed = unchecked(SixteenSixteen.FromWhole(grapple.BeamStart.Y - cameraPosition.Y));
        int segmentCount = Math.Max(1, grapple.RopeLength / GrappleBeamDefinitions.Rendering.SegmentSpacing);
        for (int segment = 0; segment < segmentCount; segment++)
        {
            // The rope draw's direction flips are ORed into the segment's own; the sheet holds each combination.
            SpriteSheetFrame image = installed.Frame(SuperMetroid.Core.Samus.Artwork.GrappleSheets.RopeFrame(
                orientation, RopePicture(grapple.RopeAnimationPhase, segment), mirror));
            sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusGrappleRope, segment), image,
                Position16<ScreenPixels>.Of(FixedCoordinate.WholeOf(centreXFixed), FixedCoordinate.WholeOf(centreYFixed)),
                SpriteMirror.None));
            centreXFixed = unchecked(centreXFixed + stepX);
            centreYFixed = unchecked(centreYFixed + stepY);
        }

        // The endpoint is placed by its centre at the anchor.
        SpriteSheetFrame endpoint = installed.Frame(SuperMetroid.Core.Samus.Artwork.GrappleSheets.EndpointFrame(grapple.PointAnimationFrame));
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusGrappleEndpoint), endpoint,
            new Position16<ScreenPixels>(CoordinateAxis.Wrap(grapple.Anchor.X - cameraPosition.X), CoordinateAxis.Wrap(grapple.Anchor.Y - cameraPosition.Y)),
            SpriteMirror.None));
    }


    /// <summary>
    /// Counts one rope draw off the rope's animation, and steps the whole rope on to its next picture on the draw that
    /// finishes the wait.
    /// </summary>
    private static void AdvanceRopeAnimation(SamusGrappleState grapple)
    {
        int remaining = grapple.RopeAnimationTimer - 1;
        if (remaining == 0)
        {
            remaining = GrappleBeamDefinitions.Rendering.SegmentAnimationDelay;
            grapple.RopeAnimationPhase = (grapple.RopeAnimationPhase + 1) % RopeCycle;
        }
        grapple.RopeAnimationTimer = remaining;
    }


    /// <summary>
    /// The picture a rope segment shows at the rope's animation phase: the muzzle's segment shows the cycle's last
    /// picture moved on by the phase, and each segment further along the rope the picture before its neighbour's.
    /// </summary>
    private static int RopePicture(int phase, int segment) =>
        (phase + RopeCycle - 1 - segment % RopeCycle) % RopeCycle;


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
