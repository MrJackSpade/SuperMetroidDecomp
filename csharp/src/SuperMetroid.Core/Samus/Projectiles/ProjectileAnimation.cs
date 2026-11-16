using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The projectile animation player and drawing: ending shots, live projectile and charge-flare sprites, flare animation state and the suit shade the flare restores.</summary>
internal sealed class ProjectileAnimation
{
    /// <summary>Owns charge-flare frame and countdown arrays advanced by the animation.</summary>
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
    /// <summary>Plays timed frames, jumps and deletion; publishes frame radius and animation metadata when the countdown expires.</summary>
    internal bool AdvanceProjectileAnimation(
        SamusProjectile shot)
    {
        shot.FrameHold = shot.FrameHold.Decrement().Remaining;
        if (!shot.FrameHold.IsZero)
            return false;
        AnimationPlayback<SamusProjectileSegment> playback = shot.Playback
            ?? throw new InvalidOperationException("An inactive projectile has no list to run.");
        if (SamusProjectilePlayback.PlayToNextFrame(ref playback, out int duration) is not { } frame)
        {
            ClearProjectile(shot);
            return true;
        }
        SamusProjectileFrameGameplay gameplay = SamusProjectileFrameDefinitions.Get(frame);
        shot.FrameHold = WrappingCountdown.Start(duration);
        shot.Sprite = _artwork.ProjectileFrames.Resolve(frame);
        shot.Radius = gameplay.Radius;
        shot.Playback = playback;
        return false;
    }


    /// <summary>Deletes a projectile, ending it, and decrements the shot count, which stays at zero.</summary>
    internal void ClearProjectile(SamusProjectile shot)
    {
        Ensure.NotNull(shot).End();
        _pool.ShotCount.CountDeletion();
    }

    /// <summary>Publishes an installed projectile composition when its screen origin passes horizontal and vertical clipping.</summary>
    internal static void DrawShot(
        SpriteFrameBuffer sprites,
        SpriteSource source,
        SamusProjectile shot,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork artwork)
    {
        Offset16<ScreenPixels> place = shot.Position.Whole.RelativeToCamera(cameraPosition);
        // Every placement is published wherever it lies; the display clips it to the view (#1541).

        ProjectileSpriteId sprite = shot.Sprite ?? throw new InvalidOperationException("A drawn projectile shows a composition.");
        // The empty composition an invisible projectile shows draws nothing, and so does a composition the sheet of the
        // beams it is pictured with holds as showing nothing (#1513, #1586).
        if (sprite.DrawsNothing)
            return;
        SpriteSheet sheet = artwork.SheetOf(shot.PicturedBeams);
        string frame = ProjectileSheetFrames.ProjectileFrame(sprite);
        if (sheet.ShowsNothing(frame))
            return;
        SpriteSheetFrame image = sheet.Frame(frame);
        sprites.Add(new ImageSpriteDraw(source, image, place, SpriteMirror.None));
    }

    /// <summary>Counts one update off a flare component's wait and shows its animation's next picture once the wait is spent.</summary>
    internal void AdvanceFlareComponent(ChargeFlareComponent component)
    {
        // The next picture shows on the update the wait's count passes below zero, so a wait of zero still holds its
        // picture for one update; testing for zero would show every picture one update too short.
        ChargeFlareComponentState flare = _charge.Flare(component);
        CountdownStep hold = flare.CountHold();
        if (hold.IsNonNegative)
            return;
        // Only the Hyper Beam's count runs a component past its first picture, and the ordinary flare never plays again
        // once Samus has the Hyper Beam.
        int picture = flare.Picture
            ?? throw new InvalidOperationException($"Charge-flare {component} has run past its first picture and plays no further.");
        flare.Show(ChargeFlareAnimations.Next(component, picture));
    }

    /// <summary>
    /// Reads Samus's shot direction and movement metadata before drawing an eligible charge-flare component. Only a pose
    /// that disables shooting draws no flare; a turning pose's restriction selects its authored turn row (#1504).
    /// </summary>
    internal void DrawFlareComponent(SpriteFrameBuffer sprites, SamusState samus, Position16<RoomPixels> cameraPosition, ChargeFlareComponent component, TurningFloorTransform? turningFloor, ChargeFlareArtwork artwork)
    {
        if (SamusPoseMetadata.ReadAim(samus).Flare is not { } flare)
            return;

        DrawFlareComponentWithMetadata(sprites, samus, cameraPosition, component, turningFloor, artwork, new ChargeFlarePose { Aim = flare, Running = SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.Running, PoseYOffset = SamusPoseMetadata.ReadArtOrigin(samus), FacingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) });
    }

    /// <summary>Applies pose muzzle offsets after optional Ceres turning-floor center transform, then draws the installed component frame.</summary>
    internal void DrawFlareComponentWithMetadata(SpriteFrameBuffer sprites, SamusState samus, Position16<RoomPixels> cameraPosition, ChargeFlareComponent component, TurningFloorTransform? turningFloor, ChargeFlareArtwork artwork, ChargeFlarePose pose)
    {
        Offset16<RoomPixels> visualOffset = artwork.Placement.Resolve(pose.Running, pose.Aim);

        // The flare draw applies Ceres's rotation whenever the body renderer does, under the same
        // Ceres status. Only Samus's center is transformed; the pose-selected muzzle
        // offsets are added afterward. Retail then restores Samus's world point, so this path
        // must never write the public physics coordinates merely to reuse the arithmetic.
        var samusPosition = samus.Position.Whole;
        Position16<RoomPixels> renderPoint = turningFloor is { } transform
            ? transform.Transform(samusPosition)
            : samusPosition;
        Offset16<ScreenPixels> place =
            (renderPoint + visualOffset - Offset16<RoomPixels>.Of(0, pose.PoseYOffset)).RelativeToCamera(cameraPosition);

        // The flare is published wherever it lies; the display clips it to the view (#1541).

        // A component the Hyper Beam's count has run past its first picture draws nothing (#1275, Mother Brain glitch
        // movie; #2114).
        if (_charge.Flare(component).Picture is not { } shownPlace)
            return;
        ProjectileSpriteId picture = ChargeFlarePictures.Of(component, pose.FacingLeft, shownPlace);
        // The flare is Samus's own charge, so it shows the sheet of the beams she has equipped (#1586).
        SpriteSheetFrame image = artwork.Objects.SheetOf(SamusBeamCombinations.Of(samus.EquippedBeams)).Frame(ProjectileSheetFrames.FlareFrame(picture));
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusChargeFlare), image,
            place, SpriteMirror.None));
    }

    /// <summary>Clears all component frames and countdowns when charge-flare animation is retired.</summary>
    internal void ClearFlareAnimationState()
    {
        _charge.ClearFlare();
    }

    /// <summary>Restores sixteen suit colors from installed artwork.</summary>
    internal static void ShowSuitShade(
        SamusColours colours,
        SamusState samus) =>
        SamusSuitShade.Show(colours, samus.EquippedItems);

}
