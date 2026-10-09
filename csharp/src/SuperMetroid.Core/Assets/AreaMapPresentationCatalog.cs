using System.Buffers;
using System.Security.Cryptography;
using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable map content snapshot. Overrides never alter stock provenance or exploration rules.</summary>
public sealed class AreaMapPresentationCatalog : IVramAssetProvider
{
    /// <summary>Per-area map views used to resolve each area's tiles, palettes, and screen presentation.</summary>
    private readonly IAreaMapView[] areas;
/// <summary>Combines the selected area views, content identity, and compiled shared presentation resources into one immutable runtime catalog.</summary>
/// <param name="areas">Compiled views indexed by retail area identity.</param>
/// <param name="contentIdentity">Hash identity of the stock and selected presentation content.</param>
/// <param name="tiles">Map-screen character tile atlas.</param>
/// <param name="hudTiles">Gameplay HUD character atlas.</param>
/// <param name="highlightCycle">Map highlight palette cycle.</param>
/// <param name="palettes">Static map and pause-screen palettes.</param>
/// <param name="labels">World-map area-label layout.</param>
/// <param name="stations">Map-station label layout.</param>
/// <param name="landmarks">Map landmark layout.</param>
/// <param name="saveMarkers">File-select save-marker layout.</param>
/// <param name="arrows">Map-arrow presentation.</param>
/// <param name="screens">Compiled map-screen tilemap layers.</param>
/// <param name="worldArtwork">World-map foreground and background artwork.</param>
/// <param name="sprites">Map sprite catalog.</param>
/// <param name="pauseTiles">Pause-menu character atlas.</param>
/// <param name="pauseBackdrops">Pause-screen backdrop presentation.</param>
/// <param name="pauseWireframes">Pause-screen suit wireframes.</param>
/// <param name="pauseSelectors">Pause-screen selection indicators.</param>
/// <param name="pauseReserveTanks">Reserve-tank strip presentation.</param>
/// <param name="pauseReserveUi">Reserve labels, digits, and arrows.</param>
/// <param name="pauseEquipmentBase">Pause equipment base tilemap.</param>
/// <param name="pauseEquipmentLabels">Pause equipment labels.</param>
/// <param name="escapeTimer">Escape countdown display layout.</param>
/// <param name="escapeTimerTiles">Escape countdown character tiles.</param>
/// <param name="gameplayHud">Gameplay HUD display presentation.</param>
/// <param name="gameOver">Game-over screen presentation.</param>
/// <param name="gameOptions">Options-menu presentation.</param>
/// <param name="fileSelect">File-select screen presentation.</param>
/// <param name="gameplayMessageTitles">Gameplay message title presentation.</param>
/// <param name="gameplayMessagePanels">Gameplay message panel presentation.</param>
/// <param name="gameplayMessageNotices">Short gameplay notice presentation.</param>
/// <param name="escapeTypewriter">Escape sequence typewriter presentation.</param>
/// <param name="introNarration">Opening narration content and layout.</param>
/// <param name="introFont">Opening narration font atlas.</param>
/// <param name="endingText">Ending text presentation.</param>
/// <param name="endingFont">Ending font atlas.</param>
/// <param name="staffCredits">Staff-credit presentation.</param>
/// <param name="titleGraphics">Title-screen graphics.</param>
/// <param name="titlePalette">Title-screen palette.</param>
/// <param name="titleGradient">Title-screen gradient.</param>
/// <param name="roomPaletteFx">Room palette effects.</param>
/// <param name="motherBrainHealthPalette">Mother Brain health palette.</param>
/// <param name="motherBrainRainbowPalette">Mother Brain rainbow palette.</param>
/// <param name="roomFxAnimatedTiles">Animated room-effect tile atlas.</param>
/// <param name="roomFxLayer3Tilemaps">Room-effect layer-three tilemaps.</param>
/// <param name="roomFxPaletteBlends">Room-effect palette blends.</param>
/// <param name="powerBombFixedColors">Power Bomb fixed colors.</param>
/// <param name="samusVisorColors">Samus visor colors.</param>
/// <param name="samusHurtColors">Samus hurt colors.</param>
/// <param name="samusSuitColors">Samus suit colors.</param>
/// <param name="samusFullBodyCycleColors">Samus full-body cycle colors.</param>
/// <param name="crystalFlashColors">Crystal Flash colors.</param>
/// <param name="samusChargeColors">Samus charge colors.</param>
/// <param name="ceresRidleyColors">Ceres Ridley colors.</param>
/// <param name="ceresRidleyMode7Colors">Ceres Ridley Mode-7 colors.</param>
/// <param name="samusHyperBeamColors">Samus Hyper Beam colors.</param>
private AreaMapPresentationCatalog(IAreaMapView[] areas, string contentIdentity, MapTileAtlas tiles, HudTileAtlas hudTiles, MapPaletteCycle highlightCycle, MapStaticPalettes palettes, WorldMapLabelLayout labels, MapStationLayout stations, MapLandmarkLayout landmarks, MapSaveMarkerLayout saveMarkers, MapArrowPresentation arrows, MapScreenPresentation screens, WorldMapArtwork worldArtwork, MapSpriteCatalog sprites, MapTileAtlas pauseTiles, PauseBackdropPresentation pauseBackdrops, PauseWireframePresentation pauseWireframes, PauseSelectorPresentation pauseSelectors, PauseReserveTankPresentation pauseReserveTanks, PauseReserveUiPresentation pauseReserveUi, PauseEquipmentBasePresentation pauseEquipmentBase, PauseEquipmentLabelPresentation pauseEquipmentLabels, EscapeTimerPresentation escapeTimer, EscapeTimerTileAtlas escapeTimerTiles, GameplayHudPresentation gameplayHud, GameOverPresentation gameOver, GameOptionsPresentation gameOptions, FileSelectPresentation fileSelect, GameplayMessageTitlePresentation gameplayMessageTitles, GameplayMessagePanelPresentation gameplayMessagePanels, GameplayMessageNoticePresentation gameplayMessageNotices, EscapeTypewriterPresentation escapeTypewriter, IntroNarrationPresentation introNarration, IntroFontAtlas introFont, EndingTextPresentation endingText, EndingFontAtlas endingFont, CreditsPresentation staffCredits, TitleGraphicsPresentation titleGraphics, TitlePalettePresentation titlePalette, TitleGradientPresentation titleGradient, RoomPaletteFxPresentation roomPaletteFx, MotherBrainHealthPalettePresentation motherBrainHealthPalette, MotherBrainRainbowPalettePresentation motherBrainRainbowPalette, RoomFxAnimatedTileAtlas roomFxAnimatedTiles, RoomFxLayer3TilemapCatalog roomFxLayer3Tilemaps, RoomFxPaletteBlendCatalog roomFxPaletteBlends, PowerBombFixedColorCatalog powerBombFixedColors, SamusVisorColorCatalog samusVisorColors, SamusHurtColorCatalog samusHurtColors, SamusSuitColorCatalog samusSuitColors, SamusFullBodyCycleColorCatalog samusFullBodyCycleColors, CrystalFlashColorCatalog crystalFlashColors, SamusChargeColorCatalog samusChargeColors, CeresRidleyColorCatalog ceresRidleyColors, CeresRidleyMode7ColorCatalog ceresRidleyMode7Colors, SamusHyperBeamColorCatalog samusHyperBeamColors)
    {
        this.areas = areas;
        ContentIdentity = contentIdentity;
        Tiles = tiles;
        HudTiles = hudTiles;
        HighlightCycle = highlightCycle;
        Palettes = palettes;
        Labels = labels;
        Stations = stations;
        Landmarks = landmarks;
        SaveMarkers = saveMarkers;
        Arrows = arrows;
        Screens = screens;
        WorldArtwork = worldArtwork;
        Sprites = sprites;
        PauseTiles = pauseTiles;
        PauseBackdrops = pauseBackdrops;
        PauseWireframes = pauseWireframes;
        PauseSelectors = pauseSelectors;
        PauseReserveTanks = pauseReserveTanks;
        PauseReserveUi = pauseReserveUi;
        PauseEquipmentBase = pauseEquipmentBase;
        PauseEquipmentLabels = pauseEquipmentLabels;
        EscapeTimer = escapeTimer;
        EscapeTimerTiles = escapeTimerTiles;
        GameplayHud = gameplayHud;
        GameOver = gameOver;
        GameOptions = gameOptions;
        FileSelect = fileSelect;
        GameplayMessageTitles = gameplayMessageTitles;
        GameplayMessagePanels = gameplayMessagePanels;
        GameplayMessageNotices = gameplayMessageNotices;
        EscapeTypewriter = escapeTypewriter;
        IntroNarration = introNarration;
        IntroFont = introFont;
        EndingText = endingText;
        EndingFont = endingFont;
        StaffCredits = staffCredits;
        TitleGraphics = titleGraphics;
        TitlePalette = titlePalette;
        TitleGradient = titleGradient;
        RoomPaletteFx = roomPaletteFx;
        MotherBrainHealthPalette = motherBrainHealthPalette;
        MotherBrainRainbowPalette = motherBrainRainbowPalette;
        RoomFxAnimatedTiles = roomFxAnimatedTiles;
        RoomFxLayer3Tilemaps = roomFxLayer3Tilemaps;
        RoomFxPaletteBlends = roomFxPaletteBlends;
        PowerBombFixedColors = powerBombFixedColors;
        SamusVisorColors = samusVisorColors;
        SamusHurtColors = samusHurtColors;
        SamusSuitColors = samusSuitColors;
        SamusFullBodyCycleColors = samusFullBodyCycleColors;
        CrystalFlashColors = crystalFlashColors;
        SamusChargeColors = samusChargeColors;
        CeresRidleyColors = ceresRidleyColors;
        CeresRidleyMode7Colors = ceresRidleyMode7Colors;
        SamusHyperBeamColors = samusHyperBeamColors;
    }

    /// <summary>Uppercase SHA-256 identity of length-framed stock map rules, station masks, and selected presentation resources in fixed order; override edits affect identity without changing stock provenance.</summary>
    public string ContentIdentity { get; }
    /// <summary>Selected map-screen character atlas; pause-menu artwork is supplied separately by <see cref="PauseTiles"/>.</summary>
    public MapTileAtlas Tiles { get; }
    /// <summary>Standard gameplay HUD character transfers, including the four BG3 quarters restored after Kraid.</summary>
    public HudTileAtlas HudTiles { get; }
    /// <summary>Selected highlight color frames used by map presentation; animation progression remains engine-owned.</summary>
    public MapPaletteCycle HighlightCycle { get; }
    /// <summary>Complete static palettes for pause, file-select, and each world-map area, separate from the highlight cycle.</summary>
    public MapStaticPalettes Palettes { get; }
    /// <summary>World-map area-name drawing anchors, independent of area selection and map exploration.</summary>
    public WorldMapLabelLayout Labels { get; }
    /// <summary>Map-station label drawing positions; moving these does not change compiled discovery cells or station reveal masks.</summary>
    public MapStationLayout Stations { get; }
    /// <summary>Boss, elevator, and gunship drawing anchors, independent of progression and destination rules.</summary>
    public MapLandmarkLayout Landmarks { get; }
    /// <summary>File-select save-marker visual coordinate overrides, independent of native load targets and scrolling.</summary>
    public MapSaveMarkerLayout SaveMarkers { get; }
    /// <summary>Map-arrow artwork and presentation frames for scrolling indicators.</summary>
    public MapArrowPresentation Arrows { get; }
    /// <summary>Named compiled map-screen tilemap layers, separate from the character atlas and per-area map cells.</summary>
    public MapScreenPresentation Screens { get; }
    /// <summary>Selected foreground and background PNG artwork for the world-map screen.</summary>
    public WorldMapArtwork WorldArtwork { get; }
    /// <summary>Selected map sprite shapes and indexed PNG artwork used to draw map icons.</summary>
    public MapSpriteCatalog Sprites { get; }
    /// <summary>Selected pause-menu character atlas, separate from map-screen characters in <see cref="Tiles"/>.</summary>
    public MapTileAtlas PauseTiles { get; }
    /// <summary>Pause-screen backdrop tilemap presentation, independent of equipment and map state.</summary>
    public PauseBackdropPresentation PauseBackdrops { get; }
    /// <summary>Samus suit wireframe artwork for the pause equipment screen.</summary>
    public PauseWireframePresentation PauseWireframes { get; }
    /// <summary>Pause selection-indicator artwork and presentation parameters; inventory and selection logic remain engine-owned.</summary>
    public PauseSelectorPresentation PauseSelectors { get; }
    /// <summary>Reserve-strip artwork and drawing origins, independent of reserve energy and fill selection.</summary>
    public PauseReserveTankPresentation PauseReserveTanks { get; }
    /// <summary>Reserve labels, digits, and arrow appearances, separate from the tank strip and compiled reserve-mode behavior.</summary>
    public PauseReserveUiPresentation PauseReserveUi { get; }
    /// <summary>Equipment-page base tilemap artwork, before live inventory and reserve patches are applied.</summary>
    public PauseEquipmentBasePresentation PauseEquipmentBase { get; }
    /// <summary>Equipment-page label artwork and placement, separate from the base tilemap and live inventory decisions.</summary>
    public PauseEquipmentLabelPresentation PauseEquipmentLabels { get; }
    /// <summary>Escape countdown visual layout and digit presentation, separate from the character bytes in <see cref="EscapeTimerTiles"/>.</summary>
    public EscapeTimerPresentation EscapeTimer { get; }
    /// <summary>Selected character bytes for the two escape-timer VRAM transfers.</summary>
    public EscapeTimerTileAtlas EscapeTimerTiles { get; }
    /// <summary>Gameplay HUD tilemap and display presentation, separate from the standard HUD character atlas.</summary>
    public GameplayHudPresentation GameplayHud { get; }
    /// <summary>Selected game-over screen presentation resources.</summary>
    public GameOverPresentation GameOver { get; }
    /// <summary>Selected options-menu artwork and display presentation; option values remain runtime state.</summary>
    public GameOptionsPresentation GameOptions { get; }
    /// <summary>Selected file-select screen presentation, separate from per-area maps and save-marker coordinates.</summary>
    public FileSelectPresentation FileSelect { get; }
    /// <summary>Gameplay-message title text and tile presentation, separate from message panels and short notices.</summary>
    public GameplayMessageTitlePresentation GameplayMessageTitles { get; }
    /// <summary>Gameplay-message panel artwork and layout, separate from title content and short notice presentation.</summary>
    public GameplayMessagePanelPresentation GameplayMessagePanels { get; }
    /// <summary>Short gameplay-notice presentation, separate from full message titles and panels.</summary>
    public GameplayMessageNoticePresentation GameplayMessageNotices { get; }
    /// <summary>Selected escape-sequence typewriter text and visual presentation.</summary>
    public EscapeTypewriterPresentation EscapeTypewriter { get; }
    /// <summary>Opening narration text and presentation, separate from its selected font characters.</summary>
    public IntroNarrationPresentation IntroNarration { get; }
    /// <summary>Selected opening-narration character artwork used with <see cref="IntroNarration"/>.</summary>
    public IntroFontAtlas IntroFont { get; }
    /// <summary>Ending text and display presentation, separate from staff-credit content.</summary>
    public EndingTextPresentation EndingText { get; }
    /// <summary>Selected character artwork for ending text and its font transfers.</summary>
    public EndingFontAtlas EndingFont { get; }
    /// <summary>Selected staff-credit text and presentation, separate from the other ending text.</summary>
    public CreditsPresentation StaffCredits { get; }
    /// <summary>Title Mode 7 characters and tilemap, OBJ characters, and Baby character artwork selected from their extracted files.</summary>
    public TitleGraphicsPresentation TitleGraphics { get; }
    /// <summary>Title-screen palette colors, separate from the title gradient and graphics transfers.</summary>
    public TitlePalettePresentation TitlePalette { get; }
    /// <summary>Title-screen gradient presentation, separate from its static palette and character artwork.</summary>
    public TitleGradientPresentation TitleGradient { get; }
    /// <summary>Room and cinematic palette-animation colors; instruction timing, CGRAM destinations, and side effects remain compiled.</summary>
    public RoomPaletteFxPresentation RoomPaletteFx { get; }
    /// <summary>Mother Brain health-selected palette artwork, separate from rainbow and room-color families.</summary>
    public MotherBrainHealthPalettePresentation MotherBrainHealthPalette { get; }
    /// <summary>Mother Brain rainbow, drain, revival, normal-restoration, and fake-death colors; older overrides inherit newly introduced rows from stock.</summary>
    public MotherBrainRainbowPalettePresentation MotherBrainRainbowPalette { get; }
    /// <summary>Mother Brain arena color presentation, separate from boss health and rainbow palettes; older overrides inherit new color families from verified stock.</summary>
    public MotherBrainRoomColorPresentation MotherBrainRoomColors { get; private set; } = null!;
    /// <summary>Selected animated room-FX character artwork, separate from layer-3 tilemaps and palette blending.</summary>
    public RoomFxAnimatedTileAtlas RoomFxAnimatedTiles { get; }
    /// <summary>Selected layer-3 room-FX tilemaps, separate from animated characters and palette-blend colors.</summary>
    public RoomFxLayer3TilemapCatalog RoomFxLayer3Tilemaps { get; }
    /// <summary>Selected room-FX palette-blend presentation, separate from layer-3 tilemap geometry and animated character bytes.</summary>
    public RoomFxPaletteBlendCatalog RoomFxPaletteBlends { get; }
    /// <summary>Fixed RGB5 colors for Power Bomb, Crystal Flash, and Ceres explosion effects, separate from Samus palette colors.</summary>
    public PowerBombFixedColorCatalog PowerBombFixedColors { get; }
    /// <summary>Visor colors shared by X-ray and room palette cycling, separate from complete suit palettes.</summary>
    public SamusVisorColorCatalog SamusVisorColors { get; }
    /// <summary>Samus hurt-flash and cinematic-restoration color artwork.</summary>
    public SamusHurtColorCatalog SamusHurtColors { get; }
    /// <summary>Base Power, Varia, and Gravity Suit colors, separate from transient full-body and charge cycles.</summary>
    public SamusSuitColorCatalog SamusSuitColors { get; }
    /// <summary>Samus full-body palette-cycle inputs, separate from static suit, visor, and charged-beam colors.</summary>
    public SamusFullBodyCycleColorCatalog SamusFullBodyCycleColors { get; }
    /// <summary>Crystal Flash body and bubble colors; native timing, frame selection, and completion remain engine-owned.</summary>
    public CrystalFlashColorCatalog CrystalFlashColors { get; }
    /// <summary>Charged-beam and pseudo-Screw-Attack palette inputs, separate from base suit colors.</summary>
    public SamusChargeColorCatalog SamusChargeColors { get; }
    /// <summary>Ceres Ridley colors, shared Norfair Ridley health colors, and private Baby drawing colors; Mode 7 zoom shades are supplied separately.</summary>
    public CeresRidleyColorCatalog CeresRidleyColors { get; }
    /// <summary>Ceres Ridley Mode 7 zoom shades, separate from ordinary Ridley colors and compiled movement and rotation.</summary>
    public CeresRidleyMode7ColorCatalog CeresRidleyMode7Colors { get; }
    /// <summary>Samus Hyper Beam palette-cycle colors, separate from charged-beam presentation and other suit cycles.</summary>
    public SamusHyperBeamColorCatalog SamusHyperBeamColors { get; }
    /// <summary>Resolves installed HUD, Kraid HUD-restoration, or escape-timer character bytes for a VRAM transfer.</summary>
    /// <param name="asset">The transfer identity; this provider supports only the HUD and escape-timer identities in its catalog.</param>
    /// <returns>Read-only planar character bytes for the requested transfer.</returns>
    /// <exception cref="InvalidDataException">The requested identity is not supported by this provider.</exception>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => asset switch
    {
        VramAssetId.StandardHudTiles => HudTiles.Transfer,
        VramAssetId.KraidBg3RestoreQuarter0 => HudTiles.KraidRestoreQuarter(0),
        VramAssetId.KraidBg3RestoreQuarter1 => HudTiles.KraidRestoreQuarter(1),
        VramAssetId.KraidBg3RestoreQuarter2 => HudTiles.KraidRestoreQuarter(2),
        VramAssetId.KraidBg3RestoreQuarter3 => HudTiles.KraidRestoreQuarter(3),
        VramAssetId.EscapeTimerFirstTiles or VramAssetId.EscapeTimerSecondTiles => EscapeTimerTiles.Resolve(asset),
        _ => throw new InvalidDataException($"Map catalog cannot resolve VRAM asset {asset}."),
    };
    /// <summary>Returns an area's selected map presentation with verified stock exploration and station-reveal rules.</summary>
    /// <param name="area">One of the seven retail area identities, including Ceres.</param>
    /// <returns>The immutable view compiled when this catalog was loaded.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a retail area identity.</exception>
    public IAreaMapView Get(AreaId area) => areas[AreaIds.ToIndex(area)];

    /// <summary>Reads all areas atomically into a new catalog; an invalid override is never replaced with stock.</summary>
    public static AreaMapPresentationCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        using var stock = VerifiedStockReader.Open(stockDirectory);
        var mapBytes = new Dictionary<AreaId, ArraySegment<byte>>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
            mapBytes.Add(area, stock.Read(AreaMapCatalogFormat.FileName(area)));
        Dictionary<AreaId, HashSet<int>> stationCells = stock.ReadStationCells();
        var areas = new IAreaMapView[AreaIds.RetailCount];
        using var identity = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            using var baselineStream = ReadOnlyStream(mapBytes[area]);
            var baseline = AreaMapPresentationAsset.Load(baselineStream, new AreaMapDecodeRules(area));
            IAreaMapView definition = new AreaMapStockRules(baseline, stationCells[area]);
            // The identity covers baseline rule content too: equal override bytes do
            // not imply equal exploration semantics across different stock installs.
            AppendFramed(mapBytes[area]);
            byte[] stationPlane = new byte[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
            foreach (int cell in stationCells[area]) stationPlane[cell] = 1;
            AppendFramed(stationPlane);
            string? replacement = OverridePath(AreaMapCatalogFormat.FileName(area));
            ArraySegment<byte> selected = replacement is not null ? File.ReadAllBytes(replacement) : mapBytes[area];
            if (replacement is null)
                areas[AreaIds.ToIndex(area)] = baseline.WithRules(definition);
            else
            {
                try
                {
                    using var stream = ReadOnlyStream(selected);
                    areas[AreaIds.ToIndex(area)] = AreaMapPresentationAsset.Load(stream, definition);
                }
                catch (InvalidDataException error)
                {
                    throw new InvalidDataException($"Invalid {area} map presentation ({replacement}): {error.Message}", error);
                }
            }
            // Hash framed contents in fixed area order: identity changes after a valid
            // replacement is reloaded, without rewriting original extraction provenance.
            AppendFramed(selected);
        }
        // Each resource below is framed into the identity in this fixed order.
        MapTileAtlas tiles = Resource(MapTileAtlasFormat.FileName, "map tile atlas", s => MapTileAtlas.Load(s));
        HudTileAtlas hudTiles = Resource(HudTileAtlasFormat.FileName, "HUD tile atlas", s => HudTileAtlas.Load(s));
        MapPaletteCycle cycle = Resource(MapPaletteCycleFormat.FileName, "map highlight cycle", s => MapPaletteCycle.Load(s));
        MapStaticPalettes palettes = Resource(MapStaticPalettesFormat.FileName, "map palettes", s => MapStaticPalettes.Load(s));
        WorldMapLabelLayout labels = Resource(WorldMapLabelFormat.FileName, "world-map labels", s => WorldMapLabelLayout.Load(s));
        MapStationLayout stations = Resource(MapStationLayoutFormat.FileName, "map station labels", s => MapStationLayout.Load(s));
        MapLandmarkLayout landmarks = Resource(MapLandmarkFormat.FileName, "map landmarks", s => MapLandmarkLayout.Load(s));
        MapSaveMarkerLayout saveMarkers = Resource(MapSaveMarkerFormat.FileName, "save markers", s => MapSaveMarkerLayout.Load(s));
        MapArrowPresentation arrows = Resource(MapArrowFormat.FileName, "map arrows", s => MapArrowPresentation.Load(s));
        MapScreenPresentation screens = Resource(MapScreenDefinitions.FileName, "map screens", s => MapScreenPresentation.Load(s));
        WorldMapArtwork artwork = MultiFileResource<WorldMapArtwork>([WorldMapArtworkFormat.ForegroundFile, WorldMapArtworkFormat.BackgroundFile],
            "world-map PNG artwork", (s, _) => WorldMapArtwork.Load(s[0], s[1]));
        MapSpriteCatalog sprites = MultiFileResource<MapSpriteCatalog>([MapSpriteFormat.JsonFile, MapSpriteFormat.PngFile],
            "map sprites", (s, _) => MapSpriteCatalog.Load(s[0], s[1]));
        MapTileAtlas pauseTiles = Resource(PauseTileAtlasFormat.FileName, "pause artwork", s => MapTileAtlas.Load(s));
        PauseBackdropPresentation pauseBackdrops = Resource(PauseBackdropDefinitions.FileName, "pause backdrop", s => PauseBackdropPresentation.Load(s));
        PauseSelectorPresentation pauseSelectors = Resource(PauseSelectorDefinitions.FileName, "pause selectors", s => PauseSelectorPresentation.Load(s));
        PauseWireframePresentation pauseWireframes = Resource(PauseWireframeDefinitions.FileName, "pause wireframes", s => PauseWireframePresentation.Load(s));
        PauseReserveTankPresentation pauseReserveTanks = Resource(PauseReserveTankDefinitions.FileName, "reserve tank presentation", s => PauseReserveTankPresentation.Load(s));
        PauseReserveUiPresentation pauseReserveUi = Resource(PauseReserveUiDefinitions.FileName, "reserve UI presentation", s => PauseReserveUiPresentation.Load(s));
        PauseEquipmentBasePresentation pauseEquipmentBase = Resource(PauseEquipmentBaseDefinitions.FileName, "pause equipment base", s => PauseEquipmentBasePresentation.Load(s));
        PauseEquipmentLabelPresentation pauseEquipmentLabels = Resource(PauseEquipmentLabelDefinitions.FileName, "pause equipment labels", s => PauseEquipmentLabelPresentation.Load(s));
        EscapeTimerPresentation escapeTimer = Resource(EscapeTimerPresentationDefinitions.FileName, "escape timer presentation", s => EscapeTimerPresentation.Load(s));
        EscapeTimerTileAtlas escapeTimerTiles = Resource(EscapeTimerTileAtlasFormat.FileName, "escape timer artwork", s => EscapeTimerTileAtlas.Load(s));
        GameplayHudPresentation gameplayHud = Resource(GameplayHudDefinitions.FileName, "gameplay HUD presentation", s => GameplayHudPresentation.Load(s));
        GameOverPresentation gameOver = Resource(GameOverPresentationDefinitions.FileName, "game-over presentation", s => GameOverPresentation.Load(s));
        GameOptionsPresentation gameOptions = Resource(GameOptionsPresentationDefinitions.FileName, "options-menu presentation", s => GameOptionsPresentation.Load(s));
        FileSelectPresentation fileSelect = Resource(FileSelectPresentationDefinitions.FileName, "file-select presentation", s => FileSelectPresentation.Load(s));
        GameplayMessageTitlePresentation gameplayMessageTitles = Resource(GameplayMessageTitleDefinitions.FileName, "gameplay-message titles", s => GameplayMessageTitlePresentation.Load(s));
        GameplayMessagePanelPresentation gameplayMessagePanels = Resource(GameplayMessagePanelDefinitions.FileName, "gameplay-message panels", s => GameplayMessagePanelPresentation.Load(s));
        GameplayMessageNoticePresentation gameplayMessageNotices = Resource(GameplayMessageNoticeDefinitions.FileName, "gameplay-message notices", s => GameplayMessageNoticePresentation.Load(s));
        EscapeTypewriterPresentation escapeTypewriter = Resource(EscapeTypewriterDefinitions.FileName, "escape typewriter", s => EscapeTypewriterPresentation.Load(s));
        IntroNarrationPresentation introNarration = Resource(IntroNarrationDefinitions.FileName, "intro narration", s => IntroNarrationPresentation.Load(s));
        IntroFontAtlas introFont = Resource(IntroFontAtlasFormat.FileName, "intro font", s => IntroFontAtlas.Load(s));
        EndingTextPresentation endingText = Resource(EndingTextDefinitions.FileName, "ending text", s => EndingTextPresentation.Load(s));
        EndingFontAtlas endingFont = Resource(EndingFontAtlasFormat.FileName, "ending font", s => EndingFontAtlas.Load(s));
        CreditsPresentation staffCredits = Resource(CreditsPresentationDefinitions.FileName, "staff credits", s => CreditsPresentation.Load(s));
        TitleGraphicsPresentation titleGraphics = MultiFileResource<TitleGraphicsPresentation>(
            [TitleGraphicsFormat.Mode7TilesFile, TitleGraphicsFormat.Mode7MapFile, TitleGraphicsFormat.ObjectTilesFile, TitleGraphicsFormat.BabyTilesFile],
            "title graphics", (s, _) => TitleGraphicsPresentation.Load(s[0], s[1], s[2], s[3]));
        TitlePalettePresentation titlePalette = Resource(TitlePaletteFormat.FileName, "title palette", s => TitlePalettePresentation.Load(s));
        TitleGradientPresentation titleGradient = Resource(TitleGradientFormat.FileName, "title gradient", s => TitleGradientPresentation.Load(s));
        // Version-seventeen overrides predate the Samus-in-heat rows. Preserve
        // their edits while inheriting only those new rows from current stock.
        RoomPaletteFxPresentation roomPaletteFx = MigratingResource<RoomPaletteFxPresentation>(RoomPaletteFxPresentationFormat.FileName,
            "room palette effects", (s, current) => RoomPaletteFxPresentation.Load(s, current), frameStock: true);
        MotherBrainHealthPalettePresentation motherBrainHealthPalette = Resource(MotherBrainHealthPaletteFormat.FileName, "Mother Brain health palette", s => MotherBrainHealthPalettePresentation.Load(s));
        // A version-two user override predates the fake-death rows. Preserve its edits
        // and fill only that new color family from verified, current stock content.
        MotherBrainRainbowPalettePresentation motherBrainRainbowPalette = MigratingResource<MotherBrainRainbowPalettePresentation>(MotherBrainRainbowPaletteFormat.FileName,
            "Mother Brain rainbow palette", (s, current) => MotherBrainRainbowPalettePresentation.Load(s, current), frameStock: true);
        MotherBrainRoomColorPresentation motherBrainRoomColors = MigratingResource<MotherBrainRoomColorPresentation>(MotherBrainRoomColorFormat.FileName,
            "Mother Brain room colors", (s, current) => MotherBrainRoomColorPresentation.Load(s, current), frameStock: true);
        RoomFxAnimatedTileAtlas roomFxAnimatedTiles = MigratingResource<RoomFxAnimatedTileAtlas>(RoomFxAnimatedTileAtlasFormat.FileName,
            "room-FX presentation", (s, current) => RoomFxAnimatedTileAtlas.Load(s, current), frameStock: false);
        RoomFxLayer3TilemapCatalog roomFxLayer3Tilemaps = Resource(RoomFxLayer3TilemapFormat.FileName, "room-FX presentation", s => RoomFxLayer3TilemapCatalog.Load(s));
        RoomFxPaletteBlendCatalog roomFxPaletteBlends = Resource(RoomFxPaletteBlendDefinitions.FileName, "room-FX presentation", s => RoomFxPaletteBlendCatalog.Load(s));
        PowerBombFixedColorCatalog powerBombFixedColors = Resource(PowerBombFixedColorFormat.FileName, "Power Bomb fixed colors", s => PowerBombFixedColorCatalog.Load(s));
        SamusVisorColorCatalog samusVisorColors = Resource(SamusVisorColorFormat.FileName, "Samus visor colors", s => SamusVisorColorCatalog.Load(s));
        SamusHurtColorCatalog samusHurtColors = Resource(SamusHurtColorFormat.FileName, "Samus hurt colors", s => SamusHurtColorCatalog.Load(s));
        SamusSuitColorCatalog samusSuitColors = Resource(SamusSuitColorFormat.FileName, "Samus suit colors", s => SamusSuitColorCatalog.Load(s));
        SamusFullBodyCycleColorCatalog samusFullBodyCycleColors = Resource(SamusFullBodyCycleColorFormat.FileName, "Samus full-body cycle colors", s => SamusFullBodyCycleColorCatalog.Load(s));
        CrystalFlashColorCatalog crystalFlashColors = Resource(CrystalFlashColorFormat.FileName, "Crystal Flash colors", s => CrystalFlashColorCatalog.Load(s));
        SamusChargeColorCatalog samusChargeColors = Resource(SamusChargeColorFormat.FileName, "Samus charge colors", s => SamusChargeColorCatalog.Load(s));
        CeresRidleyColorCatalog ceresRidleyColors = MigratingResource<CeresRidleyColorCatalog>(CeresRidleyColorFormat.FileName,
            "Ceres Ridley colors", (s, current) => CeresRidleyColorCatalog.Load(s, current), frameStock: false);
        CeresRidleyMode7ColorCatalog ceresRidleyMode7Colors = Resource(CeresRidleyMode7ColorFormat.FileName, "Ceres Ridley Mode-7 colors", s => CeresRidleyMode7ColorCatalog.Load(s));
        SamusHyperBeamColorCatalog samusHyperBeamColors = Resource(SamusHyperBeamColorFormat.FileName, "Samus Hyper Beam colors", s => SamusHyperBeamColorCatalog.Load(s));
        stock.RequireAllManifestFilesRead();
        var catalog = new AreaMapPresentationCatalog(areas, Convert.ToHexString(identity.GetHashAndReset()), tiles, hudTiles, cycle, palettes, labels, stations, landmarks, saveMarkers, arrows, screens, artwork, sprites, pauseTiles, pauseBackdrops, pauseWireframes, pauseSelectors, pauseReserveTanks, pauseReserveUi, pauseEquipmentBase, pauseEquipmentLabels, escapeTimer, escapeTimerTiles, gameplayHud, gameOver, gameOptions, fileSelect, gameplayMessageTitles, gameplayMessagePanels, gameplayMessageNotices, escapeTypewriter, introNarration, introFont, endingText, endingFont, staffCredits, titleGraphics, titlePalette, titleGradient, roomPaletteFx, motherBrainHealthPalette, motherBrainRainbowPalette, roomFxAnimatedTiles, roomFxLayer3Tilemaps, roomFxPaletteBlends, powerBombFixedColors, samusVisorColors, samusHurtColors, samusSuitColors, samusFullBodyCycleColors, crystalFlashColors, samusChargeColors, ceresRidleyColors, ceresRidleyMode7Colors, samusHyperBeamColors);
        catalog.MotherBrainRoomColors = motherBrainRoomColors;
        return catalog;

        T Resource<T>(string file, string description, Func<Stream, T> compile) where T : class =>
            MultiFileResource<T>([file], description, (streams, _) => compile(streams[0]));

        T MigratingResource<T>(string file, string description, Func<Stream, T?, T> compile, bool frameStock) where T : class =>
            MultiFileResource<T>([file], description, (streams, current) => compile(streams[0], current), frameStock);

        // Stock is hash-checked and compiled exactly once. An unedited resource reuses that
        // compiled stock value; an edited one compiles its override against current stock.
        T MultiFileResource<T>(string[] files, string description, Func<Stream[], T?, T> compile, bool frameStock = false) where T : class
        {
            ArraySegment<byte>[] stockBytes = files.Select(stock.Read).ToArray();
            T stockValue = Compile(files.Select(file => Path.Combine(stockDirectory, file)).ToArray(), stockBytes, null);
            if (frameStock)
                foreach (ArraySegment<byte> bytes in stockBytes) AppendFramed(bytes);
            string?[] edited = files.Select(OverridePath).ToArray();
            var selected = new ArraySegment<byte>[files.Length];
            for (int index = 0; index < files.Length; index++)
            {
                selected[index] = edited[index] is { } path ? File.ReadAllBytes(path) : stockBytes[index];
                AppendFramed(selected[index]);
            }
            if (edited.All(path => path is null)) return stockValue;
            return Compile(files.Select((file, index) => edited[index] ?? Path.Combine(stockDirectory, file)).ToArray(), selected, stockValue);

            T Compile(string[] paths, ArraySegment<byte>[] contents, T? current)
            {
                try
                {
                    return compile(contents.Select(bytes => (Stream)ReadOnlyStream(bytes)).ToArray(), current);
                }
                catch (InvalidDataException error)
                {
                    // Attribute both required stock and optional edits to the actual files,
                    // retaining the codec error instead of reporting only a directory.
                    throw new InvalidDataException($"Invalid {description} '{string.Join("', '", paths.Select(Path.GetFullPath))}': {error.Message}", error);
                }
            }
        }

        string? OverridePath(string name)
        {
            string? path = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            return path is not null && File.Exists(path) ? path : null;
        }

        void AppendFramed(ReadOnlySpan<byte> bytes)
        {
            Span<byte> length = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(length, bytes.Length);
            identity.AppendData(length);
            identity.AppendData(bytes);
        }
    }

    /// <summary>
    /// A read-only view whose buffer the JSON reader may parse in place. Compilers copy what
    /// they keep, so pooled stock bytes are never referenced after <see cref="Load"/> returns.
    /// </summary>
    private static MemoryStream ReadOnlyStream(ArraySegment<byte> bytes) =>
        new(bytes.Array!, bytes.Offset, bytes.Count, writable: false, publiclyVisible: true);

    /// <summary>Installer integrity check; never repairs files or touches the override directory.</summary>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Reads stock files that must match the extraction manifest's SHA-256 values.</summary>
    private sealed class VerifiedStockReader : IDisposable
    {
        /// <summary>Directory containing the stock files whose hashes are recorded in the manifest.</summary>
        private readonly string directory;
        /// <summary>Expected SHA-256 values and resource provenance loaded from the stock manifest.</summary>
        private readonly AreaMapCatalogManifest manifest;
        /// <summary>Names of manifest resources that have been read and successfully verified.</summary>
        private readonly HashSet<string> read = new(StringComparer.Ordinal);
        // Stock bytes live only for one catalog load; every buffer returns to the pool on dispose.
        /// <summary>Rented file buffers retained until catalog loading finishes, then returned to the shared pool.</summary>
        private readonly List<byte[]> rented = [];

        /// <summary>Creates a reader that verifies stock resources against a previously validated manifest.</summary>
        /// <param name="directory">Directory containing the manifest and stock resources.</param>
        /// <param name="manifest">Validated mapping of stock filenames to expected digests.</param>
        private VerifiedStockReader(string directory, AreaMapCatalogManifest manifest)
        {
            this.directory = directory;
            this.manifest = manifest;
        }

        /// <summary>Loads and validates the stock manifest before opening the verified resource reader.</summary>
        /// <param name="directory">Directory containing the stock manifest and resource files.</param>
        /// <returns>A reader that checks each resource against its manifest digest.</returns>
        /// <exception cref="InvalidDataException">The manifest is missing, malformed, or does not describe the supported resource set.</exception>
        public static VerifiedStockReader Open(string directory)
        {
            AreaMapCatalogManifest manifest;
            try
            {
                manifest = JsonAssetDocument.Read<AreaMapCatalogManifest>(
                    File.ReadAllBytes(Path.Combine(directory, AreaMapCatalogFormat.ManifestFile)), MapPresentationFormat.JsonOptions)
                    ?? throw new InvalidDataException("Map catalog manifest is null.");
            }
            catch (JsonException error) { throw new InvalidDataException($"Invalid map catalog manifest in {directory}.", error); }
            if (manifest.Version != AreaMapCatalogFormat.Version || manifest.Sha256 is null || manifest.Sha256.Count != AreaIds.RetailCount + AreaMapCatalogFormat.SharedResourceCount)
                throw new InvalidDataException($"Map catalog manifest must contain the supported version, seven maps and all {AreaMapCatalogFormat.SharedResourceCount} shared presentation resource hashes.");
            return new VerifiedStockReader(directory, manifest);
        }

        /// <summary>Reads a manifest-listed stock file into a pooled buffer and verifies its SHA-256 digest.</summary>
        /// <param name="file">Manifest key and filename of the stock resource to read.</param>
        /// <returns>A segment containing the verified file bytes; the buffer remains owned until this reader is disposed.</returns>
        /// <exception cref="InvalidDataException">The file is absent from the manifest or its digest does not match.</exception>
        public ArraySegment<byte> Read(string file)
        {
            if (!manifest.Sha256.TryGetValue(file, out string? expected))
                throw new InvalidDataException($"Map catalog manifest is missing {file}.");
            using var input = File.OpenRead(Path.Combine(directory, file));
            int length = checked((int)input.Length);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
            rented.Add(buffer);
            input.ReadExactly(buffer.AsSpan(0, length));
            var bytes = new ArraySegment<byte>(buffer, 0, length);
            Span<byte> actual = stackalloc byte[SHA256.HashSizeInBytes];
            SHA256.HashData(bytes, actual);
            if (!string.Equals(expected, Convert.ToHexString(actual), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock map '{Path.GetFullPath(Path.Combine(directory, file))}' failed its SHA-256 check. Put edits in the overrides directory, not stock content.");
            read.Add(file);
            return bytes;
        }

        /// <summary>Returns every stock-file buffer rented by this reader to the shared array pool.</summary>
        public void Dispose()
        {
            foreach (byte[] buffer in rented) ArrayPool<byte>.Shared.Return(buffer);
            rented.Clear();
        }

        /// <summary>Loads the station reveal mask and validates one unique set of in-bounds cells for each retail area.</summary>
        /// <returns>Area-indexed sets of logical map cell indices revealed by stations.</returns>
        /// <exception cref="InvalidDataException">The mask is malformed, omits an area, or contains duplicate or out-of-range cells.</exception>
        public Dictionary<AreaId, HashSet<int>> ReadStationCells()
        {
            Dictionary<string, int[]> masks;
            try
            {
                masks = JsonAssetDocument.Read<Dictionary<string, int[]>>(ReadOnlyStream(Read(AreaMapCatalogFormat.StationRevealFile)), MapPresentationFormat.JsonOptions)
                    ?? throw new InvalidDataException("Station reveal content is null.");
            }
            catch (JsonException error) { throw new InvalidDataException("Invalid station reveal content.", error); }
            if (masks.Count != AreaIds.RetailCount) throw new InvalidDataException("Station reveal content requires all seven areas.");
            var stationCells = new Dictionary<AreaId, HashSet<int>>();
            foreach (AreaId area in Enum.GetValues<AreaId>())
            {
                if (!masks.TryGetValue(area.ToString(), out int[]? cells) || cells is null ||
                    cells.Any(cell => (uint)cell >= AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles) ||
                    cells.Distinct().Count() != cells.Length)
                    throw new InvalidDataException($"Invalid or duplicate station reveal cells for {area}.");
                stationCells.Add(area, cells.ToHashSet());
            }
            return stationCells;
        }

        /// <summary>Every manifest-bound stock file is verified by the load that uses it.</summary>
        public void RequireAllManifestFilesRead()
        {
            string[] unread = manifest.Sha256.Keys.Where(file => !read.Contains(file)).ToArray();
            if (unread.Length > 0)
                throw new InvalidDataException($"Map catalog manifest lists stock files the catalog never verifies: {string.Join(", ", unread)}.");
        }
    }
}

/// <summary>Stock content provenance, separate from a catalog's selected replacement identity.</summary>
public sealed record AreaMapCatalogManifest
{
    /// <summary>Catalog schema version that must equal <see cref="AreaMapCatalogFormat.Version"/> when stock content is loaded.</summary>
    public required int Version { get; init; }
    /// <summary>SHA-256 digest of the cartridge used to extract the stock content, independent of selected user overrides.</summary>
    public required string SourceCartridgeSha256 { get; init; }
    /// <summary>Extracted filename-to-SHA-256 mapping; every listed stock file must be read and verified during catalog loading.</summary>
    public required Dictionary<string, string> Sha256 { get; init; }
}

/// <summary>On-disk names and manifest bounds for seven area maps and their shared presentation resources.</summary>
public static class AreaMapCatalogFormat
{
    /// <summary>Supported extracted map-catalog schema version; other manifest versions are rejected.</summary>
    public const int Version = 78;
    /// <summary>Manifest-bound non-area artwork files, including Ceres Mode-7 colors.</summary>
    public const int SharedResourceCount = 61;
    /// <summary>Bundled authored reveal mask: logical row-major cell indexes, not SRAM offsets or editable engine code.</summary>
    public const string StationRevealFile = "station-reveal.json";
    /// <summary>Stock manifest filename containing extraction provenance and the SHA-256 digest of every catalog resource.</summary>
    public const string ManifestFile = "manifest.json";
    /// <summary>Builds the extracted JSON filename from a validated retail area identity using its lowercase enum name.</summary>
    /// <param name="area">One of the seven retail area identities.</param>
    /// <returns>The area's lowercase name followed by <c>.json</c>, without a directory.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a retail area identity.</exception>
    public static string FileName(AreaId area)
    {
        _ = AreaIds.ToIndex(area);
        return area.ToString().ToLowerInvariant() + ".json";
    }
}
