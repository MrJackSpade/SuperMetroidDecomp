using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>
/// Host-installed artwork catalogs (room tiles, palettes, layouts, sky, Samus, beams, grapple, map presentation) the
/// runtime's owners draw and colour from.
/// </summary>
internal sealed class RuntimeArtwork
{

    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;
    /// <summary>The loaded room, whose active escape typewriters follow a rebound escape-warning text.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Receives gameplay HUD presentation and reports initialization state.</summary>
    private readonly HudState _hud;
    /// <summary>Receives Samus palette, grapple, and tile-art bindings.</summary>
    private readonly SamusFrameState _player;

    /// <summary>
    /// Binds the installed projectile artwork to one room's Samus projectile and bomb systems: when the room builds them,
    /// and again whenever the host rebinds artwork.
    /// </summary>
    internal void BindProjectiles(SamusProjectileSystem projectiles, SamusBombProjectileSystem bombs)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(bombs);
        projectiles.Animation.FrameBindings = ProjectileFrameBindings;
        bombs.FrameBindings = ProjectileFrameBindings;
        bombs.PowerBombExplosion.PresentationColors = MapPresentation?.PowerBombGlowColors;
    }

    /// <summary>Captures the runtime owners that consume host-installed visual assets.</summary>
    internal RuntimeArtwork(RuntimeArtworkDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _room = dependencies.Room;
        _hud = dependencies.Hud;
        _player = dependencies.Player;
    }
    /// <summary>Gets or binds the host-owned presentation catalog used by maps, HUD, messages, room effects, Samus/enemy colors, and named artwork assets.</summary>
    /// <remarks>Binding nonnull artwork rebinds the presentation owners; assigning null removes them. The HUD shows the HUD word sheet's pictures, so no characters are shown again.</remarks>
    public IGameplayPresentationContent? MapPresentation
    {
        get;
        set
        {
            field = value;
            _hud.BindPresentation(value?.GameplayHud);
            BindSamusPalettePresentation();
            _bossArtwork.EscapeTypewriterPresentation = value?.EscapeTypewriter;
            _bossArtwork.MotherBrainRainbowColors = value?.MotherBrainRainbowPalette;
            _bossArtwork.CeresRidleyGetawayBoostDelays = value?.CeresRidleyGetawayBoostDelays;
            _room.Scope?.Enemies.RebindEscapeTypewriters(value?.EscapeTypewriter);
        }
    }

    /// <summary>Rebinds current artwork when a saved or newly constructed Samus becomes active.</summary>
    internal void BindSamusPalettePresentation()
    {
        if (_player.Samus is null)
            return;
        // All Samus constructors call this rebind before her first draw.
        _player.Samus.Presentation.BindArtwork(SamusBodyArt);
        _player.Samus.Presentation.BindSheets(SpriteSheets);
        _player.Samus.ArmCannon.Artwork = SamusBodyArt?.ArmCannon;
    }

    /// <summary>Rebinds authored visual frame choices to both projectile slot owners.</summary>
    public InstalledProjectileFrameBindings? ProjectileFrameBindings
    {
        get;
        set;
    }
    /// <summary>Installed visual-only charge-flare offsets selected by pose movement and direction.</summary>
    public ChargeFlarePlacements? ChargeFlarePlacement { get; set; }

    /// <summary>Current Grapple flare offsets.</summary>
    public ChargeFlarePlacements? GrappleFlarePlacement
    {
        get;
        set { field = value; BindGrapplePresentation(); }
    }

    /// <summary>Current frame Samus's swing pose shows at each grapple angle.</summary>
    public InstalledGrappleSwingFrames? GrappleSwingFrames
    {
        get;
        set { field = value; BindGrapplePresentation(); }
    }

    /// <summary>Rebinds current Grapple flare and swing visuals to an active Samus actor.</summary>
    internal void BindGrapplePresentation()
    {
        if (_player.Samus is not null)
        {
            _player.Samus.Grapple.FlarePlacement = GrappleFlarePlacement;
            _player.Samus.Grapple.SwingFrames = GrappleSwingFrames;
        }
    }

    /// <summary>The installed pre-coloured sprite sheets drawn families select their frames from.</summary>
    public InstalledSpriteSheets? SpriteSheets
    {
        get;
        set
        {
            field = value;
            ProjectileSheets = value is null ? null : SuperMetroid.Core.Samus.Artwork.ProjectileSheets.Gameplay(value);
            _player.Samus?.Presentation.BindSheets(value);
        }
    }

    /// <summary>The projectile sheets Samus's projectiles, bombs, charge flare and trails draw from.</summary>
    public SuperMetroid.Core.Samus.Artwork.ProjectileSheets? ProjectileSheets { get; private set; }

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    public InstalledRoomArtwork? RoomBlockSheets { get; set; }

    /// <summary>Installed visual room layouts, rebound when a saved game state is restored.</summary>
    public RoomLevelArtworks? RoomVisualLayouts { get; set; }
    /// <summary>Installed terrain of every room level, rebound when a saved game state is restored.</summary>
    public InstalledRoomLevelStreams? RoomLevelStreams { get; set; }
    /// <summary>Host-owned Samus visual tiles; restored and future Samus states use the same edit.</summary>
    public InstalledSamusBodyArtwork? SamusBodyArt
    {
        get;
        set
        {
            field = value;
            _player.Samus?.Presentation.BindArtwork(value);
            if (_player.Samus is not null) _player.Samus.ArmCannon.Artwork = value?.ArmCannon;
        }
    }
}
