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
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
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
/// import pre-colours sheets from are read by the import's read (<see cref="LoadForImport"/>) alone.
/// </summary>
public static class MapPresentationFiles
{
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
        using var installed = InstalledMapReader.Open(directory);
        GameRead game = ReadGame(installed);
        installed.RequireAllManifestFilesRead(ImportColourFiles);
        return new InstalledAreaMapPresentation(new AreaMapPresentation(game.Parts), game.Sources);
    }

    /// <summary>
    /// Reads all areas atomically into the catalog the import composes from: the game's catalog with the colours only the
    /// import pre-colours sheets and pictures from (#1544). A file that does not decode fails loudly, naming the file.
    /// </summary>
    public static ImportAreaMapPresentation LoadForImport(string directory)
    {
        using var installed = InstalledMapReader.Open(directory);
        GameRead game = ReadGame(installed);
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
            game.RoomPaletteFx);
        installed.RequireAllManifestFilesRead([]);
        return new ImportAreaMapPresentation(new AreaMapPresentation(game.Parts), game.Sources, colours);

        // The import's colours are not part of the presentation's identity: the game reads none of them, and the sheets
        // composed from them carry their own.
        T Colours<T>(string file, string description, Func<Stream, T> compile) where T : class =>
            Compile<T>([installed.Read(file)], [file], installed, description, streams => compile(streams[0]));
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
        // Each resource below is framed into the identity in this fixed order.
        MapTileCharacters tiles = Resource(MapTileAtlasFormat.FileName, "map tile atlas", s => MapTileCharacters.Read(s));
        HudTileAtlas hudTiles = Resource(HudTileAtlasFormat.FileName, "HUD tile atlas", s => HudTileAtlasPng.Read(s));
        MapPaletteCycle cycle = Resource(MapPaletteCycleFile.FileName, "map highlight cycle", s => MapPaletteCycleFile.Read(s));
        SceneColours sceneColours = Resource(SceneColourFiles.FileName, "scene colours", s => SceneColourFiles.Read(s));
        WorldMapLabelPositions labels = Resource(WorldMapLabelFormat.FileName, "world-map labels", s => WorldMapLabelFile.Read(s));
        MapStationPositions stations = Resource(MapStationLayoutFormat.FileName, "map station labels", s => MapStationLayoutFile.Read(s));
        MapLandmarkPositions landmarks = Resource(MapLandmarkFormat.FileName, "map landmarks", s => MapLandmarkFile.Read(s));
        MapSaveMarkerPositions saveMarkers = Resource(MapSaveMarkerFormat.FileName, "save markers", s => MapSaveMarkerFile.Read(s));
        MapArrowPresentation arrows = Resource(MapArrowFormat.FileName, "map arrows", s => MapArrowFile.Read(s));
        MapScreenPresentation screens = Resource(MapScreenDefinitions.FileName, "map screens", s => MapScreenPresentation.Load(s));
        WorldMapCharacters worldCharacters = MultiFileResource<WorldMapCharacters>([WorldMapArtworkFormat.ForegroundFile, WorldMapArtworkFormat.BackgroundFile],
            "world-map PNG artwork", s => WorldMapCharacters.Read(s[0], s[1]));
        MapSpriteCompositions sprites = MultiFileResource<MapSpriteCompositions>([MapSpriteFormat.JsonFile, MapSpriteFormat.PngFile],
            "map sprites", s => MapSpriteArtwork.Read(s[0], s[1]));
        MapTileCharacters pauseTiles = Resource(PauseTileAtlasFormat.FileName, "pause artwork", s => MapTileCharacters.Read(s));
        PauseBackdropPresentation pauseBackdrops = Resource(PauseBackdropDefinitions.FileName, "pause backdrop", s => PauseBackdropPresentation.Load(s));
        PauseSelectorSourcePresentation pauseSelectors = Resource(PauseSelectorFormat.FileName, "pause selectors", s => PauseSelectorSourcePresentation.Load(s));
        PauseWireframePresentation pauseWireframes = Resource(PauseWireframeDefinitions.FileName, "pause wireframes", s => PauseWireframePresentation.Load(s));
        PauseReserveTankSourcePresentation pauseReserveTanks = Resource(PauseReserveTankFormat.FileName, "reserve tank presentation", s => PauseReserveTankSourcePresentation.Load(s));
        PauseReserveUiPresentation pauseReserveUi = Resource(PauseReserveUiDefinitions.FileName, "reserve UI presentation", s => PauseReserveUiFile.Read(s));
        PauseEquipmentBasePresentation pauseEquipmentBase = Resource(PauseEquipmentBaseDefinitions.FileName, "pause equipment base", s => PauseEquipmentBasePresentation.Load(s));
        PauseEquipmentLabelPresentation pauseEquipmentLabels = Resource(PauseEquipmentLabelArtworkDefinitions.FileName, "pause equipment labels", s => PauseEquipmentLabelPresentation.Load(s));
        EscapeTimerPresentation escapeTimer = Resource(EscapeTimerLayoutDefinitions.FileName, "escape timer presentation", s => EscapeTimerPresentationFile.Load(s));
        EscapeTimerCharacters escapeTimerTiles = Resource(EscapeTimerTileAtlasFormat.FileName, "escape timer artwork", s => EscapeTimerCharacters.Read(s));
        GameplayHudArtwork gameplayHud = Resource(GameplayHudDocumentDefinitions.FileName, "gameplay HUD presentation", s => GameplayHudPresentationFile.Read(s));
        GameOverSourcePresentation gameOver = Resource(GameOverPresentationFormat.FileName, "game-over presentation", s => GameOverSourcePresentation.Load(s));
        GameOptionsSourcePresentation gameOptions = Resource(GameOptionsPresentationFormat.FileName, "options-menu presentation", s => GameOptionsSourcePresentation.Load(s));
        FileSelectSourcePresentation fileSelect = Resource(FileSelectPresentationFormat.FileName, "file-select presentation", s => FileSelectSourcePresentation.Load(s));
        GameplayMessageTitlePresentation gameplayMessageTitles = Resource(GameplayMessageTitleDefinitions.FileName, "gameplay-message titles", s => GameplayMessageTitlePresentation.Load(s));
        GameplayMessagePanelPresentation gameplayMessagePanels = Resource(GameplayMessagePanelDefinitions.FileName, "gameplay-message panels", s => GameplayMessagePanelPresentation.Load(s));
        GameplayMessageNoticePresentation gameplayMessageNotices = Resource(GameplayMessageNoticeDefinitions.FileName, "gameplay-message notices", s => GameplayMessageNoticePresentation.Load(s));
        EscapeTypewriterPresentation escapeTypewriter = Resource(EscapeTypewriterDocumentDefinitions.FileName, "escape typewriter", s => EscapeTypewriterPresentationFile.Load(s));
        IntroNarrationPresentation introNarration = Resource(IntroNarrationFile.FileName, "intro narration", s => IntroNarrationFile.Read(s));
        IntroFontCharacters introFont = Resource(IntroFontCharacters.FileName, "intro font", s => IntroFontCharacters.Read(s));
        EndingTextPresentation endingText = Resource(EndingTextDefinitions.FileName, "ending text", s => EndingTextPresentation.Load(s));
        EndingFontCharacters endingFont = Resource(EndingFontAtlasFormat.FileName, "ending font", s => EndingFontCharacters.Read(s));
        CreditsPresentation staffCredits = Resource(CreditsPresentationDefinitions.FileName, "staff credits", s => CreditsPresentation.Load(s));
        TitleGraphicsCharacters titleGraphics = MultiFileResource<TitleGraphicsCharacters>(
            [TitleGraphicsFormat.FloorTilesFile, TitleGraphicsFormat.FloorMapFile, TitleGraphicsFormat.ObjectTilesFile, TitleGraphicsFormat.BabyTilesFile],
            "title graphics", s => TitleGraphicsCharacters.Read(s[0], s[1], s[2], s[3]));
        TitleGradientPresentation titleGradient = Resource(TitleGradientFile.FileName, "title gradient", s => TitleGradientFile.Read(s));
        RoomPaletteFxPresentation roomPaletteFx = Resource(RoomPaletteFxPresentationFormat.FileName, "room palette effects", s => RoomPaletteFxPresentationFile.Read(s));
        MotherBrainRainbowPalettePresentation motherBrainRainbowPalette = Resource(MotherBrainRainbowPaletteFile.FileName, "Mother Brain rainbow palette", s => MotherBrainRainbowPaletteFile.Read(s));
        RoomFxAnimatedTileAtlas roomFxAnimatedTiles = Resource(RoomFxAnimatedTileAtlasFile.FileName, "room-FX presentation", s => RoomFxAnimatedTileAtlasPng.Read(s));
        RoomFxLayer3Pages roomFxLayer3Tilemaps = Resource(RoomFxLayer3TilemapFormat.FileName, "room-FX presentation", s => RoomFxLayer3Pages.Load(s));
        InstalledPowerBombFixedColors powerBombFixedColors = Resource(PowerBombFixedColorFile.FileName, "Power Bomb fixed colors", s => PowerBombFixedColorFile.Read(s));
        CeresRidleyGetawayBoostDelays ceresRidleyGetawayBoostDelays = Resource(CeresRidleyGetawayBoostDelayFormat.FileName, "Ceres Ridley getaway boost delays", s => CeresRidleyGetawayBoostDelayFiles.Read(s));
        return new GameRead(new AreaMapPresentationParts { Areas = areas, ContentIdentity = Convert.ToHexString(identity.GetHashAndReset()), HighlightCycle = cycle.Timing, Backdrops = sceneColours.Menus, Labels = labels, Stations = stations, Landmarks = landmarks, SaveMarkers = saveMarkers, Arrows = arrows, PauseSelectors = pauseSelectors.Presentation, PauseReserveTanks = pauseReserveTanks.Presentation, EscapeTimer = escapeTimer, GameplayHud = gameplayHud.Presentation, GameOver = gameOver.Presentation, GameOptions = gameOptions.Presentation, FileSelect = fileSelect.Presentation, EscapeTypewriter = escapeTypewriter, IntroNarration = introNarration, EndingText = endingText.Typewriter(), TitleBackdrop = sceneColours.TitleBackdrop, TitleGradient = titleGradient, EndingSpaceBackdrops = EndingSpaceBackdropColours.From(roomPaletteFx), MotherBrainRainbowPalette = motherBrainRainbowPalette, CeresHaze = sceneColours.CeresHaze, PowerBombFixedColors = powerBombFixedColors, CeresRidleyGetawayBoostDelays = ceresRidleyGetawayBoostDelays },
            new InstalledPresentationSources(new MapCharacterArtwork(tiles, hudTiles, pauseTiles),
                new GameplayMessageArtwork(gameplayMessageTitles, gameplayMessagePanels, gameplayMessageNotices), roomFxLayer3Tilemaps, staffCredits,
                endingText, screens, new PauseScreenArtwork(pauseBackdrops, pauseWireframes, pauseEquipmentBase, pauseEquipmentLabels, pauseReserveUi), sprites,
                cycle, escapeTimerTiles, roomFxAnimatedTiles, worldCharacters, introFont, endingFont, titleGraphics, pauseSelectors, pauseReserveTanks,
                gameplayHud, gameOptions, fileSelect) { GameOver = gameOver }, roomPaletteFx);

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

    /// <summary>What the game binds of the map presentation: its parts and the source artwork it was read with.</summary>
    /// <param name="Parts">The parts the game reads.</param>
    /// <param name="Sources">The source artwork only the import composes pictures from.</param>
    /// <param name="RoomPaletteFx">
    /// The palette-effect colours, which only the import pre-colours sheets from; the game keeps only the backdrops of the
    /// space behind the exploding planet taken from them (#1583).
    /// </param>
    private sealed record GameRead(AreaMapPresentationParts Parts, InstalledPresentationSources Sources, RoomPaletteFxPresentation RoomPaletteFx);

    /// <summary>
    /// A read-only view whose buffer the JSON reader may parse in place. Compilers copy what
    /// they keep, so pooled installed bytes are never referenced after <see cref="Load"/> returns.
    /// </summary>
    private static MemoryStream ReadOnlyStream(ArraySegment<byte> bytes) =>
        new(bytes.Array ?? throw new ArgumentException("An installed resource view must wrap an array.", nameof(bytes)),
            bytes.Offset, bytes.Count, writable: false, publiclyVisible: true);

    /// <summary>Reads the installed files the extraction manifest lists, each as installed.</summary>
    private sealed class InstalledMapReader : IDisposable
    {
        /// <summary>Root of the installed map presentation files.</summary>
        private readonly string directory;
        /// <summary>The extraction manifest, which lists every installed file the catalog reads.</summary>
        private readonly AreaMapCatalogManifest manifest;
        /// <summary>Names actually read during this catalog load.</summary>
        private readonly HashSet<string> read = new(StringComparer.Ordinal);
        /// <summary>Buffers retained only for one load and returned to the shared pool on disposal.</summary>
        private readonly List<byte[]> rented = [];

        /// <summary>Starts a manifest-bound read session in the installed directory.</summary>
        private InstalledMapReader(string directory, AreaMapCatalogManifest manifest)
        {
            this.directory = directory;
            this.manifest = manifest;
        }

        /// <summary>Loads and validates the manifest's version and required resource count.</summary>
        public static InstalledMapReader Open(string directory)
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
            return new InstalledMapReader(directory, manifest);
        }

        /// <summary>The full path of an installed file, for diagnostics.</summary>
        public string PathOf(string file) => Path.GetFullPath(Path.Combine(directory, file));

        /// <summary>Rents a buffer and reads one listed installed file into it.</summary>
        public ArraySegment<byte> Read(string file)
        {
            if (!manifest.Sha256.ContainsKey(file))
                throw new InvalidDataException($"Map catalog manifest is missing {file}.");
            using var input = File.OpenRead(Path.Combine(directory, file));
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
