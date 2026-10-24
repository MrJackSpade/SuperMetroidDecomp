using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.World;

/// <summary>
/// Host-installed artwork catalogs (room tiles, palettes, layouts, sky, Samus, beams, grapple, map presentation)
/// and the VRAM asset and installed-artwork transfer source the runtime's VRAM queue resolves through.
/// </summary>
internal sealed class RuntimeArtwork : IVramAssetProvider, IInstalledArtworkTransferSource, IObjCharacterSourceResolver,
    IInstalledObjCharacterArtwork, IInstalledBossObjCharacterArtwork, IInstalledAreaObjCharacterArtwork
{
    /// <summary>Resolves an OBJ binding's installed VRAM asset.</summary>
    ReadOnlyMemory<byte> IObjCharacterSourceResolver.ResolveAsset(VramAssetId asset) => ResolveVramAsset(asset);

    /// <summary>Resolves an OBJ binding's native upload through the installed artwork that served it.</summary>
    /// <exception cref="InvalidOperationException">No bound artwork serves the upload any more.</exception>
    ReadOnlyMemory<byte> IObjCharacterSourceResolver.ResolveTransfer(int sourceAddress, int byteCount) =>
        TryResolveInstalledArtwork(sourceAddress, byteCount, out ReadOnlyMemory<byte> data) ? data
            : throw new InvalidOperationException($"No bound artwork serves the OBJ upload from ${sourceAddress:X6} ({byteCount} bytes).");

    /// <inheritdoc/>
    BossArtworkCatalog IInstalledBossObjCharacterArtwork.BossTiles => _bossArtwork.Installed;

    /// <inheritdoc/>
    AreaArtworkCatalog IInstalledAreaObjCharacterArtwork.AreaTiles => _areaArtwork.Installed;

    /// <inheritdoc/>
    EnemyTileArtworkCatalog IInstalledObjCharacterArtwork.EnemyTiles =>
        _enemySession.TileArtwork ?? throw new InvalidOperationException("No enemy artwork is installed.");

    /// <inheritdoc/>
    RoomFxAnimatedTileAtlas IInstalledObjCharacterArtwork.AnimatedTiles =>
        MapPresentation?.RoomFxAnimatedTiles ?? throw new InvalidOperationException("No map presentation is installed.");

    /// <summary>Resolves a legacy native-source DMA record from currently installed artwork.</summary>
    bool IInstalledArtworkTransferSource.TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data) =>
        TryResolveInstalledArtwork(sourceAddress, byteCount, out data);

    /// <summary>Resolves a typed VRAM asset from its currently bound artwork catalog.</summary>
    ReadOnlyMemory<byte> IVramAssetProvider.Resolve(VramAssetId asset) => ResolveVramAsset(asset);

    /// <summary>Locates the current HUD character base in VRAM.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Receives enemy artwork and presentation catalogs for every room's enemies.</summary>
    private readonly EnemySession _enemySession;

    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;

    /// <summary>The session's installed area artwork.</summary>
    private readonly AreaArtworkBinding _areaArtwork;
    /// <summary>The loaded room, whose active escape typewriters follow a rebound escape-warning text.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Receives gameplay HUD presentation and reports initialization state.</summary>
    private readonly HudState _hud;
    /// <summary>Receives installed message title, panel, and notice presentation.</summary>
    private readonly GameplayMessageBoxState _messageBox;
    /// <summary>Receives Samus palette, grapple, and tile-art bindings.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Receives animated tile, layer-three map, and blend-color catalogs for every room's FX.</summary>
    private readonly RoomFxSession _roomFx;
    /// <summary>Pending DMA records rebound from native source addresses to installed assets.</summary>
    private readonly VramWriteQueue _vramWrites;

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
        _display = dependencies.Display;
        _enemySession = dependencies.EnemySession;
        _bossArtwork = dependencies.BossArtwork;
        _areaArtwork = dependencies.AreaArtwork;
        _room = dependencies.Room;
        _hud = dependencies.Hud;
        _messageBox = dependencies.MessageBox;
        _player = dependencies.Player;
        _roomFx = dependencies.RoomFx;
        _vramWrites = dependencies.VramWrites;
    }
    /// <summary>Installed colors used at construction and on each room-entry palette restore.</summary>
    [field: NonSerialized]
    public GameplayBasePaletteCatalog? InitialPaletteArt { get; set; }
    /// <summary>Whether the next accepted NMI must refresh already initialized HUD characters.</summary>
    [NonSerialized] private HudTileAtlas? pendingHudTiles;
    /// <summary>Gets or binds the host-owned presentation catalog used by maps, HUD, messages, room effects, Samus/enemy colors, and named VRAM assets.</summary>
    /// <remarks>Binding nonnull artwork schedules an initialized HUD refresh and rebinds compiled bus-source identities; assigning null removes presentation owners without rewriting already published VRAM.</remarks>
    [field: NonSerialized]     public IGameplayPresentationContent? MapPresentation
    {
        get;
        set
        {
            field = value;
            _hud.BindPresentation(value?.GameplayHud, _player.Samus);
            _messageBox.BindPresentation(value?.GameplayMessageTitles, value?.GameplayMessagePanels,
                value?.GameplayMessageNotices);
            _roomFx.AnimatedTileArtwork = value?.RoomFxAnimatedTiles;
            _roomFx.Layer3Tilemaps = value?.RoomFxLayer3Tilemaps;
            _roomFx.PaletteBlendColors = value?.RoomFxPaletteBlends;
            BindSamusPalettePresentation();
            _bossArtwork.EscapeTimerArtwork = value?.EscapeTimerTiles;
            _bossArtwork.EscapeTypewriterPresentation = value?.EscapeTypewriter;
            _bossArtwork.HudTileArtwork = value?.HudTiles;
            _bossArtwork.MotherBrainHealthColors = value?.MotherBrainHealthPalette;
            _bossArtwork.MotherBrainRainbowColors = value?.MotherBrainRainbowPalette;
            _bossArtwork.MotherBrainRoomColors = value?.MotherBrainRoomColors;
            _bossArtwork.CeresRidleyColors = value?.CeresRidleyColors;
            _bossArtwork.CeresRidleyFloorColors = value?.CeresRidleyFloorColors;
            _room.Scope?.Enemies.RebindEscapeTypewriters(value?.EscapeTypewriter);
            // A HUD whose upload is still queued shows nothing yet; that upload resolves to the new artwork itself.
            pendingHudTiles = value is not null && _hud.IsInitialized && !_vramWrites.Queues(VramAssetId.StandardHudTiles)
                ? value.HudTiles : null;
            if (value is not null)
                RebindLegacyArtworkTransfers();
        }
    }

    /// <summary>
    /// Points pending transfers a restored legacy queue still sources from the cartridge bus at the
    /// installed Kraid BG3 and escape-timer artwork, matched by their native source and size.
    /// </summary>
    [SuperMetroid.Core.Hardware.PpuLayoutBoundary("codec: legacy queued VRAM DMA records rebound from native bus sources to installed artwork")]
    private void RebindLegacyArtworkTransfers()
    {
        for (int quarter = 0; quarter < SuperMetroid.Core.Bosses.Kraid.KraidBackgroundRomData.StandardBg3TransferCount;
             quarter++)
            _vramWrites.RebindBusSource(
                SuperMetroid.Core.Bosses.Kraid.KraidBackgroundRomData.StandardBg3QuarterAddress(quarter),
                SuperMetroid.Core.Bosses.Kraid.KraidBackgroundRomData.StandardBg3TransferBytes,
                SuperMetroid.Core.Bosses.Kraid.KraidBackgroundRomData.StandardBg3AssetForQuarter(quarter));
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
        _player.Samus.Presentation.TileTransfers.BindStandardObjects(StandardObjects);
        _player.Samus.Presentation.TileTransfers.BindSheets(SpriteSheets);
        _player.Samus.ArmCannon.Artwork = SamusBodyArt?.ArmCannon;
    }

    /// <summary>Shows rebound HUD characters at the HUD's current character base from the accepted NMI.</summary>
    /// <param name="backgrounds">The background artwork bindings, which show the HUD characters.</param>
    internal void PublishReboundHudArtwork(BgArtworkBindings backgrounds)
    {
        if (pendingHudTiles is not { } hudTiles) return;
        // Binding a restored session changes content ownership, not the already
        // published frame. Refresh at accepted NMI, before native queued writes.
        // The original upload's clearing half may now lie under live BG2 words.
        // Kraid's room moves the HUD characters to his base while his BG2 tilemap
        // covers the ordinary one. A restored-state art rebind must honor the
        // *current* character base, or the next NMI paints HUD pixels across Kraid.
        hudTiles.ShowCharactersAgain(backgrounds, _display.GameplayHudCharacterBaseWord);
        pendingHudTiles = null;
    }
    /// <summary>Matches a native DMA source and exact length to the currently installed immutable visual data.</summary>
    internal bool TryResolveInstalledArtwork(int sourceAddress, int byteCount,
        out ReadOnlyMemory<byte> data)
    {
        // Old pending native records and new typed records share the selected
        // artwork. Do not send these immutable sources to the RAM-only DMA path.
        if (MapPresentation?.EscapeTimerTiles.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (BeamArtwork?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (GrappleArtwork?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (SamusBodyArt?.DeathTiles.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (SamusBodyArt?.ArmCannon.TryResolveTile(sourceAddress, byteCount, out data) == true)
            return true;
        if (_enemySession.TileArtwork?.GunshipLiftoff?.TryResolve(
                sourceAddress, byteCount, out data) == true)
            return true;
        if (_enemySession.TileArtwork?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (_areaArtwork.Artwork?.CeresEscapeTiles?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (_areaArtwork.Artwork?.CeresEscapeOverlayTilemaps?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        // The hand PLM queues its fragment sheet through NMI, unlike the enemy
        // instruction uploads that apply directly. Both use current installed art.
        if (_bossArtwork.Artwork?.TorizoInstructionVram?.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (MapPresentation?.RoomFxAnimatedTiles.TryResolve(sourceAddress, byteCount, out data) == true)
            return true;
        if (RoomSkyTilemapArt is not null)
            return RoomSkyTilemapArt.TryResolve(sourceAddress, byteCount, out data);
        data = default;
        return false;
    }

    /// <summary>Complete installed standard OBJ sheet, which the standard object page is composed from.</summary>
    /// <remarks>Host content is rebound after restoring a graph; saved state must not freeze an old user override into
    /// the simulation. Only composition emission uses this.</remarks>
    [field: NonSerialized]
    public RoomCharacterAtlas? StandardObjectArt
    {
        get;
        set
        {
            field = value;
            StandardObjects = value is null ? null
                : ObjCharacterPage.Create(new SpriteArtworkId(SpriteArtworkKind.StandardObjects), value.Transfer.Span);
            _player.Samus?.Presentation.TileTransfers.BindStandardObjects(StandardObjects);
            BindProjectileObjects();
        }
    }

    /// <summary>
    /// The installed standard OBJ sheet as a 512-character page. Characters past the sheet are blank, and a
    /// further row plus one is blank overhang, so any nine-bit OBJ character a large part names stays inside it.
    /// </summary>
    [field: NonSerialized]
    public SpriteArtwork? StandardObjects { get; private set; }

    /// <summary>
    /// The standard OBJ page with the equipped beam's and both trails' characters, which Samus's projectiles,
    /// bombs, charge flare and trails draw from; null until the standard sheet, beam and trail artwork are all bound.
    /// </summary>
    [field: NonSerialized]
    public ProjectileObjectPages? ProjectileObjects { get; private set; }

    /// <summary>Recomposes <see cref="ProjectileObjects"/> from the currently bound sheets.</summary>
    private void BindProjectileObjects() =>
        ProjectileObjects = StandardObjects is { } page && BeamArtwork is { } beams && TrailArtwork?.Tiles is { } trails
            ? new ProjectileObjectPages(page, beams, trails)
            : null;

    /// <summary>Rebinds authored visual frame choices to both native projectile slot owners.</summary>
    [field: NonSerialized]     public ProjectileFrameBindingCatalog? ProjectileFrameBindings
    {
        get;
        set;
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

    /// <summary>Current projectile-trail characters and visual compositions; the projectile pages bind them.</summary>
    [field: NonSerialized]     public ProjectileTrailCatalog? TrailArtwork
    {
        get;
        set { field = value; BindProjectileObjects(); }
    }

    /// <summary>The installed cartridge bytes Samus's glitch paths read; the projectile system binds them.</summary>
    [field: NonSerialized]
    public SamusOverreadCatalog? SamusOverreads
    {
        get;
        set;
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
            _player.Samus?.Presentation.TileTransfers.BindSheets(value);
        }
    }

    /// <summary>The projectile sheets Samus's projectiles, bombs, charge flare and trails draw from.</summary>
    [field: NonSerialized]
    public SuperMetroid.Core.Samus.Artwork.ProjectileSheets? ProjectileSheets { get; private set; }

    /// <summary>Beam artwork bound since the last accepted NMI, whose palette that NMI publishes.</summary>
    [NonSerialized] private BeamTileCatalog? pendingBeamPalette;

    /// <summary>Current beam characters and palettes; the projectile pages bind the equipped beam's characters.</summary>
    [field: NonSerialized]     public BeamTileCatalog? BeamArtwork
    {
        get;
        set { field = value; pendingBeamPalette = value; BindProjectileObjects(); }
    }

    /// <summary>
    /// The equipped-beam word whose characters <c>Update_Beam_Tiles_and_Palette</c> ($90:AC8D) last loaded. The
    /// projectile pages show this beam's characters until the next update, even when code changes the equipment in
    /// between: the Golden Torizo code grants beams during room initialization, after the entering beam was loaded.
    /// </summary>
    public SuperMetroid.Core.Progression.SamusBeamFlags LoadedBeams { get; private set; }

    /// <summary>
    /// Ports <c>Update_Beam_Tiles_and_Palette</c> at $90:AC8D: latches the beam whose characters the projectile pages
    /// show and loads its palette.
    /// </summary>
    internal void UpdateBeamTilesAndPalette(SamusColours colours, SuperMetroid.Core.Progression.SamusBeamFlags equippedBeams)
    {
        LoadedBeams = equippedBeams;
        SuperMetroid.Core.Samus.Projectiles.SamusProjectileSystem.LoadBeamPalette(colours.Beam, equippedBeams, BeamArtwork?.Palettes);
    }

    /// <summary>
    /// Loads a newly bound beam artwork's palette for the equipped beams at an accepted NMI. The characters need no
    /// publication: the projectile pages bind them.
    /// </summary>
    internal void PublishReboundBeamPalette()
    {
        if (pendingBeamPalette is not { } beamArtwork || _player.Samus is not { } samus) return;
        // Do not reinterpret invalid combination overreads as a legal replacement.
        if (new SuperMetroid.Core.Samus.Projectiles.SamusBeamLoadoutWord(samus.EquippedBeams).Combination is SuperMetroid.Core.Progression.SamusBeamCombination selection &&
            selection.IsRetail &&
            // An in-flight effect owns these colors across frames. Its own completion
            // restores the selected normal palette; host rebind must not erase its phase.
            samus.CrystalFlash.SpecialPaletteKind != SuperMetroid.Core.Samus.Abilities.SamusSpecialPaletteType.CrystalFlash &&
            !samus.Drained.HyperBeamPaletteFx.IsActive)
            beamArtwork.Palettes?.LoadTo(_player.Colours.Beam, selection);
        pendingBeamPalette = null;
    }

    /// <summary>Dispatches a typed VRAM asset to its bound gunship, Grapple, trail, HUD, or beam catalog.</summary>
    internal ReadOnlyMemory<byte> ResolveVramAsset(VramAssetId asset) =>
        asset == VramAssetId.StandardObjectTiles
            ? (StandardObjectArt ?? throw new InvalidOperationException("Standard OBJ artwork is not bound.")).Transfer
            : asset is VramAssetId.GunshipLiftoffFirstTiles or
            VramAssetId.GunshipLiftoffSecondTiles or
            VramAssetId.GunshipLiftoffThirdTiles or
            VramAssetId.GunshipLiftoffFourthTiles or
            VramAssetId.GunshipLiftoffFifthTiles
            ? (_enemySession.TileArtwork?.GunshipLiftoff ??
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

    /// <summary>The pre-coloured block pictures of each tileset (#1392).</summary>
    public RoomBlockSheetCatalog? RoomBlockSheets { get; set; }
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
