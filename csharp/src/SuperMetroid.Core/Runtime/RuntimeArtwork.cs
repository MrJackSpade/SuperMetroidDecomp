using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Host-installed artwork catalogs (room tiles, palettes, layouts, sky, Samus, beams, grapple, map presentation)
/// and the VRAM asset and installed-artwork transfer source the runtime's VRAM queue resolves through.
/// </summary>
internal sealed class RuntimeArtwork : IVramAssetProvider, IInstalledArtworkTransferSource
{
    /// <summary>Resolves a legacy native-source DMA record from currently installed artwork.</summary>
    bool IInstalledArtworkTransferSource.TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data) =>
        TryResolveInstalledArtwork(sourceAddress, byteCount, out data);

    /// <summary>Resolves a typed VRAM asset from its currently bound artwork catalog.</summary>
    ReadOnlyMemory<byte> IVramAssetProvider.Resolve(VramAssetId asset) => ResolveVramAsset(asset);

    /// <summary>Supplies Power Bomb presentation colors on artwork rebind.</summary>
    private readonly SamusBombProjectileSystem _bombProjectiles;
    /// <summary>Receives selected beam palettes during a safe rebind.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Locates the current HUD character base in VRAM.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Receives installed enemy presentation and supplies enemy tile transfers.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>Receives gameplay HUD presentation and reports initialization state.</summary>
    private readonly HudState _hud;
    /// <summary>Receives installed message title, panel, and notice presentation.</summary>
    private readonly GameplayMessageBoxState _messageBox;
    /// <summary>Receives Samus palette, grapple, and tile-art bindings.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Receives compiled projectile frame composition bindings.</summary>
    private readonly SamusProjectileSystem _projectiles;
    /// <summary>Receives animated tile, layer-three map, and blend-color catalogs.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;
    /// <summary>Destination for accepted-NMI artwork refresh transfers.</summary>
    private readonly SnesVram _vram;
    /// <summary>Pending DMA records rebound from native source addresses to installed assets.</summary>
    private readonly VramWriteQueue _vramWrites;

    /// <summary>Captures the runtime owners that consume host-installed visual assets.</summary>
    internal RuntimeArtwork(RuntimeArtworkDependencies dependencies)
    {
        _bombProjectiles = dependencies.BombProjectiles;
        _cgram = dependencies.Cgram;
        _display = dependencies.Display;
        _enemies = dependencies.Enemies;
        _hud = dependencies.Hud;
        _messageBox = dependencies.MessageBox;
        _player = dependencies.Player;
        _projectiles = dependencies.Projectiles;
        _roomLayer3Fx = dependencies.RoomLayer3Fx;
        _vram = dependencies.Vram;
        _vramWrites = dependencies.VramWrites;
    }
    /// <summary>Installed colors used at construction and on each room-entry palette restore.</summary>
    [field: NonSerialized]
    public GameplayBasePaletteCatalog? InitialPaletteArt { get; set; }
    /// <summary>Whether the next accepted NMI must refresh already initialized HUD characters.</summary>
    [NonSerialized] private HudTileAtlas? pendingHudTiles;
    /// <summary>Gets or binds the host-owned presentation catalog used by maps, HUD, messages, room effects, Samus/enemy colors, and named VRAM assets.</summary>
    /// <remarks>Binding nonnull artwork schedules an initialized HUD refresh and rebinds compiled bus-source identities; assigning null removes presentation owners without rewriting already published VRAM.</remarks>
    [field: NonSerialized]     public AreaMapPresentationCatalog? MapPresentation
    {
        get;
        set
        {
            field = value;
            _hud.BindPresentation(value?.GameplayHud, _player.Samus);
            _messageBox.BindPresentation(value?.GameplayMessageTitles, value?.GameplayMessagePanels,
                value?.GameplayMessageNotices);
            _roomLayer3Fx.AnimatedTileArtwork = value?.RoomFxAnimatedTiles;
            _roomLayer3Fx.Layer3Tilemaps = value?.RoomFxLayer3Tilemaps;
            _roomLayer3Fx.PaletteBlendColors = value?.RoomFxPaletteBlends;
            _bombProjectiles.PowerBombExplosion.PresentationColors = value?.PowerBombFixedColors;
            BindSamusPalettePresentation();
            _enemies.BindMapPresentation(value);
            pendingHudTiles = value is not null && _hud.IsInitialized ? value.HudTiles : null;
            if (value is not null)
                RebindLegacyArtworkTransfers();
        }
    }

    /// <summary>
    /// Points pending transfers a restored legacy queue still sources from the cartridge bus at the
    /// installed HUD, Kraid BG3 and escape-timer artwork, matched by their native source and size.
    /// </summary>
    [SuperMetroid.Core.Hardware.PpuLayoutBoundary("codec: legacy queued VRAM DMA records rebound from native bus sources to installed artwork")]
    private void RebindLegacyArtworkTransfers()
    {
        _vramWrites.RebindBusSource(HudTileAtlasFormat.SourceAddress, HudTileAtlasFormat.TransferByteCount,
            SuperMetroid.Core.Hardware.VramAssetId.StandardHudTiles);
        for (int quarter = 0; quarter < Game.KraidBackgroundRomData.StandardBg3TransferCount;
             quarter++)
            _vramWrites.RebindBusSource(
                Game.KraidBackgroundRomData.StandardBg3TilesAddress +
                    quarter * Game.KraidBackgroundRomData.StandardBg3TransferBytes,
                Game.KraidBackgroundRomData.StandardBg3TransferBytes,
                Game.KraidBackgroundRomData.StandardBg3AssetForQuarter(quarter));
        _vramWrites.RebindBusSource(EscapeTimerTileRomData.FirstSourceAddress,
            EscapeTimerTileAtlasFormat.FirstByteCount,
            SuperMetroid.Core.Hardware.VramAssetId.EscapeTimerFirstTiles);
        _vramWrites.RebindBusSource(EscapeTimerTileRomData.SecondSourceAddress,
            EscapeTimerTileAtlasFormat.SecondByteCount,
            SuperMetroid.Core.Hardware.VramAssetId.EscapeTimerSecondTiles);
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
        // All Samus constructors call this rebind before priming the first visual DMA.
        _player.Samus.Presentation.TileTransfers.BindArtwork(SamusBodyArt);
        _player.Samus.ArmCannon.Artwork = SamusBodyArt?.ArmCannon;
    }

    /// <summary>Publishes rebound HUD characters at the active PPU character base on accepted NMI.</summary>
    internal void PublishReboundHudArtwork()
    {
        if (pendingHudTiles is not { } hudTiles) return;
        // Binding a restored session changes content ownership, not the already
        // published frame. Refresh at accepted NMI, before native queued writes.
        // The original upload's padding may now contain live BG2 tilemaps.
        // Kraid's room changes BG34NBA to place HUD characters at $2000 while
        // his private BG2 tilemap occupies the ordinary $4000 HUD character
        // address. A restored-state art rebind must honor the *current* PPU
        // character base, or the next NMI paints HUD pixels across Kraid.
        hudTiles.LoadCharactersTo(_display.HudCharactersAtCurrentBase(_vram));
        pendingHudTiles = null;
    }
    /// <summary>Matches a native DMA source and exact length to the currently installed immutable visual data.</summary>
    internal bool TryResolveInstalledArtwork(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> data)
    {
        // Old pending native records and new typed records share the selected
        // artwork. Do not send these immutable sources to the RAM-only DMA path.
        if (MapPresentation?.HudTiles.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (MapPresentation?.EscapeTimerTiles.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (BeamArtwork?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (GrappleArtwork?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (MapPresentation is not null &&
            sourceAddress == GameplayHudDefinitions.TopRowAddress &&
            byteCount == GameplayHudDefinitions.TopRowByteCount)
        {
            data = MapPresentation.GameplayHud.TopRowTransfer;
            return true;
        }
        if (StandardObjectArt is not null &&
            sourceAddress == StandardObjectArtworkAddresses.Source &&
            byteCount == StandardObjectArtworkFormat.TransferByteCount)
        {
            data = StandardObjectArt.Transfer;
            return true;
        }
        if (SamusBodyArt?.DeathTiles.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (SamusBodyArt?.ArmCannon.TryResolveTile(sourceAddress, byteCount, out data) == true)
            return true;
        if (_enemies.TileArtwork?.GunshipLiftoff?.TryResolve(
                sourceAddress, byteCount, out data) == true)
            return true;
        if (_enemies.TileArtwork?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (MapPresentation?.RoomFxAnimatedTiles.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (RoomSkyTilemapArt is not null)
            return RoomSkyTilemapArt.TryResolve(sourceAddress, byteCount, out data);
        data = default;
        return false;
    }

    // Host content is rebound after restoring a graph; saved state must not freeze
    // an old user override into the simulation. Only composition emission uses this.
    /// <summary>Complete installed standard OBJ sheet used by the queued gameplay DMA.</summary>
    [field: NonSerialized]
    public RoomCharacterAtlas? StandardObjectArt { get; set; }

    /// <summary>Rebinds authored visual frame choices to both native projectile slot owners.</summary>
    [field: NonSerialized]     public ProjectileFrameBindingCatalog? ProjectileFrameBindings
    {
        get;
        set
        {
            field = value;
            _projectiles.Animation.FrameBindings = value;
            _bombProjectiles.FrameBindings = value;
        }
    }
    /// <summary>Installed visual-only charge-flare offsets selected by pose movement and direction.</summary>
    [field: NonSerialized]
    public ChargeFlarePlacementCatalog? ChargeFlarePlacement { get; set; }
    /// <summary>Installed charge-flare OAM compositions selected independently of timing and damage.</summary>
    [field: NonSerialized]
    public ChargeFlareSpriteCatalog? ChargeFlareCompositions { get; set; }

    /// <summary>Current Grapple artwork and visual definitions. Rebinding legacy pending transfers preserves their NMI order and destination.</summary>
    [field: NonSerialized]     public GrappleTileAtlas? GrappleArtwork
    {
        get;
        set { field = value; value?.RebindPendingWrites(_vramWrites); BindGrapplePresentation(); }
    }
    /// <summary>Rebinds current Grapple flare and swing visuals to an active Samus actor.</summary>
    internal void BindGrapplePresentation()
    {
        if (_player.Samus is not null)
        {
            _player.Samus.Grapple.FlarePlacement = GrappleArtwork?.FlarePlacement;
            _player.Samus.Grapple.SwingFrames = GrappleArtwork?.SwingFrames;
        }
    }

    /// <summary>Whether accepted NMI must publish rebound projectile trail characters.</summary>
    [NonSerialized] private ProjectileTrailAtlas? pendingTrailTiles;
    /// <summary>Current projectile-trail tiles and visual compositions; rebinding refreshes installed tiles at the next display boundary.</summary>
    [field: NonSerialized]     public ProjectileTrailCatalog? TrailArtwork
    {
        get;
        set { field = value; pendingTrailTiles = value?.Tiles; }
    }

    /// <summary>Whether accepted NMI must publish the current beam tiles and normal palette.</summary>
    [NonSerialized] private BeamTileCatalog? pendingBeamArtwork;

    /// <summary>Current beam tiles. Rebinding changes the next accepted display, not the retained frame.</summary>
    [field: NonSerialized]     public BeamTileCatalog? BeamArtwork
    {
        get;
        set { field = value; pendingBeamArtwork = value; }
    }

    /// <summary>Dispatches a typed VRAM asset to its bound gunship, Grapple, trail, HUD, or beam catalog.</summary>
    internal ReadOnlyMemory<byte> ResolveVramAsset(VramAssetId asset) =>
        asset is VramAssetId.GunshipLiftoffFirstTiles or
            VramAssetId.GunshipLiftoffSecondTiles or
            VramAssetId.GunshipLiftoffThirdTiles or
            VramAssetId.GunshipLiftoffFourthTiles or
            VramAssetId.GunshipLiftoffFifthTiles
            ? (_enemies.TileArtwork?.GunshipLiftoff ??
                throw new InvalidOperationException("Gunship takeoff artwork is not bound.")).Resolve(asset)
            : asset is VramAssetId.GrapplePointFirstTiles or VramAssetId.GrapplePointSecondTiles or
            VramAssetId.GrapplePointThirdTiles or VramAssetId.GrapplePointFourthTiles or
            VramAssetId.GrappleHorizontalSegmentTiles or VramAssetId.GrappleDiagonalSegmentTiles or VramAssetId.GrappleVerticalSegmentTiles
            ? (GrappleArtwork ?? throw new InvalidOperationException("Grapple artwork is not bound.")).Resolve(asset)
            : asset is VramAssetId.ProjectileIceWaveTrailTiles or VramAssetId.ProjectileMissileTrailTiles
            ? (TrailArtwork?.Tiles ?? throw new InvalidOperationException("Trail artwork is not bound.")).Resolve(asset)
            : asset is VramAssetId.StandardHudTiles or
                VramAssetId.KraidBg3RestoreQuarter0 or VramAssetId.KraidBg3RestoreQuarter1 or
                VramAssetId.KraidBg3RestoreQuarter2 or VramAssetId.KraidBg3RestoreQuarter3 or
                VramAssetId.EscapeTimerFirstTiles or VramAssetId.EscapeTimerSecondTiles
            ? (MapPresentation ?? throw new InvalidOperationException("Map presentation artwork is not bound.")).Resolve(asset)
            : (BeamArtwork ?? throw new InvalidOperationException("Beam artwork is not bound.")).Resolve(asset);

    /// <summary>Publishes rebound projectile trails after any restored legacy queue drains.</summary>
    internal void PublishReboundTrailArtwork()
    {
        if (pendingTrailTiles is not { } trailTiles) return;
        // A restored legacy queue can still contain the standard OBJ upload. Publish
        // current host art only after that queue drains, and only on accepted NMIs.
        trailTiles.LoadTo(_vram);
        pendingTrailTiles = null;
    }

    /// <summary>Publishes the retail-selected beam art while preserving active special palette effects.</summary>
    internal void PublishReboundBeamArtwork()
    {
        if (pendingBeamArtwork is not { } beamArtwork || _player.Samus is not { } samus) return;
        // Do not reinterpret invalid combination overreads as a legal replacement.
        if (new Game.SamusBeamLoadoutWord(samus.EquippedBeams).Combination is Game.SamusBeamCombination selection &&
            selection.IsRetail)
        {
            new Game.SamusVram(_vram).BeamTiles.Load(beamArtwork.Resolve(BeamTileCatalog.AssetFor(selection)).Span);
            // An in-flight effect owns these colors across frames. Its own completion
            // restores the selected normal palette; host rebind must not erase its phase.
            if (samus.CrystalFlash.SpecialPaletteKind != Game.SamusSpecialPaletteType.CrystalFlash &&
                !samus.Drained.HyperBeamPaletteFx.IsActive)
                beamArtwork.Palettes?.LoadTo(_cgram, selection);
        }
        pendingBeamArtwork = null;
    }

    /// <summary>Current external timed-projectile composition catalog, independent of mechanics.</summary>
    [field: NonSerialized]
    public ProjectileSpriteCatalog? ProjectileCompositions { get; set; }
    /// <summary>Host-selected BG tilemaps, rebound after debugger-state restoration.</summary>
    [field: NonSerialized]
    public RoomBackgroundTilemapCatalog? RoomBackgroundTilemapArt { get; set; }
    /// <summary>Gets or binds the host-owned CRE and graphics-set character catalog used by subsequent room loads; the reference is excluded from debugger snapshots.</summary>
    [field: NonSerialized]
    public RoomCharacterAtlasCatalog? RoomCharacterArt { get; set; }
    /// <summary>Host-selected visual block compositions, rebound after debugger-state restore.</summary>
    [field: NonSerialized]
    public RoomMetatileCatalog? RoomMetatileArt { get; set; }
    /// <summary>Host-selected base room palettes, rebound after debugger-state restore.</summary>
    [field: NonSerialized]
    public RoomStaticPaletteCatalog? RoomPaletteArt { get; set; }
    /// <summary>Installed scrolling-sky pages, rebound after debugger-state restoration.</summary>
    [field: NonSerialized]
    public RoomSkyTilemapCatalog? RoomSkyTilemapArt { get; set; }
    /// <summary>Host-selected visual room layouts, rebound after debugger-state restore.</summary>
    [field: NonSerialized]
    public RoomVisualLayoutCatalog? RoomVisualLayouts { get; set; }
    /// <summary>Host-bound stock native level allocations, rebound after debugger-state restore.</summary>
    [field: NonSerialized]
    public RoomLevelStreamCatalog? RoomLevelStreams { get; set; }
    /// <summary>Host-owned Samus visual tiles; restored and future Samus states use the same edit.</summary>
    [field: NonSerialized]     public SamusBodyArtworkCatalog? SamusBodyArt
    {
        get;
        set
        {
            field = value;
            _player.Samus?.Presentation.TileTransfers.BindArtwork(value);
            if (_player.Samus is not null) _player.Samus.ArmCannon.Artwork = value?.ArmCannon;
        }
    }
}
