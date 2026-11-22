using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The projectile animation player and drawing: shot animation and endings, live projectile sprites, and the charge flare's components, advanced and drawn.</summary>
internal sealed partial class ProjectileAnimation
{
    /// <summary>The charge flare, whose components' pictures and waits the animation steps.</summary>
    private readonly BeamChargeState _charge;
    /// <summary>Owns the shot count reduced when a delete cue ends a projectile.</summary>
    private readonly SamusShotPool _pool;

    /// <summary>The Samus artwork whose installed projectile frames each shot's frames draw.</summary>
    private readonly SamusArtworkBinding _artwork;

    /// <summary>Binds projectile cues to the shot pool, the charge-flare state and the artwork the frames draw.</summary>
    internal ProjectileAnimation(BeamChargeState charge, SamusShotPool pool, SamusArtworkBinding artwork)
    {
        _charge = charge;
        _pool = pool;
        _artwork = artwork;
    }
    /// <summary>
    /// Plays timed frames, still pictures and deletion: each frame <paramref name="animation"/>, the shot's own, shows anew
    /// publishes its picture and radius, and an animation that ends deletes the projectile. A shot showing a still picture
    /// keeps it.
    /// </summary>
    /// <param name="shot">The projectile whose animation plays.</param>
    /// <param name="animation">The animation the projectile is playing.</param>
    internal void AdvanceProjectileAnimation(
        SamusProjectileOccupant shot,
        AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> animation) =>
        SamusProjectilePlayback.PlayOn(
            Ensure.NotNull(animation),
            _artwork.ProjectileFrames,
            (sprite, radius) =>
            {
                shot.Sprite = sprite;
                shot.Radius = radius;
            },
            () => ClearProjectile(shot.Projectile));


    /// <summary>Deletes a projectile, ending it, and decrements the shot count, which stays at zero.</summary>
    internal void ClearProjectile(SamusProjectile shot)
    {
        Ensure.NotNull(shot).End();
        _pool.ShotCount.CountDeletion();
    }

    /// <summary>Draws a shot's picture where it lies; the display clips it to the view.</summary>
    internal static void DrawShot(
        SpriteFrameBuffer sprites,
        SpriteSource source,
        SamusProjectile shot,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork artwork)
    {
        Offset16<ScreenPixels> place = shot.Position.Whole.RelativeToCamera(cameraPosition);
        // Every placement is published wherever it lies; the display clips it to the view (#1541).

        // A frame that shows no composition draws nothing (#2558).
        if (shot.Sprite is not { } sprite)
            return;
        // A composition the sheet of the beams the projectile is pictured with holds as showing nothing draws nothing
        // (#1513, #1586); an invisible frame shows no composition at all (#2558).
        SpriteSheet sheet = artwork.SheetOf(shot.PicturedBeams);
        string frame = ProjectileSheetFrames.ProjectileFrame(sprite);
        if (sheet.ShowsNothing(frame))
            return;
        SpriteSheetFrame image = sheet.Frame(frame);
        sprites.Add(new SamusColouredDraw(source, image, place));
    }

    /// <summary>Plays a flare component's ordinary animation one update on.</summary>
    internal void AdvanceFlareComponent(ChargeFlareComponent component) => _charge.PlayFlareForward(component);

    /// <summary>
    /// Reads Samus's shot direction and movement metadata before drawing an eligible charge-flare component. Only a pose
    /// that disables shooting draws no flare; a turning pose's restriction selects its authored turn row (#1504).
    /// </summary>
    internal void DrawFlareComponent(SpriteFrameBuffer sprites, SamusState samus, Position16<RoomPixels> cameraPosition, ChargeFlareComponent component, TurningFloorTransform? turningFloor, ChargeFlareArtwork artwork)
    {
        if (SamusPoseMetadata.ReadAim(samus) is not SamusPoseAim.Armed { Flare: var flare })
            return;

        DrawFlareComponentWithMetadata(sprites, samus, cameraPosition, component, turningFloor, artwork, new ChargeFlarePose { Aim = flare, Running = SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Running, PoseYOffset = SamusPoseMetadata.ReadArtOrigin(samus), Facing = FlareFacings[SamusPoseMetadata.ReadFacingDirection(samus)] });
    }

    /// <summary>Applies pose muzzle offsets after optional Ceres turning-floor center transform, then draws the installed component frame.</summary>
    internal void DrawFlareComponentWithMetadata(SpriteFrameBuffer sprites, SamusState samus, Position16<RoomPixels> cameraPosition, ChargeFlareComponent component, TurningFloorTransform? turningFloor, ChargeFlareArtwork artwork, ChargeFlarePose pose)
    {
        Offset16<RoomPixels> visualOffset = pose.Aim.PlacedBy(artwork.Placement, pose.Running);

        // The flare turns with Ceres's turning floor whenever Samus's body does. Only Samus's centre turns; the muzzle
        // offsets her pose selects are added after, and her position itself is unchanged.
        var samusPosition = samus.Position.Whole;
        Position16<RoomPixels> renderPoint = turningFloor is { } transform
            ? transform.Transform(samusPosition)
            : samusPosition;
        Offset16<ScreenPixels> place =
            (renderPoint + visualOffset - Offset16<RoomPixels>.Of(0, pose.PoseYOffset)).RelativeToCamera(cameraPosition);

        // The flare is published wherever it lies; the display clips it to the view (#1541).

        // A component the Hyper Beam's count has stepped back past its first picture draws nothing (#1275, #2114).
        if (_charge.FlareComposition(component, pose.Facing) is not { } picture)
            return;
        // The flare is Samus's own charge, so it shows the sheet of the beams she has equipped (#1586).
        SpriteSheetFrame image = artwork.Sheets.SheetOf(SamusBeamCombinations.Of(samus.EquippedBeams)).Frame(ProjectileSheetFrames.FlareFrame(picture));
        sprites.Add(new SamusColouredDraw(new SpriteSource(SpriteSourceKind.SamusChargeFlare), image, place));
    }

    /// <summary>The way the charge flare faces for each way Samus's pose faces: left only while she faces left.</summary>
    private static IdentityTable<SamusFacingDirection, HorizontalDirection> FlareFacings { get; } = new(new Dictionary<SamusFacingDirection, HorizontalDirection>
    {
        [SamusFacingDirection.ForwardOrSpecial] = HorizontalDirection.Right,
        [SamusFacingDirection.Left] = HorizontalDirection.Left,
        [SamusFacingDirection.Right] = HorizontalDirection.Right,
    });
}
