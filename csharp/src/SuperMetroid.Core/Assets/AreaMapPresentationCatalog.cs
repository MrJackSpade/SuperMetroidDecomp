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
    private readonly IAreaMapView[] areas;
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
    public MotherBrainRoomColorPresentation MotherBrainRoomColors { get; private set; } = null!;
    public RoomFxAnimatedTileAtlas RoomFxAnimatedTiles { get; }
    public RoomFxLayer3TilemapCatalog RoomFxLayer3Tilemaps { get; }
    public RoomFxPaletteBlendCatalog RoomFxPaletteBlends { get; }
    public PowerBombFixedColorCatalog PowerBombFixedColors { get; }
    public SamusVisorColorCatalog SamusVisorColors { get; }
    public SamusHurtColorCatalog SamusHurtColors { get; }
    public SamusSuitColorCatalog SamusSuitColors { get; }
    public SamusFullBodyCycleColorCatalog SamusFullBodyCycleColors { get; }
    public CrystalFlashColorCatalog CrystalFlashColors { get; }
    public SamusChargeColorCatalog SamusChargeColors { get; }
    public CeresRidleyColorCatalog CeresRidleyColors { get; }
    public CeresRidleyMode7ColorCatalog CeresRidleyMode7Colors { get; }
    public SamusHyperBeamColorCatalog SamusHyperBeamColors { get; }
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
        private readonly string directory;
        private readonly AreaMapCatalogManifest manifest;
        private readonly HashSet<string> read = new(StringComparer.Ordinal);
        // Stock bytes live only for one catalog load; every buffer returns to the pool on dispose.
        private readonly List<byte[]> rented = [];

        private VerifiedStockReader(string directory, AreaMapCatalogManifest manifest)
        {
            this.directory = directory;
            this.manifest = manifest;
        }

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

        public void Dispose()
        {
            foreach (byte[] buffer in rented) ArrayPool<byte>.Shared.Return(buffer);
            rented.Clear();
        }

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
    public required int Version { get; init; }
    public required string SourceCartridgeSha256 { get; init; }
    public required Dictionary<string, string> Sha256 { get; init; }
}

public static class AreaMapCatalogFormat
{
    public const int Version = 78;
    /// <summary>Manifest-bound non-area artwork files, including Ceres Mode-7 colors.</summary>
    public const int SharedResourceCount = 61;
    /// <summary>Bundled authored reveal mask: logical row-major cell indexes, not SRAM offsets or editable engine code.</summary>
    public const string StationRevealFile = "station-reveal.json";
    public const string ManifestFile = "manifest.json";
    public static string FileName(AreaId area)
    {
        _ = AreaIds.ToIndex(area);
        return area.ToString().ToLowerInvariant() + ".json";
    }
}
