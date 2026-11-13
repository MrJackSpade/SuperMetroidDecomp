using System.Buffers.Binary;
using System.Buffers;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// Reads the installed map and menu presentation (#1409), as the player may have edited it in place, compiled into the
/// presentation catalog the game binds. The game's read (<see cref="Load"/>) reads no palette document (#1544): the colours
/// it shows that no picture holds are the scene colours (<see cref="SceneColourFiles"/>), and the colour documents only the
/// import pre-colours sheets from are read by the import's read (<see cref="LoadForImport"/>) alone. Neither reads a
/// character page from the installation: the pages are import data (#1636), which only the import's read reads, from the
/// component's import-data directory (<see cref="PageFiles"/>).
/// </summary>
public static class MapPresentationFiles
{
    /// <summary>
    /// The component's character and tile pages (#1636), which the extraction writes to its import-data directory and only
    /// the import's read reads: the installation never holds them. The manifest lists their hashes beside the documents'.
    /// </summary>
    internal static readonly string[] PageFiles =
    [
        MapTileAtlasFormat.FileName, PauseTileAtlasFormat.FileName, HudTileAtlasFormat.FileName,
        WorldMapArtworkFormat.ForegroundFile, WorldMapArtworkFormat.BackgroundFile, MapSpriteFormat.PngFile,
        EscapeTimerTileAtlasFormat.FileName, IntroFontCharacters.FileName, EndingFontAtlasFormat.FileName,
        TitleGraphicsFormat.FloorTilesFile, TitleGraphicsFormat.ObjectTilesFile, TitleGraphicsFormat.BabyTilesFile,
        RoomFxAnimatedTileAtlasFile.FileName,
    ];

    /// <summary>
    /// The component's documents only the import reads whose entries are the original's packed tile placements (#1638), which
    /// the extraction writes to its import-data directory beside the pages: the installation never holds them. The manifest
    /// lists their hashes beside the installed documents'.
    /// </summary>
    internal static readonly string[] ImportDocumentFiles =
    [
        FileSelectPresentationFormat.ArtworkFileName, PauseBackdropDefinitions.FileName, PauseWireframeDefinitions.FileName,
        PauseEquipmentBaseDefinitions.FileName, PauseEquipmentLabelArtworkDefinitions.FileName, PauseReserveUiDefinitions.FileName,
        RoomFxLayer3TilemapFormat.FileName, GameplayMessageTitleDefinitions.FileName, GameplayMessagePanelDefinitions.FileName,
        GameplayMessageNoticeDefinitions.FileName, GameplayHudDocumentDefinitions.ArtworkFileName,
    ];

    /// <summary>
    /// The installed documents only the import reads with the pages, which the game's read leaves unread: the map sprites'
    /// compositions and the title floor's map, read together with their pages.
    /// </summary>
    private static readonly string[] ImportCompositionFiles = [MapSpriteFormat.JsonFile, TitleGraphicsFormat.FloorMapFile];
    /// <summary>The installed colour documents only the import reads, which the game's read leaves unread.</summary>
    private static readonly string[] ImportColourFiles =
    [
        MapStaticPalettesFormat.FileName, TitlePaletteFormat.FileName, MotherBrainHealthPaletteFile.FileName,
        MotherBrainRoomColorFormat.FileName, RoomFxPaletteBlendDefinitions.FileName, SamusVisorColorFile.FileName,
        SamusHurtColorFile.FileName, SamusSuitColorFile.FileName, SamusFullBodyCycleColorFormat.FileName,
        CrystalFlashColorFile.FileName, SamusChargeColorFormat.FileName, CeresRidleyColorFormat.FileName,
        CeresRidleyFloorColorFormat.FileName, SamusHyperBeamColorFormat.FileName,
    ];

    /// <summary>
    /// Reads all areas atomically into the catalog the game binds (<see cref="InstalledAreaMapPresentation.Game"/>), beside
    /// the source artwork only the import composes from; a file that does not decode fails loudly, naming the file. The colour
    /// documents only the import reads are left unread.
    /// </summary>
    public static InstalledAreaMapPresentation Load(string directory)
    {
        using var installed = InstalledMapReader.Open(directory, importData: null);
        GameRead game = ReadGame(installed);
        installed.RequireAllManifestFilesRead([.. ImportColourFiles, .. ImportCompositionFiles, .. PageFiles, .. ImportDocumentFiles]);
        return new InstalledAreaMapPresentation(new AreaMapPresentation(game.Parts), game.Sources);
    }

    /// <summary>
    /// Reads all areas atomically into the catalog the import composes from: the game's catalog with the characters, from
    /// the component's import data <paramref name="importData"/> (#1636), and the colours only the import pre-colours sheets
    /// and pictures from (#1544). A file that does not decode fails loudly, naming the file.
    /// </summary>
    /// <param name="directory">The component's directory, holding the manifest and the documents.</param>
    /// <param name="importData">The component's import-data directory, holding the character and tile pages.</param>
    public static ImportAreaMapPresentation LoadForImport(string directory, string importData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(importData);
        using var installed = InstalledMapReader.Open(directory, importData);
        GameRead game = ReadGame(installed);
        var characters = new ImportPresentationCharacters(
            new MapCharacterArtwork(
                Page(MapTileAtlasFormat.FileName, "map tile atlas", s => MapTileCharacters.Read(s)),
                Page(HudTileAtlasFormat.FileName, "HUD tile atlas", s => HudTileAtlasPng.Read(s)),
                Page(PauseTileAtlasFormat.FileName, "pause artwork", s => MapTileCharacters.Read(s))),
            Pages<MapSpriteCompositions>([MapSpriteFormat.JsonFile, MapSpriteFormat.PngFile], "map sprites", s => MapSpriteArtwork.Read(s[0], s[1])),
            Page(EscapeTimerTileAtlasFormat.FileName, "escape timer artwork", s => EscapeTimerCharacters.Read(s)),
            Page(RoomFxAnimatedTileAtlasFile.FileName, "room-FX presentation", s => RoomFxAnimatedTileAtlasPng.Read(s)),
            Pages<WorldMapCharacters>([WorldMapArtworkFormat.ForegroundFile, WorldMapArtworkFormat.BackgroundFile],
                "world-map PNG artwork", s => WorldMapCharacters.Read(s[0], s[1])),
            Page(IntroFontCharacters.FileName, "intro font", s => IntroFontCharacters.Read(s)),
            Page(EndingFontAtlasFormat.FileName, "ending font", s => EndingFontCharacters.Read(s)),
            Pages<TitleGraphicsCharacters>(
                [TitleGraphicsFormat.FloorTilesFile, TitleGraphicsFormat.FloorMapFile, TitleGraphicsFormat.ObjectTilesFile, TitleGraphicsFormat.BabyTilesFile],
                "title graphics", s => TitleGraphicsCharacters.Read(s[0], s[1], s[2], s[3])));
        var colours = new ImportPresentationColours(
            new SamusMapColourTables(
                Colours(SamusVisorColorFile.FileName, "Samus visor colors", s => SamusVisorColorFile.Read(s)),
                Colours(SamusHurtColorFile.FileName, "Samus hurt colors", s => SamusHurtColorFile.Read(s)),
                Colours(SamusSuitColorFile.FileName, "Samus suit colors", s => SamusSuitColorFile.Read(s)),
                Colours(SamusFullBodyCycleColorFormat.FileName, "Samus full-body cycle colors", s => SamusFullBodyCycleColorFile.Read(s).ToColours()),
                Colours(SamusChargeColorFormat.FileName, "Samus charge colors", s => SamusChargeColorFile.Read(s).ToColours()),
                Colours(SamusHyperBeamColorFormat.FileName, "Samus Hyper Beam colors", s => SamusHyperBeamColorFile.Read(s).ToColours()),
                Colours(CrystalFlashColorFile.FileName, "Crystal Flash colors", s => CrystalFlashColorFile.Read(s))),
            Colours(TitlePaletteFormat.FileName, "title palette", s => TitlePaletteFile.Read(s)),
            Colours(MapStaticPalettesFormat.FileName, "map palettes", s => MapStaticPalettesFile.Read(s)),
            Colours(CeresRidleyColorFormat.FileName, "Ceres Ridley colors", s => CeresRidleyColorFile.Read(s)),
            Colours(CeresRidleyFloorColorFormat.FileName, "Ceres Ridley Mode-7 colors", s => CeresRidleyMode7ColorFile.Read(s)),
            Colours(MotherBrainHealthPaletteFile.FileName, "Mother Brain health palette", s => MotherBrainHealthPaletteFile.Read(s)),
            Colours(MotherBrainRoomColorFormat.FileName, "Mother Brain room colors", s => MotherBrainRoomColorFile.Read(s)),
            Colours(RoomFxPaletteBlendDefinitions.FileName, "room-FX blends", s => RoomFxPaletteBlendFile.Read(s)),
            game.RoomPaletteFx,
            game.MotherBrainRainbow);
        var documents = new ImportPresentationDocuments(
            Compile<FileSelectSourcePresentation>([installed.Read(FileSelectPresentationFormat.FileName), installed.Read(FileSelectPresentationFormat.ArtworkFileName)],
                [FileSelectPresentationFormat.FileName, FileSelectPresentationFormat.ArtworkFileName], installed, "file-select artwork",
                streams => FileSelectSourcePresentation.Load(streams[0], streams[1])),
            new PauseScreenArtwork(
                Document(PauseBackdropDefinitions.FileName, "pause backdrop", s => PauseBackdropPresentation.Load(s)),
                Document(PauseWireframeDefinitions.FileName, "pause wireframes", s => PauseWireframePresentation.Load(s)),
                Document(PauseEquipmentBaseDefinitions.FileName, "pause equipment base", s => PauseEquipmentBasePresentation.Load(s)),
                Document(PauseEquipmentLabelArtworkDefinitions.FileName, "pause equipment labels", s => PauseEquipmentLabelPresentation.Load(s)),
                Document(PauseReserveUiDefinitions.FileName, "reserve UI presentation", s => PauseReserveUiFile.Read(s))),
            Document(RoomFxLayer3TilemapFormat.FileName, "room-FX presentation", s => RoomFxLayer3Pages.Load(s)),
            new GameplayMessageArtwork(
                Document(GameplayMessageTitleDefinitions.FileName, "gameplay-message titles", s => GameplayMessageTitlePresentation.Load(s)),
                Document(GameplayMessagePanelDefinitions.FileName, "gameplay-message panels", s => GameplayMessagePanelPresentation.Load(s)),
                Document(GameplayMessageNoticeDefinitions.FileName, "gameplay-message notices", s => GameplayMessageNoticePresentation.Load(s))),
            Document(GameplayHudDocumentDefinitions.ArtworkFileName, "gameplay HUD presentation", s => GameplayHudPresentationFile.Read(s)));
        installed.RequireAllManifestFilesRead([]);
        return new ImportAreaMapPresentation(new AreaMapPresentation(game.Parts), game.Sources, characters, colours, documents);

        // The import's colours and characters are not part of the presentation's identity: the game reads none of them,
        // and the sheets composed from them carry their own.
        T Colours<T>(string file, string description, Func<Stream, T> compile) where T : class =>
            Compile<T>([installed.Read(file)], [file], installed, description, streams => compile(streams[0]));

        // An import-only document is read from the import data (#1638).
        T Document<T>(string file, string description, Func<Stream, T> compile) where T : class =>
            Compile<T>([installed.Read(file)], [file], installed, description, streams => compile(streams[0]));

        T Page<T>(string file, string description, Func<Stream, T> compile) where T : class =>
            Pages<T>([file], description, streams => compile(streams[0]));

        // A page is read from the import data and a composition read with it from the documents (#1636).
        T Pages<T>(string[] files, string description, Func<Stream[], T> compile) where T : class =>
            Compile(files.Select(installed.Read).ToArray(), files, installed, description, compile);
    }

    /// <summary>
    /// Reads what the game binds: the areas, the presentation resources and the scene colours, framing each into the
    /// presentation's identity in a fixed order.
    /// </summary>
    private static GameRead ReadGame(InstalledMapReader installed)
    {
        Dictionary<AreaId, HashSet<int>> stationCells = installed.ReadStationCells();
        var areas = new IAreaMapView[AreaIds.RetailCount];
        using var identity = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            string file = AreaMapCatalogFormat.FileName(area);
            ArraySegment<byte> mapBytes = installed.Read(file);
            try
            {
                using var baselineStream = ReadOnlyStream(mapBytes);
                AreaMapPresentationAsset baseline = AreaMapPresentationFile.Read(baselineStream, new AreaMapDecodeRules(area));
                IAreaMapView definition = new AreaMapStockRules(baseline, stationCells[area]);
                // The cells, decoded again under the exploration rules they were read to derive.
                using var stream = ReadOnlyStream(mapBytes);
                areas[AreaIds.ToIndex(area)] = AreaMapPresentationFile.Read(stream, definition);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Invalid {area} map presentation ({installed.PathOf(file)}): {error.Message}", error);
            }
            // Hash framed contents in fixed area order, the station cells with the map they reveal.
            AppendFramed(mapBytes);
            byte[] stationPlane = new byte[AreaMapSize.WidthInTiles * AreaMapSize.HeightInTiles];
            foreach (int cell in stationCells[area]) stationPlane[cell] = 1;
            AppendFramed(stationPlane);
        }
        // Each resource below is framed into the identity in this fixed order. No character page is among them (#1636).
        MapPaletteCycle cycle = Resource(MapPaletteCycleFile.FileName, "map highlight cycle", s => MapPaletteCycleFile.Read(s));
        SceneColours sceneColours = Resource(SceneColourFiles.FileName, "scene colours", s => SceneColourFiles.Read(s));
        WorldMapLabelPositions labels = Resource(WorldMapLabelFormat.FileName, "world-map labels", s => WorldMapLabelFile.Read(s));
        MapStationPositions stations = Resource(MapStationLayoutFormat.FileName, "map station labels", s => MapStationLayoutFile.Read(s));
        MapLandmarkPositions landmarks = Resource(MapLandmarkFormat.FileName, "map landmarks", s => MapLandmarkFile.Read(s));
        MapSaveMarkerPositions saveMarkers = Resource(MapSaveMarkerFormat.FileName, "save markers", s => MapSaveMarkerFile.Read(s));
        MapArrowPresentation arrows = Resource(MapArrowFormat.FileName, "map arrows", s => MapArrowFile.Read(s));
        MapScreenPresentation screens = Resource(MapScreenDefinitions.FileName, "map screens", s => MapScreenPresentation.Load(s));
        PauseSelectorSourcePresentation pauseSelectors = Resource(PauseSelectorFormat.FileName, "pause selectors", s => PauseSelectorSourcePresentation.Load(s));
        PauseReserveTankSourcePresentation pauseReserveTanks = Resource(PauseReserveTankFormat.FileName, "reserve tank presentation", s => PauseReserveTankSourcePresentation.Load(s));
        EscapeTimerPresentation escapeTimer = Resource(EscapeTimerLayoutDefinitions.FileName, "escape timer presentation", s => EscapeTimerPresentationFile.Load(s));
        GameplayHudPresentation gameplayHud = Resource(GameplayHudDocumentDefinitions.FileName, "gameplay HUD layout", s => GameplayHudLayoutFile.Read(s));
        GameOverSourcePresentation gameOver = Resource(GameOverPresentationFormat.FileName, "game-over presentation", s => GameOverSourcePresentation.Load(s));
        GameOptionsSourcePresentation gameOptions = Resource(GameOptionsPresentationFormat.FileName, "options-menu presentation", s => GameOptionsSourcePresentation.Load(s));
        FileSelectPresentation fileSelect = Resource(FileSelectPresentationFormat.FileName, "file-select presentation", s => FileSelectSourcePresentation.ReadLayout(s));
        EscapeTypewriterPresentation escapeTypewriter = Resource(EscapeTypewriterDocumentDefinitions.FileName, "escape typewriter", s => EscapeTypewriterPresentationFile.Load(s));
        IntroNarrationPresentation introNarration = Resource(IntroNarrationFile.FileName, "intro narration", s => IntroNarrationFile.Read(s));
        EndingTextPresentation endingText = Resource(EndingTextDefinitions.FileName, "ending text", s => EndingTextPresentation.Load(s));
        CreditsPresentation staffCredits = Resource(CreditsPresentationDefinitions.FileName, "staff credits", s => CreditsPresentation.Load(s));
        TitleGradientPresentation titleGradient = Resource(TitleGradientFile.FileName, "title gradient", s => TitleGradientFile.Read(s));
        RoomPaletteFxPresentation roomPaletteFx = Resource(RoomPaletteFxPresentationFormat.FileName, "room palette effects", s => RoomPaletteFxPresentationFile.Read(s));
        InstalledMotherBrainRainbowPalette motherBrainRainbowPalette = Resource(MotherBrainRainbowPaletteFile.FileName, "Mother Brain rainbow palette", s => MotherBrainRainbowPaletteFile.Read(s));
        InstalledPowerBombGlowColors powerBombGlowColors = Resource(PowerBombGlowColorFile.FileName, "Power Bomb glow colours", s => PowerBombGlowColorFile.Read(s));
        CeresRidleyGetawayBoostDelays ceresRidleyGetawayBoostDelays = Resource(CeresRidleyGetawayBoostDelayFormat.FileName, "Ceres Ridley getaway boost delays", s => CeresRidleyGetawayBoostDelayFiles.Read(s));
        return new GameRead(new AreaMapPresentationParts { Areas = areas, ContentIdentity = Convert.ToHexString(identity.GetHashAndReset()), HighlightCycle = cycle.Timing, Backdrops = sceneColours.Menus, Labels = labels, Stations = stations, Landmarks = landmarks, SaveMarkers = saveMarkers, Arrows = arrows, PauseSelectors = pauseSelectors.Presentation, PauseReserveTanks = pauseReserveTanks.Presentation, EscapeTimer = escapeTimer, GameplayHud = gameplayHud, GameOver = gameOver.Presentation, GameOptions = gameOptions.Presentation, FileSelect = fileSelect, EscapeTypewriter = escapeTypewriter, IntroNarration = introNarration, EndingText = endingText.Typewriter(), TitleBackdrop = sceneColours.TitleBackdrop, TitleGradient = titleGradient, EndingSpaceBackdrops = EndingSpaceBackdropColours.From(roomPaletteFx), MotherBrainRainbowPalette = motherBrainRainbowPalette.Beam, CeresHaze = sceneColours.CeresHaze, PowerBombGlowColors = powerBombGlowColors, CeresRidleyGetawayBoostDelays = ceresRidleyGetawayBoostDelays },
            new InstalledPresentationSources(
                staffCredits,
                endingText, screens,
                cycle, pauseSelectors, pauseReserveTanks,
                gameOptions) { GameOver = gameOver }, roomPaletteFx, motherBrainRainbowPalette);

        T Resource<T>(string file, string description, Func<Stream, T> compile) where T : class =>
            MultiFileResource<T>([file], description, streams => compile(streams[0]));

        // Each installed file is read and compiled once, as installed.
        T MultiFileResource<T>(string[] files, string description, Func<Stream[], T> compile) where T : class
        {
            ArraySegment<byte>[] contents = files.Select(installed.Read).ToArray();
            foreach (ArraySegment<byte> bytes in contents) AppendFramed(bytes);
            return Compile(contents, files, installed, description, compile);
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
    /// Compiles installed <paramref name="contents"/> read from <paramref name="files"/>, attributing a failure to the actual
    /// files and retaining the codec error instead of reporting only a directory.
    /// </summary>
    private static T Compile<T>(ArraySegment<byte>[] contents, string[] files, InstalledMapReader installed, string description, Func<Stream[], T> compile)
        where T : class
    {
        try
        {
            return compile(contents.Select(bytes => (Stream)ReadOnlyStream(bytes)).ToArray());
        }
        catch (InvalidDataException error)
        {
            throw new InvalidDataException($"Invalid {description} '{string.Join("', '", files.Select(installed.PathOf))}': {error.Message}", error);
        }
    }

    /// <summary>What the game binds of the map presentation: its parts and the installed documents it was read with.</summary>
    /// <param name="Parts">The parts the game reads.</param>
    /// <param name="Sources">The installed documents the import composes pictures from and the game names sheet frames from.</param>
    /// <param name="RoomPaletteFx">
    /// The palette-effect colours, which only the import pre-colours sheets from; the game keeps only the backdrops of the
    /// space behind the exploding planet taken from them (#1583).
    /// </param>
    /// <param name="MotherBrainRainbow">
    /// Mother Brain's rainbow colours, which only the import pre-colours her pictures from; the game keeps only the beam
    /// backdrop's colours taken from them (#1616).
    /// </param>
    private sealed record GameRead(AreaMapPresentationParts Parts, InstalledPresentationSources Sources, RoomPaletteFxPresentation RoomPaletteFx,
        InstalledMotherBrainRainbowPalette MotherBrainRainbow);

    /// <summary>
    /// A read-only view whose buffer the JSON reader may parse in place. Compilers copy what
    /// they keep, so pooled installed bytes are never referenced after <see cref="Load"/> returns.
    /// </summary>
    private static MemoryStream ReadOnlyStream(ArraySegment<byte> bytes) =>
        new(bytes.Array ?? throw new ArgumentException("An installed resource view must wrap an array.", nameof(bytes)),
            bytes.Offset, bytes.Count, writable: false, publiclyVisible: true);

    /// <summary>
    /// Reads the files the extraction manifest lists, each as extracted: a document from the component's directory and a
    /// character page from its import data (#1636), which only the import's read has.
    /// </summary>
    private sealed class InstalledMapReader : IDisposable
    {
        /// <summary>Root of the installed map presentation files.</summary>
        private readonly string directory;
        /// <summary>The component's import-data directory holding its pages, for the import's read; none for the game's.</summary>
        private readonly string? importData;
        /// <summary>The extraction manifest, which lists every installed file the catalog reads.</summary>
        private readonly AreaMapCatalogManifest manifest;
        /// <summary>Names actually read during this catalog load.</summary>
        private readonly HashSet<string> read = new(StringComparer.Ordinal);
        /// <summary>Buffers retained only for one load and returned to the shared pool on disposal.</summary>
        private readonly List<byte[]> rented = [];

        /// <summary>Starts a manifest-bound read session in the installed directory and, for the import, its import data.</summary>
        private InstalledMapReader(string directory, string? importData, AreaMapCatalogManifest manifest)
        {
            this.directory = directory;
            this.importData = importData;
            this.manifest = manifest;
        }

        /// <summary>
        /// Loads and validates the manifest's version and required resource count; <paramref name="importData"/> is the
        /// component's import-data directory for the import's read, and none for the game's, which reads no page.
        /// </summary>
        public static InstalledMapReader Open(string directory, string? importData)
        {
            AreaMapCatalogManifest manifest;
            try
            {
                manifest = JsonAssetDocument.Read<AreaMapCatalogManifest>(
                    File.ReadAllBytes(Path.Combine(directory, AreaMapCatalogFormat.ManifestFile)), ArtworkJson.Options)
                    ?? throw new InvalidDataException("Map catalog manifest is null.");
            }
            catch (JsonException error) { throw new InvalidDataException($"Invalid map catalog manifest in {directory}.", error); }
            if (manifest.Version != AreaMapCatalogFormat.Version || manifest.Sha256 is null || manifest.Sha256.Count != AreaIds.RetailCount + AreaMapCatalogFormat.SharedResourceCount)
                throw new InvalidDataException($"Map catalog manifest must contain the supported version, seven maps and all {AreaMapCatalogFormat.SharedResourceCount} shared presentation resources.");
            return new InstalledMapReader(directory, importData, manifest);
        }

        /// <summary>The full path of a file, for diagnostics: a page's in the import data, a document's in the directory.</summary>
        public string PathOf(string file) => Path.GetFullPath(Path.Combine(DirectoryOf(file), file));

        /// <summary>
        /// The directory <paramref name="file"/> is read from: the import data for a page (#1636) or an import-only document
        /// (#1638), the component's directory for any other document.
        /// </summary>
        /// <exception cref="InvalidOperationException">The file is import data and this read is the game's, which reads none.</exception>
        private string DirectoryOf(string file) => !PageFiles.Contains(file) && !ImportDocumentFiles.Contains(file) ? directory
            : importData ?? throw new InvalidOperationException($"The game's read of the map presentation reads no import data, but {file} was asked for.");

        /// <summary>Rents a buffer and reads one listed file into it.</summary>
        public ArraySegment<byte> Read(string file)
        {
            if (!manifest.Sha256.ContainsKey(file))
                throw new InvalidDataException($"Map catalog manifest is missing {file}.");
            using var input = File.OpenRead(Path.Combine(DirectoryOf(file), file));
            int length = checked((int)input.Length);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
            rented.Add(buffer);
            input.ReadExactly(buffer.AsSpan(0, length));
            read.Add(file);
            return new ArraySegment<byte>(buffer, 0, length);
        }

        /// <summary>Returns every buffer rented during this load to the shared pool.</summary>
        public void Dispose()
        {
            foreach (byte[] buffer in rented) ArrayPool<byte>.Shared.Return(buffer);
            rented.Clear();
        }

        /// <summary>Validates seven station masks as distinct in-bounds row-major map cells.</summary>
        public Dictionary<AreaId, HashSet<int>> ReadStationCells()
        {
            Dictionary<string, int[]> masks;
            try
            {
                masks = JsonAssetDocument.Read<Dictionary<string, int[]>>(ReadOnlyStream(Read(AreaMapCatalogFormat.StationRevealFile)), ArtworkJson.Options)
                    ?? throw new InvalidDataException("Station reveal content is null.");
            }
            catch (JsonException error) { throw new InvalidDataException("Invalid station reveal content.", error); }
            if (masks.Count != AreaIds.RetailCount) throw new InvalidDataException("Station reveal content requires all seven areas.");
            var stationCells = new Dictionary<AreaId, HashSet<int>>();
            foreach (AreaId area in Enum.GetValues<AreaId>())
            {
                if (!masks.TryGetValue(area.ToString(), out int[]? cells) || cells is null ||
                    cells.Any(cell => (uint)cell >= AreaMapSize.WidthInTiles * AreaMapSize.HeightInTiles) ||
                    cells.Distinct().Count() != cells.Length)
                    throw new InvalidDataException($"Invalid or duplicate station reveal cells for {area}.");
                stationCells.Add(area, cells.ToHashSet());
            }
            return stationCells;
        }

        /// <summary>
        /// Every file the manifest lists is read by the load that uses it, except exactly <paramref name="leftUnread"/>, the
        /// files another reader of the same presentation reads.
        /// </summary>
        public void RequireAllManifestFilesRead(IReadOnlyCollection<string> leftUnread)
        {
            string[] unread = manifest.Sha256.Keys.Where(file => !read.Contains(file) && !leftUnread.Contains(file)).ToArray();
            if (unread.Length > 0)
                throw new InvalidDataException($"Map catalog manifest lists files the catalog never reads: {string.Join(", ", unread)}.");
            string[] missing = leftUnread.Where(file => !manifest.Sha256.ContainsKey(file)).ToArray();
            if (missing.Length > 0)
                throw new InvalidDataException($"Map catalog manifest is missing {string.Join(", ", missing)}.");
        }
    }
}
