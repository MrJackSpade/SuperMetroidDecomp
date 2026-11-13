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

        // A just-fired flare restarts at the main flare's loop on every such call, then counts its wait as usual, so
        // the first visible picture's wait is already one update spent.
        if (grapple.FlareStage == GrappleFlareStage.JustFired)
            ShowFlare(grapple, ChargeFlareAnimations.MainFlareLoop());

        grapple.FlareAnimationTimer = grapple.FlareAnimationTimer.Decrement().Remaining;
        if (grapple.FlareAnimationTimer.IsNegative)
            ShowFlare(grapple, ChargeFlareAnimations.NextMainFlare(grapple.FlarePicture));

        int screenX = CoordinateAxis.Wrap(grapple.BeamStart.X - cameraPosition.X);
        int screenY = CoordinateAxis.Wrap(grapple.BeamStart.Y - cameraPosition.Y);
        // The flare is published wherever it lies; the display clips it to the view (#1541).

        // The grapple shows the main flare's pictures alone, the same for either facing. The flare is Samus's own, so it
        // shows the sheet of the beams she has equipped (#1586).
        SuperMetroid.Core.Samus.Artwork.ProjectileSpriteId picture =
            SuperMetroid.Core.Samus.Artwork.ChargeFlarePictures.Of(ChargeFlareComponent.MainFlare, facingLeft: false, grapple.FlarePicture.Index)
            ?? throw new InvalidOperationException($"The grapple flare's picture {grapple.FlarePicture.Index} shows no main flare.");
        SpriteSheetFrame image = objects.SheetOf(SamusBeamCombinations.Of(samus.EquippedBeams)).Frame(SuperMetroid.Core.Samus.Artwork.ProjectileSheetFrames.FlareFrame(
            picture));
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
        grapple.FlarePicture = ChargeFlareAnimations.FirstPicture;
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


    /// <summary>Shows <paramref name="step"/>'s main-flare picture on the grapple's flare and holds it for the step's wait.</summary>
    private static void ShowFlare(SamusGrappleState grapple, ChargeFlareStep step)
    {
        grapple.FlarePicture = new ChargeFlarePhase(step.Phase);
        grapple.FlareAnimationTimer = step.Hold;
    }


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
        // Preserve the fraction across segments. Rounding each individual
        // displacement shortens diagonals and rounds negative steps incorrectly.
        int stepX = SamusGrappleSwing.ReadSignedSine(drawAngle + RotationAngle.QuarterTurn)
            * GrappleBeamDefinitions.Rendering.SegmentSampleToFixedPoint;
        int stepY = SamusGrappleSwing.ReadSignedSine(drawAngle)
            * GrappleBeamDefinitions.Rendering.SegmentSampleToFixedPoint;

        SpriteMirror mirror = RopeMirror(grapple.Angle);
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
    /// How the rope's pictures mirror at the grapple angle <paramref name="angle"/>: horizontally in the second half of
    /// the turn, and vertically within a quarter turn either side of the angle zero.
    /// </summary>
    internal static SpriteMirror RopeMirror(RotationAngle angle) =>
        (angle >= RotationAngle.HalfTurn ? SpriteMirror.Horizontal : SpriteMirror.None) |
        (angle < RotationAngle.QuarterTurn || angle >= RotationAngle.ThreeQuarterTurn ? SpriteMirror.Vertical : SpriteMirror.None);


    /// <summary>
    /// The picture a rope segment shows at the rope's animation phase: the muzzle's segment shows the cycle's last
    /// picture moved on by the phase, and each segment further along the rope the picture before its neighbour's.
    /// </summary>
    private static int RopePicture(int phase, int segment) =>
        (phase + RopeCycle - 1 - segment % RopeCycle) % RopeCycle;


    /// <summary>Clears the three charge-flare values shared by grapple cancellation and connection cleanup.</summary>
    internal static void ClearFlareAnimation(SamusGrappleState grapple)
    {
        // Every grapple teardown clears these same shared charge/grapple
        // values before restoring the ordinary draw handler and projectile palette.
        grapple.FlareStage = GrappleFlareStage.None;
        grapple.FlarePicture = ChargeFlareAnimations.FirstPicture;
        grapple.FlareAnimationTimer = WrappingCountdown.Start(0);
    }
}
