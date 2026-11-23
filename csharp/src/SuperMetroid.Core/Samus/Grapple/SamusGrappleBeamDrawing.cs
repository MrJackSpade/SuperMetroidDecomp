using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>How the grapple beam draws: its flare, the connected beam's segments and its endpoint.</summary>
public static partial class SamusGrappleBeamDrawing
{
    /// <summary>How Samus draws during <paramref name="phase"/>.</summary>
    /// <remarks>
    /// Cancellation, release, drop and the grapple wall jump each last one update. On the frame one of them begins, the
    /// grapple still owns Samus's drawing, so the ordinary charge flare stays hidden, but neither the grapple's flare nor
    /// its beam draws: Samus and her echoes draw alone.
    /// </remarks>
    public static SamusGrappleDrawing DrawingFor(GrapplePhase phase) => phase switch
    {
        GrapplePhase.Inactive => SamusGrappleDrawing.Ordinary,
        GrapplePhase.Firing or GrapplePhase.ConnectedSwinging or GrapplePhase.ConnectedLocked or GrapplePhase.WallGrab or
            GrapplePhase.WallGrabRelease => SamusGrappleDrawing.Beam,
        GrapplePhase.CancelPending or GrapplePhase.CancelPendingFromConnectedPose or GrapplePhase.ReleaseFromSwing or GrapplePhase.WallJumping or GrapplePhase.Dropped =>
            SamusGrappleDrawing.Teardown,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, $"Undefined {nameof(GrapplePhase)}."),
    };

    /// <summary>
    /// Draws the grapple beam's flare. Call before atmosphere and Samus,
    /// exactly where the active half of the grapple beam's drawing draws it.
    /// </summary>
    /// <returns>True when the flare is drawn: the grapple is firing or connected with a flare to show.</returns>
    public static bool DrawFlareBeforeSamus(
        SamusState samus,
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork sheets)
    {
        Ensure.NotNull(samus);
        Ensure.NotNull(sprites);

        SamusGrappleState grapple = samus.Grapple;
        if (DrawingFor(grapple.Phase) != SamusGrappleDrawing.Beam || grapple.FlareStage == GrappleFlareStage.None)
            return false;

        // A just-fired flare restarts at the main flare's loop on every such update, then plays on as usual, so the
        // first visible picture already has one update spent.
        if (grapple.FlareStage == GrappleFlareStage.JustFired)
            grapple.Flare.RestartAtLoop();
        grapple.Flare.PlayForward();

        Offset16<ScreenPixels> place = grapple.FlareOrigin.RelativeToCamera(cameraPosition);
        // The flare is published wherever it lies; the display clips it to the view (#1541).

        // The grapple shows the main flare's pictures alone, the same for either facing. The flare is Samus's own, so it
        // shows the sheet of the beams she has equipped (#1586).
        ProjectileSpriteId picture = ChargeFlarePictures.MainFlare(grapple.Flare.Picture);
        SpriteSheetFrame image = sheets.SheetOf(SamusBeamCombinations.Of(samus.EquippedBeams)).Frame(ProjectileSheetFrames.FlareFrame(
            picture));
        sprites.Add(new SamusColouredDraw(new SpriteSource(SpriteSourceKind.SamusGrappleFlare), image, place));
        return true;
    }

/// <summary>Seeds the first flare and point frames and the rope's animation.</summary>
    internal static void InitializeBeamAnimation(SamusGrappleState grapple)
    {
        // Firing marks the flare just fired after starting the rope's animation.
        // A beam that starts already connected was never fired, and it begins at the same first flare stage as one
        // just fired.
        grapple.FlareStage = GrappleFlareStage.JustFired;
        grapple.Flare.Clear();
        grapple.Endpoint.Show(GrappleAnimations.Endpoint);
        // The rope starts on its opening pattern: the muzzle's segment on the cycle's last picture, each segment beyond
        // on the one before. Its first step waits one draw longer than the rest.
        grapple.Rope.Show(GrappleAnimations.Rope, GrappleBeamDefinitions.Rendering.FirstSegmentAnimationDelay);
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
        SpriteSheet sheet)
    {
        Ensure.NotNull(grapple);
        Ensure.NotNull(sprites);
        if (DrawingFor(grapple.Phase) != SamusGrappleDrawing.Beam)
            return;

        // Each of its beats moves the endpoint on to the next of its pictures.
        grapple.Endpoint.Step();
        SpriteSheet installed = Ensure.NotNull(sheet);
        GrappleRopeOrientation orientation =
            GrappleRopeOrientations.For(grapple.Angle);

        // The beam's draw moves the flare on a stage before it draws any rope. It happens even
        // at zero rope length; only the rope below is conditional. This is why a newly fired
        // beam can animate its muzzle flare before its first body segment exists.
        grapple.FlareStage = grapple.FlareStage switch
        {
            GrappleFlareStage.None => GrappleFlareStage.JustFired,
            GrappleFlareStage.JustFired or GrappleFlareStage.Shown => GrappleFlareStage.Shown,
            _ => throw new InvalidOperationException($"Undefined {nameof(GrappleFlareStage)} {grapple.FlareStage}."),
        };
        if (grapple.RopeLength <= 0)
            return;

        // The rope is drawn along the direction from the flare to the anchor, worked out again on every draw rather
        // than taken from the grapple's angle, so it stays visually attached after pose-specific art-origin correction.
        RotationAngle drawAngle = SamusGrappleMovement.CalculateAngleFromXY(grapple.Anchor - grapple.FlareOrigin);
        // The fraction carries from segment to segment, so diagonal ropes keep their length and leftward and upward
        // steps match the others.
        var step = new FixedVector<ScreenPixels>(
            SegmentStep(SamusGrappleSwing.ReadSignedSine(drawAngle + RotationAngle.QuarterTurn)),
            SegmentStep(SamusGrappleSwing.ReadSignedSine(drawAngle)));

        SpriteMirror mirror = RopeMirror(grapple.Angle);
        // Each rope draw counts one off the pattern's picture, and the whole rope steps on with it.
        grapple.Rope.Step();

        // Each rope picture is placed by its centre, the first at the muzzle and each after it one segment's spacing on
        // toward the anchor. A positive rope shorter than one spacing still shows its first picture.
        FixedVector<ScreenPixels> centre = FixedVector<ScreenPixels>.FromWholePixels(grapple.FlareOrigin.RelativeToCamera(cameraPosition));
        int segmentCount = Math.Max(1, grapple.RopeLength / GrappleBeamDefinitions.Rendering.SegmentSpacing);
        // The muzzle's segment shows the pattern's picture, and each segment beyond it the picture before its neighbour's.
        GrappleRopePicture picture = grapple.Rope.Showing;
        for (int segment = 0; segment < segmentCount; segment++)
        {
            // The rope's mirroring at its angle picks the segment's picture, which the sheet holds already mirrored, so
            // the segment draws unmirrored.
            SpriteSheetFrame image = installed.Frame(GrappleSheets.RopeFrame(orientation, picture, mirror));
            sprites.Add(new SamusColouredDraw(new SpriteSource(SpriteSourceKind.SamusGrappleRope), image, centre.Whole));
            centre += step;
            picture = GrappleAnimations.RopePictureBefore[picture];
        }

        // The endpoint is placed by its centre at the anchor.
        SpriteSheetFrame endpoint = installed.Frame(GrappleSheets.EndpointFrame(grapple.Endpoint.Showing));
        sprites.Add(new SamusColouredDraw(new SpriteSource(SpriteSourceKind.SamusGrappleEndpoint), endpoint,
            grapple.Anchor.RelativeToCamera(cameraPosition)));
    }

/// <summary>
    /// The displacement along one axis from one rope picture's centre to the next: one segment's spacing scaled by the
    /// draw direction's share <paramref name="component"/> along the axis, keeping the fraction so diagonal and backward
    /// steps add up exactly.
    /// </summary>
    private static SignedSixteenSixteen SegmentStep(UnitComponent component) =>
        component.ExactReachOf(GrappleBeamDefinitions.Rendering.SegmentSpacing);

    /// <summary>
    /// How the rope's pictures mirror at the grapple angle <paramref name="angle"/>: horizontally in the second half of
    /// the turn, and vertically within a quarter turn either side of the angle zero.
    /// </summary>
    internal static SpriteMirror RopeMirror(RotationAngle angle) =>
        (angle >= RotationAngle.HalfTurn ? SpriteMirror.Horizontal : SpriteMirror.None) |
        (angle < RotationAngle.QuarterTurn || angle >= RotationAngle.ThreeQuarterTurn ? SpriteMirror.Vertical : SpriteMirror.None);

/// <summary>Clears the flare's stage and its animation, for grapple cancellation and connection cleanup alike.</summary>
    internal static void ClearFlareAnimation(SamusGrappleState grapple)
    {
        // Every grapple teardown clears the grapple's flare before restoring Samus's ordinary drawing and projectile
        // shade.
        grapple.FlareStage = GrappleFlareStage.None;
        grapple.Flare.Clear();
    }
}
