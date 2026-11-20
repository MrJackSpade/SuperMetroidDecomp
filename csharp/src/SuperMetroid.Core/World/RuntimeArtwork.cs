using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>
/// Host-installed artwork catalogs (room pictures, colours, layouts, sky, Samus, beams, grapple, map presentation) the
/// runtime's owners draw and colour from. Every catalog is installed from the start: the runtime is built over the
/// host's installed artwork (#2049), and a rebinding replaces a catalog with another installed one.
/// </summary>
/// <remarks>
/// The runtime binds a whole installation at once, setting only the catalogs whose reference changed; each setter carries one catalog to every owner that draws from it (#2290, #2487). A catalog is
/// set by itself because the owners each one reaches differ, not because a binding replaces part of an installation.
/// </remarks>
[ArchitectureReview("context-bag", "The runtime's binding of host-installed artwork: its setters rebind the HUD, the boss artwork, the loaded room's enemies and the Samus artwork binding when the host binds new content; every other member is an immutable artwork catalog, not a service.")]
internal sealed class RuntimeArtwork
{

    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;
    /// <summary>The loaded room, whose active escape typewriters follow a rebound escape-warning text.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Receives gameplay HUD presentation and reports initialization state.</summary>
    private readonly HudState _hud;

    /// <summary>The map presentation's backing value, installed from the start.</summary>
    private IGameplayPresentationContent _screenContent;

    /// <summary>The sprite sheets' backing value, installed from the start.</summary>
    private InstalledSpriteSheets _spriteSheets;

    /// <summary>Captures the runtime owners that consume host-installed visual assets and the catalogs the runtime is built over.</summary>
    internal RuntimeArtwork(RuntimeArtworkDependencies dependencies)
    {
        Ensure.NotNull(dependencies);
        _bossArtwork = dependencies.BossArtwork;
        _room = dependencies.Room;
        _hud = dependencies.Hud;
        RuntimeArtworkDependencies installed = dependencies;
        _screenContent = installed.ScreenContent;
        _spriteSheets = installed.SpriteSheets;
        ProjectileSheets = SuperMetroid.Core.Samus.Artwork.ProjectileSheets.Gameplay(installed.SpriteSheets);
        SamusArtwork = new SamusArtworkBinding(installed.SamusBodyArt, installed.SamusShownFrames, installed.SpriteSheets, installed.GrappleFlarePlacement,
            installed.GrappleSwingFrames, installed.ProjectileFrameBindings, installed.ScreenContent.PowerBombGlowColors);
        ChargeFlarePlacement = installed.ChargeFlarePlacement;
        RoomBlockSheets = installed.RoomBlockSheets;
        RoomVisualLayouts = installed.RoomVisualLayouts;
        RoomLevelStreams = installed.RoomLevelStreams;
    }

    /// <summary>Gets or binds the host-owned presentation catalog used by maps, HUD, messages, room effects, Samus/enemy colors, and named artwork assets.</summary>
    /// <remarks>Binding rebinds the presentation owners. The HUD shows the HUD sheet's pictures, so binding has no HUD artwork to show again.</remarks>
    public IGameplayPresentationContent ScreenContent
    {
        get => _screenContent;
        set
        {
            _screenContent = Ensure.NotNull(value);
            SamusArtwork.PowerBombGlowColors = value.PowerBombGlowColors;
            _hud.BindPresentation(value.GameplayHud);
            _bossArtwork.EscapeTypewriterPresentation = value.EscapeTypewriter;
            _bossArtwork.MotherBrainRainbowColors = value.MotherBrainRainbowBeamColours;
            _bossArtwork.CeresRidleyGetawayBoostDelays = value.CeresRidleyGetawayBoostDelays;
            _room.Scope?.Enemies.RebindEscapeTypewriters(value.EscapeTypewriter);
        }
    }

    /// <summary>
    /// The session's Samus artwork, which every Samus the runtime builds draws from, so a rebinding reaches the Samus
    /// already playing (#2049).
    /// </summary>
    internal SamusArtworkBinding SamusArtwork { get; }

    /// <summary>The composition every frame of Samus's shots' and bombs' animations draws: the Samus artwork's, which a rebinding replaces for both.</summary>
    public InstalledProjectileFrameBindings ProjectileFrameBindings
    {
        set => SamusArtwork.ProjectileFrames = value;
    }

    /// <summary>Installed visual-only charge-flare offsets selected by pose movement and direction.</summary>
    public ChargeFlarePlacements ChargeFlarePlacement { get; set; }

    /// <summary>Current Grapple flare offsets.</summary>
    public ChargeFlarePlacements GrappleFlarePlacement
    {
        get => SamusArtwork.GrappleFlarePlacement;
        set => SamusArtwork.GrappleFlarePlacement = value;
    }

    /// <summary>Current frame Samus's swing pose shows at each grapple angle.</summary>
    public InstalledGrappleSwingFrames GrappleSwingFrames
    {
        get => SamusArtwork.GrappleSwingFrames;
        set => SamusArtwork.GrappleSwingFrames = value;
    }

    /// <summary>The installed pre-coloured sprite sheets drawn families select their frames from.</summary>
    public InstalledSpriteSheets SpriteSheets
    {
        get => _spriteSheets;
        set
        {
            _spriteSheets = Ensure.NotNull(value);
            ProjectileSheets = SuperMetroid.Core.Samus.Artwork.ProjectileSheets.Gameplay(value);
            SamusArtwork.Sheets = value;
        }
    }

    /// <summary>The projectile sheets Samus's projectiles, bombs, charge flare and trails draw from.</summary>
    public SuperMetroid.Core.Samus.Artwork.ProjectileSheets ProjectileSheets { get; private set; }

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    public InstalledRoomArtwork RoomBlockSheets { get; set; }

    /// <summary>Installed visual room layouts, rebound when the host binds new artwork.</summary>
    public RoomLevelArtworks RoomVisualLayouts { get; set; }
    /// <summary>Installed terrain of every room level, rebound when the host binds new artwork.</summary>
    public InstalledRoomLevelStreams RoomLevelStreams { get; set; }
    /// <summary>Host-owned Samus pictures; restored and future Samus states use the same edit.</summary>
    public InstalledSamusBodyArtwork SamusBodyArt
    {
        get => SamusArtwork.Body;
        set
        {
            SamusArtwork.Body = value;
            SamusArtwork.ArmCannon = value.ArmCannon;
        }
    }

    /// <summary>Host-owned frames Samus's body can show and the pictures they draw; restored and future Samus states use the same edit.</summary>
    public SuperMetroid.Core.Samus.Drawing.SamusShownFrames SamusShownFrames
    {
        get => SamusArtwork.ShownFrames;
        set => SamusArtwork.ShownFrames = value;
    }
}
