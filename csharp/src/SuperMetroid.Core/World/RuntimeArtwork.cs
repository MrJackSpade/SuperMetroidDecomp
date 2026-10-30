using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx;
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
    /// <summary>Receives animated tile, layer-three map, and blend-color catalogs for every room's FX.</summary>
    private readonly RoomFxSession _roomFx;

    /// <summary>
    /// Binds the installed projectile artwork to one room's Samus projectile and bomb systems: when the room builds them,
    /// and again whenever the host rebinds artwork.
    /// </summary>
    internal void BindProjectiles(SamusProjectileSystem projectiles, SamusBombProjectileSystem bombs)
    {
        Ensure.NotNull(projectiles);
        Ensure.NotNull(bombs);
        projectiles.Animation.FrameBindings = ProjectileFrameBindings;
        projectiles.Overreads = SamusOverreads;
        bombs.FrameBindings = ProjectileFrameBindings;
        bombs.PowerBombExplosion.PresentationColors = MapPresentation?.PowerBombFixedColors;
    }

    /// <summary>Captures the runtime owners that consume host-installed visual assets.</summary>
    internal RuntimeArtwork(RuntimeArtworkDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _room = dependencies.Room;
        _hud = dependencies.Hud;
        _player = dependencies.Player;
        _roomFx = dependencies.RoomFx;
    }
    /// <summary>Installed colors used at construction and on each room-entry palette restore.</summary>
    [field: NonSerialized]
    public GameplayBasePalette? InitialPaletteArt { get; set; }
    /// <summary>Gets or binds the host-owned presentation catalog used by maps, HUD, messages, room effects, Samus/enemy colors, and named VRAM assets.</summary>
    /// <remarks>Binding nonnull artwork rebinds the presentation owners; assigning null removes them. The HUD shows the BG3 word sheet's pictures, so no characters are shown again.</remarks>
    [field: NonSerialized]     public IGameplayPresentationContent? MapPresentation
    {
        get;
        set
        {
            field = value;
            _hud.BindPresentation(value?.GameplayHud, _player.Samus);
            _roomFx.AnimatedTileArtwork = value?.RoomFxAnimatedTiles;
            _roomFx.PaletteBlendColors = value?.RoomFxPaletteBlends;
            BindSamusPalettePresentation();
            _bossArtwork.EscapeTypewriterPresentation = value?.EscapeTypewriter;
            _bossArtwork.MotherBrainHealthColors = value?.MotherBrainHealthPalette;
            _bossArtwork.MotherBrainRainbowColors = value?.MotherBrainRainbowPalette;
            _bossArtwork.MotherBrainRoomColors = value?.MotherBrainRoomColors;
            _bossArtwork.CeresRidleyColors = value?.CeresRidleyColors;
            _bossArtwork.CeresRidleyFloorColors = value?.CeresRidleyFloorColors;
            _bossArtwork.CeresRidleyFloorUpdates = value?.CeresRidleyFloorUpdates;
            _room.Scope?.Enemies.RebindEscapeTypewriters(value?.EscapeTypewriter);
        }
    }

    /// <summary>Rebinds current artwork when a saved or newly constructed Samus becomes active.</summary>
    internal void BindSamusPalettePresentation()
    {
        if (_player.Samus is null)
            return;
        _player.Samus.VisorPalette.PresentationColors = MapPresentation?.SamusVisorColors;
        _player.Samus.Xray.PresentationColors = MapPresentation?.SamusVisorColors;
        _player.Samus.Drained.PresentationColors = MapPresentation?.SamusHyperBeamColors;
        _player.Samus.Presentation.SuitColors = MapPresentation?.SamusSuitColors;
        _player.Samus.Presentation.FullBodyCycleColors = MapPresentation?.SamusFullBodyCycleColors;
        _player.Samus.Presentation.ChargeColors = MapPresentation?.SamusChargeColors;
        _player.Samus.Presentation.AnimationWindows = SamusOverreads?.AnimationWindows;
        // All Samus constructors call this rebind before priming the first visual DMA.
        _player.Samus.Presentation.BodyFrames.BindArtwork(SamusBodyArt);
        _player.Samus.Presentation.BodyFrames.BindSheets(SpriteSheets);
        _player.Samus.ArmCannon.Artwork = SamusBodyArt?.ArmCannon;
    }

    /// <summary>Rebinds authored visual frame choices to both native projectile slot owners.</summary>
    [field: NonSerialized]     public InstalledProjectileFrameBindings? ProjectileFrameBindings
    {
        get;
        set;
    }
    /// <summary>Installed visual-only charge-flare offsets selected by pose movement and direction.</summary>
    [field: NonSerialized]
    public ChargeFlarePlacements? ChargeFlarePlacement { get; set; }

    /// <summary>Current Grapple flare offsets.</summary>
    [field: NonSerialized]     public ChargeFlarePlacements? GrappleFlarePlacement
    {
        get;
        set { field = value; BindGrapplePresentation(); }
    }

    /// <summary>Current frame Samus's swing pose shows at each grapple angle.</summary>
    [field: NonSerialized]     public InstalledGrappleSwingFrames? GrappleSwingFrames
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

    /// <summary>The installed cartridge bytes Samus's glitch paths read; the projectile system binds them.</summary>
    [field: NonSerialized]
    public InstalledSamusOverreads? SamusOverreads
    {
        get;
        set
        {
            field = value;
            if (_player.Samus is { } samus)
                samus.Presentation.AnimationWindows = value?.AnimationWindows;
        }
    }

    /// <summary>The installed pre-coloured sprite sheets drawn families select their frames from.</summary>
    [field: NonSerialized]
    public InstalledSpriteSheets? SpriteSheets
    {
        get;
        set
        {
            field = value;
            ProjectileSheets = value is null ? null : SuperMetroid.Core.Samus.Artwork.ProjectileSheets.Gameplay(value);
            _player.Samus?.Presentation.BodyFrames.BindSheets(value);
        }
    }

    /// <summary>The projectile sheets Samus's projectiles, bombs, charge flare and trails draw from.</summary>
    [field: NonSerialized]
    public SuperMetroid.Core.Samus.Artwork.ProjectileSheets? ProjectileSheets { get; private set; }

    /// <summary>Beam palettes bound since the last accepted NMI, which that NMI publishes.</summary>
    [NonSerialized] private InstalledBeamPalettes? pendingBeamPalette;

    /// <summary>Current beam palettes.</summary>
    [field: NonSerialized]     public InstalledBeamPalettes? BeamPalettes
    {
        get;
        set { field = value; pendingBeamPalette = value; }
    }

    /// <summary>Current Hyper Beam palette effect colours.</summary>
    [field: NonSerialized]
    public HyperBeamFxColours? HyperBeamFxColors { get; set; }

    /// <summary>
    /// The equipped-beam word whose characters <c>Update_Beam_Tiles_and_Palette</c> ($90:AC8D) last loaded. The
    /// projectile sheets show this beam's characters until the next update, even when code changes the equipment in
    /// between: the Golden Torizo code grants beams during room initialization, after the entering beam was loaded.
    /// </summary>
    public SuperMetroid.Core.Progression.SamusBeamFlags LoadedBeams { get; private set; }

    /// <summary>
    /// Ports <c>Update_Beam_Tiles_and_Palette</c> at $90:AC8D: latches the beam whose characters the projectile sheets
    /// show and loads its palette.
    /// </summary>
    internal void UpdateBeamTilesAndPalette(SamusColours colours, SuperMetroid.Core.Progression.SamusBeamFlags equippedBeams)
    {
        LoadedBeams = equippedBeams;
        SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.LoadBeamPalette(colours.Beam, equippedBeams, BeamPalettes);
    }

    /// <summary>
    /// Loads newly bound beam palettes for the equipped beams at an accepted NMI. The characters need no
    /// publication: the projectile sheets are pre-composed for every beam combination.
    /// </summary>
    internal void PublishReboundBeamPalette()
    {
        if (pendingBeamPalette is not { } beamPalettes || _player.Samus is not { } samus) return;
        // Do not reinterpret invalid combination overreads as a legal replacement.
        if (new SuperMetroid.Core.Samus.Projectiles.SamusBeamLoadoutWord(samus.EquippedBeams).Combination is SuperMetroid.Core.Progression.SamusBeamCombination selection &&
            selection.IsRetail &&
            // An in-flight effect owns these colors across frames. Its own completion
            // restores the selected normal palette; host rebind must not erase its phase.
            samus.CrystalFlash.SpecialPaletteKind != SuperMetroid.Core.Samus.Abilities.SamusSpecialPaletteType.CrystalFlash &&
            !samus.Drained.HyperBeamPaletteFx.IsActive)
            beamPalettes.LoadTo(_player.Colours.Beam, selection);
        pendingBeamPalette = null;
    }


    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    public InstalledRoomArtwork? RoomBlockSheets { get; set; }
    /// <summary>Host-selected base room palettes, rebound after debugger-state restore.</summary>
    [field: NonSerialized]
    public RoomStaticPalettes? RoomPaletteArt { get; set; }

    /// <summary>Host-selected visual room layouts, rebound after debugger-state restore.</summary>
    [field: NonSerialized]
    public RoomLevelArtworks? RoomVisualLayouts { get; set; }
    /// <summary>Host-bound installed terrain of every room level, rebound after debugger-state restore.</summary>
    [field: NonSerialized]
    public InstalledRoomLevelStreams? RoomLevelStreams { get; set; }
    /// <summary>Host-owned Samus visual tiles; restored and future Samus states use the same edit.</summary>
    [field: NonSerialized]     public InstalledSamusBodyArtwork? SamusBodyArt
    {
        get;
        set
        {
            field = value;
            _player.Samus?.Presentation.BodyFrames.BindArtwork(value);
            if (_player.Samus is not null) _player.Samus.ArmCannon.Artwork = value?.ArmCannon;
        }
    }
}
