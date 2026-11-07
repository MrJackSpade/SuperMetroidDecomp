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
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Fx.Artwork;
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
/// presentation catalog the game binds.
/// </summary>
public static class MapPresentationFiles
{
    /// <summary>Reads all areas atomically into a new catalog; a file that does not decode fails loudly, naming the file.</summary>
    public static AreaMapPresentation Load(string directory)
    {
        using var installed = InstalledMapReader.Open(directory);
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
        MapStaticPalettes palettes = Resource(MapStaticPalettesFormat.FileName, "map palettes", s => MapStaticPalettesFile.Read(s));
        WorldMapLabelPositions labels = Resource(WorldMapLabelFormat.FileName, "world-map labels", s => WorldMapLabelFile.Read(s));
        MapStationPositions stations = Resource(MapStationLayoutFormat.FileName, "map station labels", s => MapStationLayoutFile.Read(s));
        MapLandmarkPositions landmarks = Resource(MapLandmarkFormat.FileName, "map landmarks", s => MapLandmarkFile.Read(s));
        MapSaveMarkerPositions saveMarkers = Resource(MapSaveMarkerFormat.FileName, "save markers", s => MapSaveMarkerFile.Read(s));
        MapArrowPresentation arrows = Resource(MapArrowFormat.FileName, "map arrows", s => MapArrowFile.Read(s));
        MapScreenPresentation screens = Resource(MapScreenDefinitions.FileName, "map screens", s => MapScreenPresentation.Load(s));
        WorldMapArtwork artwork = MultiFileResource<WorldMapArtwork>([WorldMapArtworkFormat.ForegroundFile, WorldMapArtworkFormat.BackgroundFile],
            "world-map PNG artwork", s => WorldMapCharacters.Read(s[0], s[1]));
        MapSpriteCompositions sprites = MultiFileResource<MapSpriteCompositions>([MapSpriteFormat.JsonFile, MapSpriteFormat.PngFile],
            "map sprites", s => MapSpriteArtwork.Read(s[0], s[1]));
        MapTileCharacters pauseTiles = Resource(PauseTileAtlasFormat.FileName, "pause artwork", s => MapTileCharacters.Read(s));
        PauseBackdropPresentation pauseBackdrops = Resource(PauseBackdropDefinitions.FileName, "pause backdrop", s => PauseBackdropPresentation.Load(s));
        PauseSelectorPresentation pauseSelectors = Resource(PauseSelectorFormat.FileName, "pause selectors", s => PauseSelectorSourcePresentation.Load(s));
        PauseWireframePresentation pauseWireframes = Resource(PauseWireframeDefinitions.FileName, "pause wireframes", s => PauseWireframePresentation.Load(s));
        PauseReserveTankPresentation pauseReserveTanks = Resource(PauseReserveTankFormat.FileName, "reserve tank presentation", s => PauseReserveTankSourcePresentation.Load(s));
        PauseReserveUiPresentation pauseReserveUi = Resource(PauseReserveUiDefinitions.FileName, "reserve UI presentation", s => PauseReserveUiFile.Read(s));
        PauseEquipmentBasePresentation pauseEquipmentBase = Resource(PauseEquipmentBaseDefinitions.FileName, "pause equipment base", s => PauseEquipmentBasePresentation.Load(s));
        PauseEquipmentLabelPresentation pauseEquipmentLabels = Resource(PauseEquipmentLabelArtworkDefinitions.FileName, "pause equipment labels", s => PauseEquipmentLabelPresentation.Load(s));
        EscapeTimerPresentation escapeTimer = Resource(EscapeTimerLayoutDefinitions.FileName, "escape timer presentation", s => EscapeTimerPresentationFile.Load(s));
        EscapeTimerCharacters escapeTimerTiles = Resource(EscapeTimerTileAtlasFormat.FileName, "escape timer artwork", s => EscapeTimerCharacters.Read(s));
        GameplayHudPresentation gameplayHud = Resource(GameplayHudDocumentDefinitions.FileName, "gameplay HUD presentation", s => GameplayHudPresentationFile.Read(s));
        GameOverPresentation gameOver = Resource(GameOverPresentationFormat.FileName, "game-over presentation", s => GameOverSourcePresentation.Load(s));
        GameOptionsPresentation gameOptions = Resource(GameOptionsPresentationFormat.FileName, "options-menu presentation", s => GameOptionsSourcePresentation.Load(s));
        FileSelectPresentation fileSelect = Resource(FileSelectPresentationFormat.FileName, "file-select presentation", s => FileSelectSourcePresentation.Load(s));
        GameplayMessageTitlePresentation gameplayMessageTitles = Resource(GameplayMessageTitleDefinitions.FileName, "gameplay-message titles", s => GameplayMessageTitlePresentation.Load(s));
        GameplayMessagePanelPresentation gameplayMessagePanels = Resource(GameplayMessagePanelDefinitions.FileName, "gameplay-message panels", s => GameplayMessagePanelPresentation.Load(s));
        GameplayMessageNoticePresentation gameplayMessageNotices = Resource(GameplayMessageNoticeDefinitions.FileName, "gameplay-message notices", s => GameplayMessageNoticePresentation.Load(s));
        EscapeTypewriterPresentation escapeTypewriter = Resource(EscapeTypewriterDocumentDefinitions.FileName, "escape typewriter", s => EscapeTypewriterPresentationFile.Load(s));
        IntroNarrationPresentation introNarration = Resource(IntroNarrationFile.FileName, "intro narration", s => IntroNarrationFile.Read(s));
        IntroFontAtlas introFont = Resource(IntroFontCharacters.FileName, "intro font", s => IntroFontCharacters.Read(s));
        EndingTextPresentation endingText = Resource(EndingTextDefinitions.FileName, "ending text", s => EndingTextPresentation.Load(s));
        EndingFontAtlas endingFont = Resource(EndingFontAtlasFormat.FileName, "ending font", s => EndingFontCharacters.Read(s));
        CreditsPresentation staffCredits = Resource(CreditsPresentationDefinitions.FileName, "staff credits", s => CreditsPresentation.Load(s));
        TitleGraphicsPresentation titleGraphics = MultiFileResource<TitleGraphicsPresentation>(
            [TitleGraphicsFormat.FloorTilesFile, TitleGraphicsFormat.FloorMapFile, TitleGraphicsFormat.ObjectTilesFile, TitleGraphicsFormat.BabyTilesFile],
            "title graphics", s => TitleGraphicsCharacters.Read(s[0], s[1], s[2], s[3]));
        TitlePalettePresentation titlePalette = Resource(TitlePaletteFormat.FileName, "title palette", s => TitlePaletteFile.Read(s));
        TitleGradientPresentation titleGradient = Resource(TitleGradientFile.FileName, "title gradient", s => TitleGradientFile.Read(s));
        RoomPaletteFxPresentation roomPaletteFx = Resource(RoomPaletteFxPresentationFormat.FileName, "room palette effects", s => RoomPaletteFxPresentationFile.Read(s));
        MotherBrainHealthPalettePresentation motherBrainHealthPalette = Resource(MotherBrainHealthPaletteFile.FileName, "Mother Brain health palette", s => MotherBrainHealthPaletteFile.Read(s));
        MotherBrainRainbowPalettePresentation motherBrainRainbowPalette = Resource(MotherBrainRainbowPaletteFile.FileName, "Mother Brain rainbow palette", s => MotherBrainRainbowPaletteFile.Read(s));
        MotherBrainRoomColorPresentation motherBrainRoomColors = Resource(MotherBrainRoomColorFormat.FileName, "Mother Brain room colors", s => MotherBrainRoomColorFile.Read(s));
        RoomFxAnimatedTileAtlas roomFxAnimatedTiles = Resource(RoomFxAnimatedTileAtlasFile.FileName, "room-FX presentation", s => RoomFxAnimatedTileAtlasPng.Read(s));
        RoomFxLayer3Pages roomFxLayer3Tilemaps = Resource(RoomFxLayer3TilemapFormat.FileName, "room-FX presentation", s => RoomFxLayer3Pages.Load(s));
        RoomFxBlendInks roomFxPaletteBlends = Resource(RoomFxPaletteBlendDefinitions.FileName, "room-FX presentation", s => RoomFxPaletteBlendFile.Read(s));
        InstalledPowerBombFixedColors powerBombFixedColors = Resource(PowerBombFixedColorFile.FileName, "Power Bomb fixed colors", s => PowerBombFixedColorFile.Read(s));
        InstalledSamusVisorColors samusVisorColors = Resource(SamusVisorColorFile.FileName, "Samus visor colors", s => SamusVisorColorFile.Read(s));
        InstalledSamusHurtColors samusHurtColors = Resource(SamusHurtColorFile.FileName, "Samus hurt colors", s => SamusHurtColorFile.Read(s));
        InstalledSamusSuitColors samusSuitColors = Resource(SamusSuitColorFile.FileName, "Samus suit colors", s => SamusSuitColorFile.Read(s));
        SamusFullBodyCycleColours samusFullBodyCycleColors = Resource(SamusFullBodyCycleColorFormat.FileName, "Samus full-body cycle colors", s => SamusFullBodyCycleColorFile.Read(s).ToColours());
        InstalledCrystalFlashColors crystalFlashColors = Resource(CrystalFlashColorFile.FileName, "Crystal Flash colors", s => CrystalFlashColorFile.Read(s));
        SamusChargeColours samusChargeColors = Resource(SamusChargeColorFormat.FileName, "Samus charge colors", s => SamusChargeColorFile.Read(s).ToColours());
        CeresRidleyColorCatalog ceresRidleyColors = Resource(CeresRidleyColorFormat.FileName, "Ceres Ridley colors", s => CeresRidleyColorFile.Read(s));
        CeresRidleyFloorColorCatalog ceresRidleyFloorColors = Resource(CeresRidleyFloorColorFormat.FileName, "Ceres Ridley Mode-7 colors", s => CeresRidleyMode7ColorFile.Read(s));
        CeresRidleyGetawayBoostDelays ceresRidleyGetawayBoostDelays = Resource(CeresRidleyGetawayBoostDelayFormat.FileName, "Ceres Ridley getaway boost delays", s => CeresRidleyGetawayBoostDelayFiles.Read(s));
        SamusHyperBeamColours samusHyperBeamColors = Resource(SamusHyperBeamColorFormat.FileName, "Samus Hyper Beam colors", s => SamusHyperBeamColorFile.Read(s).ToColours());
        installed.RequireAllManifestFilesRead();
        var catalog = new InstalledAreaMapPresentation(new AreaMapPresentationParts { Areas = areas, ContentIdentity = Convert.ToHexString(identity.GetHashAndReset()), HighlightCycle = cycle.Timing, Backdrops = palettes.Backdrops(), Labels = labels, Stations = stations, Landmarks = landmarks, SaveMarkers = saveMarkers, Arrows = arrows, WorldArtwork = artwork, PauseSelectors = pauseSelectors, PauseReserveTanks = pauseReserveTanks, EscapeTimer = escapeTimer, GameplayHud = gameplayHud, GameOver = gameOver, GameOptions = gameOptions, FileSelect = fileSelect, EscapeTypewriter = escapeTypewriter, IntroNarration = introNarration, IntroFont = introFont, EndingText = endingText.Typewriter(), EndingFont = endingFont, TitleGraphics = titleGraphics, TitleBackdrop = titlePalette.Backdrop, TitleGradient = titleGradient, RoomPaletteFx = roomPaletteFx, MotherBrainHealthPalette = motherBrainHealthPalette, MotherBrainRainbowPalette = motherBrainRainbowPalette, MotherBrainRoomColors = motherBrainRoomColors, RoomFxPaletteBlends = roomFxPaletteBlends, PowerBombFixedColors = powerBombFixedColors, CeresRidleyGetawayBoostDelays = ceresRidleyGetawayBoostDelays },
            new InstalledPresentationSources(new MapCharacterArtwork(tiles, hudTiles, pauseTiles),
                new GameplayMessageArtwork(gameplayMessageTitles, gameplayMessagePanels, gameplayMessageNotices), roomFxLayer3Tilemaps, staffCredits,
                endingText, screens, new PauseScreenArtwork(pauseBackdrops, pauseWireframes, pauseEquipmentBase, pauseEquipmentLabels, pauseReserveUi), sprites,
                new SamusMapColourTables(samusVisorColors, samusHurtColors, samusSuitColors, samusFullBodyCycleColors, samusChargeColors, samusHyperBeamColors, crystalFlashColors),
                titlePalette, cycle, palettes, ceresRidleyColors, ceresRidleyFloorColors, escapeTimerTiles, roomFxAnimatedTiles));
        return catalog;

        T Resource<T>(string file, string description, Func<Stream, T> compile) where T : class =>
            MultiFileResource<T>([file], description, streams => compile(streams[0]));

        // Each installed file is read and compiled once, as installed.
        T MultiFileResource<T>(string[] files, string description, Func<Stream[], T> compile) where T : class
        {
            ArraySegment<byte>[] contents = files.Select(installed.Read).ToArray();
            foreach (ArraySegment<byte> bytes in contents) AppendFramed(bytes);
            try
            {
                return compile(contents.Select(bytes => (Stream)ReadOnlyStream(bytes)).ToArray());
            }
            catch (InvalidDataException error)
            {
                // Attribute the failure to the actual files, retaining the codec error instead of reporting only a directory.
                throw new InvalidDataException($"Invalid {description} '{string.Join("', '", files.Select(installed.PathOf))}': {error.Message}", error);
            }
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

        /// <summary>Every file the manifest lists is read by the load that uses it.</summary>
        public void RequireAllManifestFilesRead()
        {
            string[] unread = manifest.Sha256.Keys.Where(file => !read.Contains(file)).ToArray();
            if (unread.Length > 0)
                throw new InvalidDataException($"Map catalog manifest lists files the catalog never reads: {string.Join(", ", unread)}.");
        }
    }
}
