using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The projectile animation interpreter and drawing: slot clearing, live projectile and charge-flare sprites, flare animation state and the suit palette the flare restores.</summary>
internal sealed class ProjectileAnimation
{
    /// <summary>Owns charge-flare frame and countdown arrays advanced by the interpreter.</summary>
    private readonly BeamChargeState _charge;
    /// <summary>Owns the shot count reduced when a delete instruction ends a projectile.</summary>
    private readonly ProjectileSlotTable _slotTable;
    /// <summary>The delays a flare frame wrapping below the flare's programs reads.</summary>
    private readonly ChargeFlareDelaysBelowPrograms _flareDelaysBelow;

    /// <summary>Binds projectile instructions to live slot ownership and charge-flare state.</summary>
    internal ProjectileAnimation(BeamChargeState charge, ProjectileSlotTable slotTable, ChargeFlareDelaysBelowPrograms flareDelaysBelow)
    {
        _charge = charge;
        _slotTable = slotTable;
        _flareDelaysBelow = Ensure.NotNull(flareDelaysBelow);
    }
    /// <summary>Decodes timed frames, goto, and delete; publishes frame radius and animation metadata when the countdown expires.</summary>
    internal bool RunProjectileInstructionHandler(
        SamusProjectile slot)
    {
        slot.FrameHold = slot.FrameHold.Decrement().Remaining;
        if (!slot.FrameHold.IsZero)
            return false;
        AnimationPlayback<SamusProjectileProgram> playback = slot.Playback
            ?? throw new InvalidOperationException("An inactive projectile has no list to run.");
        if (SamusProjectilePrograms.RunToNextRecord(ref playback, out int duration) is not { } frame)
        {
            ClearProjectile(slot);
            return true;
        }
        SamusProjectileRecord record = SamusProjectileRecordDefinitions.Get(frame);
        slot.FrameHold = WrappingCountdown.FromRaw(duration);
        slot.Sprite = (FrameBindings ?? throw new InvalidOperationException(
            "Samus's projectiles show their records' installed compositions, which are not bound.")).Resolve(frame);
        slot.Radius = record.Radius;
        slot.Playback = playback;
        return false;
    }


    /// <summary>Deletes a projectile, ending it, and decrements the shot count, which stays at zero.</summary>
    internal void ClearProjectile(SamusProjectile slot)
    {
        Ensure.NotNull(slot).End();
        _slotTable.ProjectileCounter = _slotTable.ProjectileCounter == 0
            ? 0
            : _slotTable.ProjectileCounter - 1;
    }

    /// <summary>Publishes an installed projectile composition when its screen origin passes horizontal and Y-byte clipping.</summary>
    internal static void DrawSlot(
        SpriteFrameBuffer sprites,
        SpriteSource source,
        SamusProjectile slot,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork artwork)
    {
        int screenX = CoordinateAxis.Difference(slot.Position.X.Whole, cameraPosition.X);
        int screenY = CoordinateAxis.Wrap(slot.Position.Y.Whole - cameraPosition.Y);
        // Every placement is published wherever it lies; the display clips it to the view (#1541).

        ProjectileSpriteId sprite = slot.Sprite ?? throw new InvalidOperationException("A drawn projectile shows a composition.");
        // The empty composition an invisible projectile shows draws nothing, and so does a composition the sheet of the
        // beams it is pictured with holds as showing nothing (#1513, #1586).
        if (sprite.DrawsNothing)
            return;
        SpriteSheet sheet = artwork.SheetOf(slot.PicturedBeams);
        string frame = ProjectileSheetFrames.ProjectileFrame(sprite);
        if (sheet.ShowsNothing(frame))
            return;
        SpriteSheetFrame image = sheet.Frame(frame);
        sprites.Add(new ImageSpriteDraw(source, image, Position16<ScreenPixels>.Of(CoordinateAxis.Wrap(screenX), screenY),
            SpriteMirror.None));
    }

    /// <summary>Counts one update off a flare component's wait and shows its program's next picture once the wait is spent.</summary>
    internal void AdvanceFlareComponent(ChargeFlareComponent component)
    {
        // The next picture shows on the update the wait's count passes below zero, so a wait of zero still holds its
        // picture for one update; testing for zero would show every picture one update too short.
        CountdownStep hold = _charge.CountFlareHold(component);
        if (hold.IsNonNegative)
            return;
        _charge.ShowFlare(component, ChargeFlareAnimations.Next(component, _charge.FlarePhase(component), _flareDelaysBelow));
    }

    /// <summary>
    /// Reads Samus's shot direction and movement metadata before drawing an eligible charge-flare component. Only a pose
    /// that disables shooting draws no flare; a turning pose's restriction selects its authored turn row (#1504).
    /// </summary>
    internal void DrawFlareComponent(SpriteFrameBuffer sprites, SamusState samus, Position16<RoomPixels> cameraPosition, ChargeFlareComponent component, TurningFloorTransform? turningFloor, ChargeFlareArtwork artwork)
    {
        if (SamusPoseMetadata.ReadAim(samus).Flare is not { } flare)
            return;

        DrawFlareComponentWithMetadata(sprites, samus, cameraPosition, component, turningFloor, artwork, new ChargeFlarePose { Aim = flare, Running = SamusPoseMetadata.ReadMovementKind(samus) == SamusMovementType.Running, PoseYOffset = ChargeFlarePose.RiseOf(SamusPoseMetadata.ReadGraphicsYOffset(samus)), FacingLeft = SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left) });
    }

    /// <summary>Applies pose muzzle offsets after optional Ceres turning-floor center transform, then draws the installed component frame.</summary>
    internal void DrawFlareComponentWithMetadata(SpriteFrameBuffer sprites, SamusState samus, Position16<RoomPixels> cameraPosition, ChargeFlareComponent component, TurningFloorTransform? turningFloor, ChargeFlareArtwork artwork, ChargeFlarePose pose)
    {
        var visualOffset = (artwork.Placement ?? throw new InvalidOperationException(
            "Charge flare requires installed placement definitions."))
            .Resolve(pose.Running, pose.Aim);
        int xOffset = visualOffset.X;
        int yOffset = visualOffset.Y;

        // The flare draw applies Ceres's rotation whenever the body renderer does, under the same
        // Ceres status. Only Samus's center is transformed; the pose-selected muzzle
        // offsets are added afterward. Retail then restores Samus's world point, so this path
        // must never write the public physics coordinates merely to reuse the arithmetic.
        var samusPosition = samus.Position.Whole;
        Position16<RoomPixels> renderPoint = turningFloor is { } transform
            ? transform.Transform(samusPosition)
            : samusPosition;
        int screenX = CoordinateAxis.Wrap(renderPoint.X + xOffset - cameraPosition.X);
        int screenY = CoordinateAxis.Wrap(renderPoint.Y + yOffset - pose.PoseYOffset - cameraPosition.Y);

        // The flare is published wherever it lies; the display clips it to the view (#1541).

        // A component the Hyper Beam's count wrapped past its first picture may stand at a place that shows no flare
        // picture; it draws nothing there (#1275, Mother Brain glitch movie).
        if (ChargeFlarePictures.Of(component, pose.FacingLeft, _charge.FlarePhase(component)) is not { } picture)
            return;
        // The flare is Samus's own charge, so it shows the sheet of the beams she has equipped (#1586).
        SpriteSheetFrame image = artwork.Objects.SheetOf(SamusBeamCombinations.Of(samus.EquippedBeams)).Frame(ProjectileSheetFrames.FlareFrame(picture));
        sprites.Add(new ImageSpriteDraw(new SpriteSource(SpriteSourceKind.SamusChargeFlare, ChargeFlarePictures.DrawSlot(component)), image,
            Position16<ScreenPixels>.Of(screenX, screenY), SpriteMirror.None));
    }

    /// <summary>Clears all component frames and countdowns when charge-flare animation is retired.</summary>
    internal void ClearFlareAnimationState()
    {
        _charge.ClearFlare();
    }

    /// <summary>Restores sixteen suit colors from installed artwork.</summary>
    internal static void LoadNormalSuitPalette(
        SamusColours colours,
        SamusState samus) =>
        SamusNormalSuitPalette.Load(colours, samus.EquippedItems);

    /// <summary>The installed composition each timed record draws, which a projectile's records need bound.</summary>
    public SuperMetroid.Core.Samus.Artwork.InstalledProjectileFrameBindings? FrameBindings { get; set; }
}
