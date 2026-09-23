using System.Security.Cryptography;
using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable map content snapshot. Overrides never alter stock provenance or exploration rules.</summary>
public sealed class AreaMapPresentationCatalog : IVramAssetProvider
{
    private readonly IAreaMapView[] areas;
    private AreaMapPresentationCatalog(IAreaMapView[] areas, string contentIdentity, MapTileAtlas tiles, HudTileAtlas hudTiles, MapPaletteCycle highlightCycle, MapStaticPalettes palettes, WorldMapLabelLayout labels, MapStationLayout stations, MapLandmarkLayout landmarks, MapSaveMarkerLayout saveMarkers, MapArrowPresentation arrows, MapScreenPresentation screens, WorldMapArtwork worldArtwork, MapSpriteCatalog sprites, MapTileAtlas pauseTiles, PauseBackdropPresentation pauseBackdrops, PauseWireframePresentation pauseWireframes, PauseSelectorPresentation pauseSelectors, PauseReserveTankPresentation pauseReserveTanks, PauseReserveUiPresentation pauseReserveUi, PauseEquipmentBasePresentation pauseEquipmentBase, PauseEquipmentLabelPresentation pauseEquipmentLabels, EscapeTimerPresentation escapeTimer, EscapeTimerTileAtlas escapeTimerTiles, GameplayHudPresentation gameplayHud, GameOverPresentation gameOver, GameOptionsPresentation gameOptions, FileSelectPresentation fileSelect, GameplayMessageTitlePresentation gameplayMessageTitles, GameplayMessagePanelPresentation gameplayMessagePanels, GameplayMessageNoticePresentation gameplayMessageNotices, EscapeTypewriterPresentation escapeTypewriter, IntroNarrationPresentation introNarration, IntroFontAtlas introFont, EndingTextPresentation endingText, EndingFontAtlas endingFont, CreditsPresentation staffCredits, TitleGraphicsPresentation titleGraphics, TitlePalettePresentation titlePalette, TitleGradientPresentation titleGradient, RoomPaletteFxPresentation roomPaletteFx, MotherBrainHealthPalettePresentation motherBrainHealthPalette, MotherBrainRainbowPalettePresentation motherBrainRainbowPalette, RoomFxAnimatedTileAtlas roomFxAnimatedTiles, RoomFxLayer3TilemapCatalog roomFxLayer3Tilemaps, RoomFxPaletteBlendCatalog roomFxPaletteBlends)
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
    }

    public string ContentIdentity { get; }
    public MapTileAtlas Tiles { get; }
    public HudTileAtlas HudTiles { get; }
    public MapPaletteCycle HighlightCycle { get; }
    public MapStaticPalettes Palettes { get; }
    public WorldMapLabelLayout Labels { get; }
    public MapStationLayout Stations { get; }
    public MapLandmarkLayout Landmarks { get; }
    public MapSaveMarkerLayout SaveMarkers { get; }
    public MapArrowPresentation Arrows { get; }
    public MapScreenPresentation Screens { get; }
    public WorldMapArtwork WorldArtwork { get; }
    public MapSpriteCatalog Sprites { get; }
    public MapTileAtlas PauseTiles { get; }
    public PauseBackdropPresentation PauseBackdrops { get; }
    public PauseWireframePresentation PauseWireframes { get; }
    public PauseSelectorPresentation PauseSelectors { get; }
    public PauseReserveTankPresentation PauseReserveTanks { get; }
    public PauseReserveUiPresentation PauseReserveUi { get; }
    public PauseEquipmentBasePresentation PauseEquipmentBase { get; }
    public PauseEquipmentLabelPresentation PauseEquipmentLabels { get; }
    public EscapeTimerPresentation EscapeTimer { get; }
    public EscapeTimerTileAtlas EscapeTimerTiles { get; }
    public GameplayHudPresentation GameplayHud { get; }
    public GameOverPresentation GameOver { get; }
    public GameOptionsPresentation GameOptions { get; }
    public FileSelectPresentation FileSelect { get; }
    public GameplayMessageTitlePresentation GameplayMessageTitles { get; }
    public GameplayMessagePanelPresentation GameplayMessagePanels { get; }
    public GameplayMessageNoticePresentation GameplayMessageNotices { get; }
    public EscapeTypewriterPresentation EscapeTypewriter { get; }
    public IntroNarrationPresentation IntroNarration { get; }
    public IntroFontAtlas IntroFont { get; }
    public EndingTextPresentation EndingText { get; }
    public EndingFontAtlas EndingFont { get; }
    public CreditsPresentation StaffCredits { get; }
    public TitleGraphicsPresentation TitleGraphics { get; }
    public TitlePalettePresentation TitlePalette { get; }
    public TitleGradientPresentation TitleGradient { get; }
    public RoomPaletteFxPresentation RoomPaletteFx { get; }
    public MotherBrainHealthPalettePresentation MotherBrainHealthPalette { get; }
    public MotherBrainRainbowPalettePresentation MotherBrainRainbowPalette { get; }
    public RoomFxAnimatedTileAtlas RoomFxAnimatedTiles { get; }
    public RoomFxLayer3TilemapCatalog RoomFxLayer3Tilemaps { get; }
    public RoomFxPaletteBlendCatalog RoomFxPaletteBlends { get; }
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset) => asset switch
    {
        VramAssetId.StandardHudTiles => HudTiles.Transfer,
        VramAssetId.EscapeTimerFirstTiles or VramAssetId.EscapeTimerSecondTiles => EscapeTimerTiles.Resolve(asset),
        _ => throw new InvalidDataException($"Map catalog cannot resolve VRAM asset {asset}."),
    };
    public IAreaMapView Get(AreaId area) => areas[AreaIds.ToIndex(area)];

    /// <summary>Reads all areas atomically into a new catalog; an invalid override is never replaced with stock.</summary>
    public static AreaMapPresentationCatalog Load(string stockDirectory, string? overrideDirectory)
    {
        var stock = ReadVerifiedStock(stockDirectory);
        var areas = new IAreaMapView[AreaIds.RetailCount];
        using var identity = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            using var baselineStream = new MemoryStream(stock.Maps[area], writable: false);
            var baseline = AreaMapPresentationAsset.Load(baselineStream, new AreaMapDecodeRules(area));
            IAreaMapView definition = new AreaMapStockRules(baseline, stock.StationCells[area]);
            // The identity covers baseline rule content too: equal override bytes do
            // not imply equal exploration semantics across different stock installs.
            AppendFramed(stock.Maps[area]);
            byte[] stationPlane = new byte[AreaMapLayout.WidthInTiles * AreaMapLayout.HeightInTiles];
            foreach (int cell in stock.StationCells[area]) stationPlane[cell] = 1;
            AppendFramed(stationPlane);
            string? replacement = overrideDirectory is null ? null : Path.Combine(overrideDirectory, AreaMapCatalogFormat.FileName(area));
            bool hasReplacement = replacement is not null && File.Exists(replacement);
            byte[] selected = hasReplacement ? File.ReadAllBytes(replacement!) : stock.Maps[area];
            try
            {
                using var stream = new MemoryStream(selected, writable: false);
                areas[AreaIds.ToIndex(area)] = AreaMapPresentationAsset.Load(stream, definition);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid {area} map presentation ({(hasReplacement ? replacement : stockDirectory)}): {error.Message}", error);
            }
            // Hash framed contents in fixed area order: identity changes after a valid
            // replacement is reloaded, without rewriting original extraction provenance.
            AppendFramed(selected);
        }
        string? atlasOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapTileAtlasFormat.FileName);
        byte[] atlasBytes = atlasOverride is not null && File.Exists(atlasOverride) ? File.ReadAllBytes(atlasOverride) : stock.Atlas;
        MapTileAtlas tiles;
        try { tiles = MapTileAtlas.Load(new MemoryStream(atlasBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map tile atlas ({atlasOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(atlasBytes);
        string? hudOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, HudTileAtlasFormat.FileName);
        byte[] hudBytes = hudOverride is not null && File.Exists(hudOverride) ? File.ReadAllBytes(hudOverride) : stock.HudAtlas;
        HudTileAtlas hudTiles;
        try { hudTiles = HudTileAtlas.Load(new MemoryStream(hudBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid HUD tile atlas ({hudOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(hudBytes);
        string? cycleOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapPaletteCycleFormat.FileName);
        byte[] cycleBytes = cycleOverride is not null && File.Exists(cycleOverride) ? File.ReadAllBytes(cycleOverride) : stock.HighlightCycle;
        MapPaletteCycle cycle;
        try { cycle = MapPaletteCycle.Load(new MemoryStream(cycleBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map highlight cycle ({cycleOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(cycleBytes);
        string? paletteOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapStaticPalettesFormat.FileName);
        byte[] paletteBytes = paletteOverride is not null && File.Exists(paletteOverride) ? File.ReadAllBytes(paletteOverride) : stock.Palettes;
        MapStaticPalettes palettes;
        try { palettes = MapStaticPalettes.Load(new MemoryStream(paletteBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map palettes ({paletteOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(paletteBytes);
        string? labelOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, WorldMapLabelFormat.FileName);
        byte[] labelBytes = labelOverride is not null && File.Exists(labelOverride) ? File.ReadAllBytes(labelOverride) : stock.Labels;
        WorldMapLabelLayout labels;
        try { labels = WorldMapLabelLayout.Load(new MemoryStream(labelBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid world-map labels ({labelOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(labelBytes);
        string? stationOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapStationLayoutFormat.FileName);
        byte[] stationBytes = stationOverride is not null && File.Exists(stationOverride) ? File.ReadAllBytes(stationOverride) : stock.Stations;
        MapStationLayout stations;
        try { stations = MapStationLayout.Load(new MemoryStream(stationBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map station labels ({stationOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(stationBytes);
        string? landmarkOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapLandmarkFormat.FileName);
        byte[] landmarkBytes = landmarkOverride is not null && File.Exists(landmarkOverride) ? File.ReadAllBytes(landmarkOverride) : stock.Landmarks;
        MapLandmarkLayout landmarks;
        try { landmarks = MapLandmarkLayout.Load(new MemoryStream(landmarkBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map landmarks ({landmarkOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(landmarkBytes);
        string? saveMarkerOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapSaveMarkerFormat.FileName);
        byte[] saveMarkerBytes = saveMarkerOverride is not null && File.Exists(saveMarkerOverride) ? File.ReadAllBytes(saveMarkerOverride) : stock.SaveMarkers;
        MapSaveMarkerLayout saveMarkers;
        try { saveMarkers = MapSaveMarkerLayout.Load(new MemoryStream(saveMarkerBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid save markers ({saveMarkerOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(saveMarkerBytes);
        string? arrowOverride = overrideDirectory is null ? null : Path.Combine(overrideDirectory, MapArrowFormat.FileName);
        byte[] arrowBytes = arrowOverride is not null && File.Exists(arrowOverride) ? File.ReadAllBytes(arrowOverride) : stock.Arrows;
        MapArrowPresentation arrows;
        try { arrows = MapArrowPresentation.Load(new MemoryStream(arrowBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map arrows ({arrowOverride ?? stockDirectory}): {error.Message}", error); }
        AppendFramed(arrowBytes);
        byte[] screenBytes = Select(MapScreenDefinitions.FileName, stock.Screens);
        byte[] worldFrontBytes = Select(WorldMapArtworkFormat.ForegroundFile, stock.WorldFront);
        byte[] worldBackBytes = Select(WorldMapArtworkFormat.BackgroundFile, stock.WorldBack);
        MapScreenPresentation screens;
        WorldMapArtwork artwork;
        try { screens = MapScreenPresentation.Load(new MemoryStream(screenBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map screens ({overrideDirectory ?? stockDirectory}/{MapScreenDefinitions.FileName}): {error.Message}", error); }
        try { artwork = WorldMapArtwork.Load(new MemoryStream(worldFrontBytes, writable: false), new MemoryStream(worldBackBytes, writable: false)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid world-map PNG artwork in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        byte[] spriteJson = Select(MapSpriteFormat.JsonFile, stock.SpriteJson), spritePng = Select(MapSpriteFormat.PngFile, stock.SpritePng);
        MapSpriteCatalog sprites;
        try { sprites = MapSpriteCatalog.Load(new MemoryStream(spriteJson), new MemoryStream(spritePng)); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid map sprites in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        MapTileAtlas pauseTiles;
        try { pauseTiles = MapTileAtlas.Load(new MemoryStream(Select(PauseTileAtlasFormat.FileName, stock.PauseTiles))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid pause artwork in {overrideDirectory ?? stockDirectory}/{PauseTileAtlasFormat.FileName}: {error.Message}", error); }
        PauseBackdropPresentation pauseBackdrops;
        try { pauseBackdrops = PauseBackdropPresentation.Load(new MemoryStream(Select(PauseBackdropDefinitions.FileName, stock.PauseBackdrops))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid pause backdrop in {overrideDirectory ?? stockDirectory}/{PauseBackdropDefinitions.FileName}: {error.Message}", error); }
        PauseSelectorPresentation pauseSelectors;
        try { pauseSelectors = PauseSelectorPresentation.Load(new MemoryStream(Select(PauseSelectorDefinitions.FileName, stock.PauseSelectors))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid pause selectors in {overrideDirectory ?? stockDirectory}/{PauseSelectorDefinitions.FileName}: {error.Message}", error); }
        PauseWireframePresentation pauseWireframes;
        try { pauseWireframes = PauseWireframePresentation.Load(new MemoryStream(Select(PauseWireframeDefinitions.FileName, stock.PauseWireframes))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid pause wireframes in {overrideDirectory ?? stockDirectory}/{PauseWireframeDefinitions.FileName}: {error.Message}", error); }
        PauseReserveTankPresentation pauseReserveTanks;
        try { pauseReserveTanks = PauseReserveTankPresentation.Load(new MemoryStream(Select(PauseReserveTankDefinitions.FileName, stock.PauseReserveTanks))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid reserve tank presentation in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        PauseReserveUiPresentation pauseReserveUi;
        try { pauseReserveUi = PauseReserveUiPresentation.Load(new MemoryStream(Select(PauseReserveUiDefinitions.FileName, stock.PauseReserveUi))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid reserve UI presentation in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        PauseEquipmentBasePresentation pauseEquipmentBase;
        try { pauseEquipmentBase = PauseEquipmentBasePresentation.Load(new MemoryStream(Select(PauseEquipmentBaseDefinitions.FileName, stock.PauseEquipmentBase))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid pause equipment base in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        PauseEquipmentLabelPresentation pauseEquipmentLabels;
        try { pauseEquipmentLabels = PauseEquipmentLabelPresentation.Load(new MemoryStream(Select(PauseEquipmentLabelDefinitions.FileName, stock.PauseEquipmentLabels))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid pause equipment labels in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        EscapeTimerPresentation escapeTimer;
        try { escapeTimer = EscapeTimerPresentation.Load(new MemoryStream(Select(EscapeTimerPresentationDefinitions.FileName, stock.EscapeTimer))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid escape timer presentation in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        EscapeTimerTileAtlas escapeTimerTiles;
        try { escapeTimerTiles = EscapeTimerTileAtlas.Load(new MemoryStream(Select(EscapeTimerTileAtlasFormat.FileName, stock.EscapeTimerTiles))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid escape timer artwork in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        GameplayHudPresentation gameplayHud;
        try { gameplayHud = GameplayHudPresentation.Load(new MemoryStream(Select(GameplayHudDefinitions.FileName, stock.GameplayHud))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid gameplay HUD presentation in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        GameOverPresentation gameOver;
        try { gameOver = GameOverPresentation.Load(new MemoryStream(Select(GameOverPresentationDefinitions.FileName, stock.GameOver))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid game-over presentation in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        GameOptionsPresentation gameOptions;
        try { gameOptions = GameOptionsPresentation.Load(new MemoryStream(Select(GameOptionsPresentationDefinitions.FileName, stock.GameOptions))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid options-menu presentation in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        FileSelectPresentation fileSelect;
        try { fileSelect = FileSelectPresentation.Load(new MemoryStream(Select(FileSelectPresentationDefinitions.FileName, stock.FileSelect))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid file-select presentation in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        GameplayMessageTitlePresentation gameplayMessageTitles;
        try { gameplayMessageTitles = GameplayMessageTitlePresentation.Load(new MemoryStream(Select(GameplayMessageTitleDefinitions.FileName, stock.GameplayMessageTitles))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid gameplay-message titles in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        GameplayMessagePanelPresentation gameplayMessagePanels;
        try { gameplayMessagePanels = GameplayMessagePanelPresentation.Load(new MemoryStream(Select(GameplayMessagePanelDefinitions.FileName, stock.GameplayMessagePanels))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid gameplay-message panels in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        GameplayMessageNoticePresentation gameplayMessageNotices;
        try { gameplayMessageNotices = GameplayMessageNoticePresentation.Load(new MemoryStream(Select(GameplayMessageNoticeDefinitions.FileName, stock.GameplayMessageNotices))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid gameplay-message notices in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        EscapeTypewriterPresentation escapeTypewriter;
        try { escapeTypewriter = EscapeTypewriterPresentation.Load(new MemoryStream(Select(EscapeTypewriterDefinitions.FileName, stock.EscapeTypewriter))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid escape typewriter in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        IntroNarrationPresentation introNarration;
        try { introNarration = IntroNarrationPresentation.Load(new MemoryStream(Select(IntroNarrationDefinitions.FileName, stock.IntroNarration))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid intro narration in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        IntroFontAtlas introFont;
        try { introFont = IntroFontAtlas.Load(new MemoryStream(Select(IntroFontAtlasFormat.FileName, stock.IntroFont))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid intro font in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        EndingTextPresentation endingText;
        try { endingText = EndingTextPresentation.Load(new MemoryStream(Select(EndingTextDefinitions.FileName, stock.EndingText))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid ending text in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        EndingFontAtlas endingFont;
        try { endingFont = EndingFontAtlas.Load(new MemoryStream(Select(EndingFontAtlasFormat.FileName, stock.EndingFont))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid ending font in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        CreditsPresentation staffCredits;
        try { staffCredits = CreditsPresentation.Load(new MemoryStream(Select(CreditsPresentationDefinitions.FileName, stock.StaffCredits))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid staff credits in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        TitleGraphicsPresentation titleGraphics;
        try
        {
            titleGraphics = TitleGraphicsPresentation.Load(
                new MemoryStream(Select(TitleGraphicsFormat.Mode7TilesFile, stock.TitleMode7Tiles)),
                new MemoryStream(Select(TitleGraphicsFormat.Mode7MapFile, stock.TitleMode7Map)),
                new MemoryStream(Select(TitleGraphicsFormat.ObjectTilesFile, stock.TitleObjectTiles)),
                new MemoryStream(Select(TitleGraphicsFormat.BabyTilesFile, stock.TitleBabyTiles)));
        }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid title graphics in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        TitlePalettePresentation titlePalette;
        try { titlePalette = TitlePalettePresentation.Load(new MemoryStream(Select(TitlePaletteFormat.FileName, stock.TitlePalette))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid title palette in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        TitleGradientPresentation titleGradient;
        try { titleGradient = TitleGradientPresentation.Load(new MemoryStream(Select(TitleGradientFormat.FileName, stock.TitleGradient))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid title gradient in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        RoomPaletteFxPresentation roomPaletteFx;
        try { roomPaletteFx = RoomPaletteFxPresentation.Load(new MemoryStream(Select(RoomPaletteFxPresentationFormat.FileName, stock.RoomPaletteFx))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid room palette effects in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        MotherBrainHealthPalettePresentation motherBrainHealthPalette;
        try { motherBrainHealthPalette = MotherBrainHealthPalettePresentation.Load(new MemoryStream(Select(MotherBrainHealthPaletteFormat.FileName, stock.MotherBrainHealthPalette))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid Mother Brain health palette in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        MotherBrainRainbowPalettePresentation motherBrainRainbowPalette;
        try { motherBrainRainbowPalette = MotherBrainRainbowPalettePresentation.Load(new MemoryStream(Select(MotherBrainRainbowPaletteFormat.FileName, stock.MotherBrainRainbowPalette))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid Mother Brain rainbow palette in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        RoomFxAnimatedTileAtlas roomFxAnimatedTiles;
        try { roomFxAnimatedTiles = RoomFxAnimatedTileAtlas.Load(new MemoryStream(Select(RoomFxAnimatedTileAtlasFormat.FileName, stock.RoomFxAnimatedTiles))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid room-FX animated-tile artwork in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        RoomFxLayer3TilemapCatalog roomFxLayer3Tilemaps;
        try { roomFxLayer3Tilemaps = RoomFxLayer3TilemapCatalog.Load(new MemoryStream(Select(RoomFxLayer3TilemapFormat.FileName, stock.RoomFxLayer3Tilemaps))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid room-FX BG3 tilemaps in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        RoomFxPaletteBlendCatalog roomFxPaletteBlends;
        try { roomFxPaletteBlends = RoomFxPaletteBlendCatalog.Load(new MemoryStream(Select(RoomFxPaletteBlendDefinitions.FileName, stock.RoomFxPaletteBlends))); }
        catch (InvalidDataException error) { throw new InvalidDataException($"Invalid room-FX blend palettes in {overrideDirectory ?? stockDirectory}: {error.Message}", error); }
        return new(areas, Convert.ToHexString(identity.GetHashAndReset()), tiles, hudTiles, cycle, palettes, labels, stations, landmarks, saveMarkers, arrows, screens, artwork, sprites, pauseTiles, pauseBackdrops, pauseWireframes, pauseSelectors, pauseReserveTanks, pauseReserveUi, pauseEquipmentBase, pauseEquipmentLabels, escapeTimer, escapeTimerTiles, gameplayHud, gameOver, gameOptions, fileSelect, gameplayMessageTitles, gameplayMessagePanels, gameplayMessageNotices, escapeTypewriter, introNarration, introFont, endingText, endingFont, staffCredits, titleGraphics, titlePalette, titleGradient, roomPaletteFx, motherBrainHealthPalette, motherBrainRainbowPalette, roomFxAnimatedTiles, roomFxLayer3Tilemaps, roomFxPaletteBlends);

        byte[] Select(string name, byte[] baseline)
        {
            string? path = overrideDirectory is null ? null : Path.Combine(overrideDirectory, name);
            byte[] bytes = path is not null && File.Exists(path) ? File.ReadAllBytes(path) : baseline;
            AppendFramed(bytes);
            return bytes;
        }

        void AppendFramed(byte[] bytes)
        {
            Span<byte> length = stackalloc byte[sizeof(int)];
            BinaryPrimitives.WriteInt32LittleEndian(length, bytes.Length);
            identity.AppendData(length);
            identity.AppendData(bytes);
        }
    }

    /// <summary>Installer integrity check; never repairs files or touches the override directory.</summary>
    public static void ValidateStock(string directory) => _ = ReadVerifiedStock(directory);

    private static (Dictionary<AreaId, byte[]> Maps, Dictionary<AreaId, HashSet<int>> StationCells, byte[] Atlas, byte[] HudAtlas, byte[] HighlightCycle, byte[] Palettes, byte[] Labels, byte[] Stations, byte[] Landmarks, byte[] SaveMarkers, byte[] Arrows, byte[] Screens, byte[] WorldFront, byte[] WorldBack, byte[] SpriteJson, byte[] SpritePng, byte[] PauseTiles, byte[] PauseBackdrops, byte[] PauseWireframes, byte[] PauseSelectors, byte[] PauseReserveTanks, byte[] PauseReserveUi, byte[] PauseEquipmentBase, byte[] PauseEquipmentLabels, byte[] EscapeTimer, byte[] EscapeTimerTiles, byte[] GameplayHud, byte[] GameOver, byte[] GameOptions, byte[] FileSelect, byte[] GameplayMessageTitles, byte[] GameplayMessagePanels, byte[] GameplayMessageNotices, byte[] EscapeTypewriter, byte[] IntroNarration, byte[] IntroFont, byte[] EndingText, byte[] EndingFont, byte[] StaffCredits, byte[] TitleMode7Tiles, byte[] TitleMode7Map, byte[] TitleObjectTiles, byte[] TitleBabyTiles, byte[] TitlePalette, byte[] TitleGradient, byte[] RoomPaletteFx, byte[] MotherBrainHealthPalette, byte[] MotherBrainRainbowPalette, byte[] RoomFxAnimatedTiles, byte[] RoomFxLayer3Tilemaps, byte[] RoomFxPaletteBlends) ReadVerifiedStock(string directory)
    {
        AreaMapCatalogManifest manifest;
        try
        {
            manifest = JsonSerializer.Deserialize<AreaMapCatalogManifest>(
                File.ReadAllBytes(Path.Combine(directory, AreaMapCatalogFormat.ManifestFile)), MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Map catalog manifest is null.");
        }
        catch (JsonException error) { throw new InvalidDataException($"Invalid map catalog manifest in {directory}.", error); }
        if (manifest.Version != AreaMapCatalogFormat.Version || manifest.Sha256 is null || manifest.Sha256.Count != AreaIds.RetailCount + 50)
            throw new InvalidDataException("Map catalog manifest must contain the supported version, seven maps and all fifty shared presentation resource hashes.");
        var result = new Dictionary<AreaId, byte[]>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            string file = AreaMapCatalogFormat.FileName(area);
            result.Add(area, ReadChecked(file));
        }
        Dictionary<string, int[]> masks;
        try
        {
            masks = JsonSerializer.Deserialize<Dictionary<string, int[]>>(ReadChecked(AreaMapCatalogFormat.StationRevealFile), MapPresentationFormat.JsonOptions)
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
        byte[] atlas = ReadChecked(MapTileAtlasFormat.FileName);
        _ = MapTileAtlas.Load(new MemoryStream(atlas, writable: false));
        byte[] hudAtlas = ReadChecked(HudTileAtlasFormat.FileName);
        _ = HudTileAtlas.Load(new MemoryStream(hudAtlas, writable: false));
        byte[] highlightCycle = ReadChecked(MapPaletteCycleFormat.FileName);
        _ = MapPaletteCycle.Load(new MemoryStream(highlightCycle, writable: false));
        byte[] palettes = ReadChecked(MapStaticPalettesFormat.FileName);
        _ = MapStaticPalettes.Load(new MemoryStream(palettes, writable: false));
        byte[] labels = ReadChecked(WorldMapLabelFormat.FileName);
        _ = WorldMapLabelLayout.Load(new MemoryStream(labels, writable: false));
        byte[] stations = ReadChecked(MapStationLayoutFormat.FileName);
        _ = MapStationLayout.Load(new MemoryStream(stations, writable: false));
        byte[] landmarks = ReadChecked(MapLandmarkFormat.FileName);
        _ = MapLandmarkLayout.Load(new MemoryStream(landmarks, writable: false));
        byte[] saveMarkers = ReadChecked(MapSaveMarkerFormat.FileName);
        _ = MapSaveMarkerLayout.Load(new MemoryStream(saveMarkers, writable: false));
        byte[] arrows = ReadChecked(MapArrowFormat.FileName);
        _ = MapArrowPresentation.Load(new MemoryStream(arrows, writable: false));
        byte[] screens = ReadChecked(MapScreenDefinitions.FileName);
        _ = MapScreenPresentation.Load(new MemoryStream(screens, writable: false));
        byte[] front = ReadChecked(WorldMapArtworkFormat.ForegroundFile), back = ReadChecked(WorldMapArtworkFormat.BackgroundFile);
        _ = WorldMapArtwork.Load(new MemoryStream(front, writable: false), new MemoryStream(back, writable: false));
        byte[] spriteJson = ReadChecked(MapSpriteFormat.JsonFile), spritePng = ReadChecked(MapSpriteFormat.PngFile);
        _ = MapSpriteCatalog.Load(new MemoryStream(spriteJson), new MemoryStream(spritePng));
        byte[] pauseTiles = ReadChecked(PauseTileAtlasFormat.FileName);
        _ = MapTileAtlas.Load(new MemoryStream(pauseTiles));
        byte[] pauseBackdrops = ReadChecked(PauseBackdropDefinitions.FileName);
        _ = PauseBackdropPresentation.Load(new MemoryStream(pauseBackdrops));
        byte[] pauseWireframes = ReadChecked(PauseWireframeDefinitions.FileName);
        _ = PauseWireframePresentation.Load(new MemoryStream(pauseWireframes));
        byte[] pauseSelectors = ReadChecked(PauseSelectorDefinitions.FileName);
        _ = PauseSelectorPresentation.Load(new MemoryStream(pauseSelectors));
        byte[] pauseReserveTanks = ReadChecked(PauseReserveTankDefinitions.FileName);
        _ = PauseReserveTankPresentation.Load(new MemoryStream(pauseReserveTanks));
        byte[] pauseReserveUi = ReadChecked(PauseReserveUiDefinitions.FileName);
        _ = PauseReserveUiPresentation.Load(new MemoryStream(pauseReserveUi));
        byte[] pauseEquipmentBase = ReadChecked(PauseEquipmentBaseDefinitions.FileName);
        _ = PauseEquipmentBasePresentation.Load(new MemoryStream(pauseEquipmentBase));
        byte[] pauseEquipmentLabels = ReadChecked(PauseEquipmentLabelDefinitions.FileName);
        _ = PauseEquipmentLabelPresentation.Load(new MemoryStream(pauseEquipmentLabels));
        byte[] escapeTimer = ReadChecked(EscapeTimerPresentationDefinitions.FileName);
        _ = EscapeTimerPresentation.Load(new MemoryStream(escapeTimer));
        byte[] escapeTimerTiles = ReadChecked(EscapeTimerTileAtlasFormat.FileName);
        _ = EscapeTimerTileAtlas.Load(new MemoryStream(escapeTimerTiles));
        byte[] gameplayHud = ReadChecked(GameplayHudDefinitions.FileName);
        _ = GameplayHudPresentation.Load(new MemoryStream(gameplayHud));
        byte[] gameOver = ReadChecked(GameOverPresentationDefinitions.FileName);
        _ = GameOverPresentation.Load(new MemoryStream(gameOver));
        byte[] gameOptions = ReadChecked(GameOptionsPresentationDefinitions.FileName);
        _ = GameOptionsPresentation.Load(new MemoryStream(gameOptions));
        byte[] fileSelect = ReadChecked(FileSelectPresentationDefinitions.FileName);
        _ = FileSelectPresentation.Load(new MemoryStream(fileSelect));
        byte[] gameplayMessageTitles = ReadChecked(GameplayMessageTitleDefinitions.FileName);
        _ = GameplayMessageTitlePresentation.Load(new MemoryStream(gameplayMessageTitles));
        byte[] gameplayMessagePanels = ReadChecked(GameplayMessagePanelDefinitions.FileName);
        _ = GameplayMessagePanelPresentation.Load(new MemoryStream(gameplayMessagePanels));
        byte[] gameplayMessageNotices = ReadChecked(GameplayMessageNoticeDefinitions.FileName);
        _ = GameplayMessageNoticePresentation.Load(new MemoryStream(gameplayMessageNotices));
        byte[] escapeTypewriter = ReadChecked(EscapeTypewriterDefinitions.FileName);
        _ = EscapeTypewriterPresentation.Load(new MemoryStream(escapeTypewriter));
        byte[] introNarration = ReadChecked(IntroNarrationDefinitions.FileName);
        _ = IntroNarrationPresentation.Load(new MemoryStream(introNarration));
        byte[] introFont = ReadChecked(IntroFontAtlasFormat.FileName);
        _ = IntroFontAtlas.Load(new MemoryStream(introFont));
        byte[] endingText = ReadChecked(EndingTextDefinitions.FileName);
        _ = EndingTextPresentation.Load(new MemoryStream(endingText));
        byte[] endingFont = ReadChecked(EndingFontAtlasFormat.FileName);
        _ = EndingFontAtlas.Load(new MemoryStream(endingFont));
        byte[] staffCredits = ReadChecked(CreditsPresentationDefinitions.FileName);
        _ = CreditsPresentation.Load(new MemoryStream(staffCredits));
        byte[] titleMode7Tiles = ReadChecked(TitleGraphicsFormat.Mode7TilesFile);
        byte[] titleMode7Map = ReadChecked(TitleGraphicsFormat.Mode7MapFile);
        byte[] titleObjectTiles = ReadChecked(TitleGraphicsFormat.ObjectTilesFile);
        byte[] titleBabyTiles = ReadChecked(TitleGraphicsFormat.BabyTilesFile);
        _ = TitleGraphicsPresentation.Load(
            new MemoryStream(titleMode7Tiles),
            new MemoryStream(titleMode7Map),
            new MemoryStream(titleObjectTiles),
            new MemoryStream(titleBabyTiles));
        byte[] titlePalette = ReadChecked(TitlePaletteFormat.FileName);
        _ = TitlePalettePresentation.Load(new MemoryStream(titlePalette));
        byte[] titleGradient = ReadChecked(TitleGradientFormat.FileName);
        _ = TitleGradientPresentation.Load(new MemoryStream(titleGradient));
        byte[] roomPaletteFx = ReadChecked(RoomPaletteFxPresentationFormat.FileName);
        _ = RoomPaletteFxPresentation.Load(new MemoryStream(roomPaletteFx));
        byte[] motherBrainHealthPalette = ReadChecked(MotherBrainHealthPaletteFormat.FileName);
        _ = MotherBrainHealthPalettePresentation.Load(new MemoryStream(motherBrainHealthPalette));
        byte[] motherBrainRainbowPalette = ReadChecked(MotherBrainRainbowPaletteFormat.FileName);
        _ = MotherBrainRainbowPalettePresentation.Load(new MemoryStream(motherBrainRainbowPalette));
        byte[] roomFxAnimatedTiles = ReadChecked(RoomFxAnimatedTileAtlasFormat.FileName);
        _ = RoomFxAnimatedTileAtlas.Load(new MemoryStream(roomFxAnimatedTiles));
        byte[] roomFxLayer3Tilemaps = ReadChecked(RoomFxLayer3TilemapFormat.FileName);
        _ = RoomFxLayer3TilemapCatalog.Load(new MemoryStream(roomFxLayer3Tilemaps));
        byte[] roomFxPaletteBlends = ReadChecked(RoomFxPaletteBlendDefinitions.FileName);
        _ = RoomFxPaletteBlendCatalog.Load(new MemoryStream(roomFxPaletteBlends));
        return (result, stationCells, atlas, hudAtlas, highlightCycle, palettes, labels, stations, landmarks, saveMarkers, arrows, screens, front, back, spriteJson, spritePng, pauseTiles, pauseBackdrops, pauseWireframes, pauseSelectors, pauseReserveTanks, pauseReserveUi, pauseEquipmentBase, pauseEquipmentLabels, escapeTimer, escapeTimerTiles, gameplayHud, gameOver, gameOptions, fileSelect, gameplayMessageTitles, gameplayMessagePanels, gameplayMessageNotices, escapeTypewriter, introNarration, introFont, endingText, endingFont, staffCredits, titleMode7Tiles, titleMode7Map, titleObjectTiles, titleBabyTiles, titlePalette, titleGradient, roomPaletteFx, motherBrainHealthPalette, motherBrainRainbowPalette, roomFxAnimatedTiles, roomFxLayer3Tilemaps, roomFxPaletteBlends);

        byte[] ReadChecked(string file)
        {
            if (!manifest.Sha256.TryGetValue(file, out string? expected))
                throw new InvalidDataException($"Map catalog manifest is missing {file}.");
            byte[] bytes = File.ReadAllBytes(Path.Combine(directory, file));
            if (!string.Equals(expected, Convert.ToHexString(SHA256.HashData(bytes)), StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Stock map {file} failed its SHA-256 check. Put edits in the overrides directory, not stock content.");
            return bytes;
        }
    }
}

/// <summary>Stock content provenance, separate from a catalog's selected replacement identity.</summary>
public sealed record AreaMapCatalogManifest
{
    public required int Version { get; init; }
    public required string SourceCartridgeSha256 { get; init; }
    public required Dictionary<string, string> Sha256 { get; init; }
}

public static class AreaMapCatalogFormat
{
    public const int Version = 62;
    /// <summary>Bundled authored reveal mask: logical row-major cell indexes, not SRAM offsets or editable engine code.</summary>
    public const string StationRevealFile = "station-reveal.json";
    public const string ManifestFile = "manifest.json";
    public static string FileName(AreaId area)
    {
        _ = AreaIds.ToIndex(area);
        return area.ToString().ToLowerInvariant() + ".json";
    }
}
