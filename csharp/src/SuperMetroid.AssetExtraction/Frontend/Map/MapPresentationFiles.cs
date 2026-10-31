using System.Buffers.Binary;
using System.Buffers;
using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Bosses.Ridley;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Pause;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
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
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// Reads the installed map and menu presentation (#1409): the stock files their manifest verifies and the player's
/// overrides, compiled into the presentation catalog the game binds. Overrides never alter stock provenance or
/// exploration rules.
/// </summary>
public static class MapPresentationFiles
{
    /// <summary>Reads all areas atomically into a new catalog; an invalid override is never replaced with stock.</summary>
    public static AreaMapPresentation Load(string stockDirectory, string? overrideDirectory)
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
            AreaMapPresentationAsset baseline = AreaMapPresentationFile.Read(baselineStream, new AreaMapDecodeRules(area));
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
                    areas[AreaIds.ToIndex(area)] = AreaMapPresentationFile.Read(stream, definition);
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
        MapTileCharacters tiles = Resource(MapTileAtlasFormat.FileName, "map tile atlas", s => MapTileCharacters.Read(s));
        HudTileAtlas hudTiles = Resource(HudTileAtlasFormat.FileName, "HUD tile atlas", s => HudTileAtlasPng.Read(s));
        MapPaletteCycle cycle = Resource(MapPaletteCycleFormat.FileName, "map highlight cycle", s => MapPaletteCycleFile.Read(s));
        MapStaticPalettes palettes = Resource(MapStaticPalettesFormat.FileName, "map palettes", s => MapStaticPalettesFile.Read(s));
        WorldMapLabelPositions labels = Resource(WorldMapLabelFormat.FileName, "world-map labels", s => WorldMapLabelPositions.Load(s));
        MapStationPositions stations = Resource(MapStationLayoutFormat.FileName, "map station labels", s => MapStationPositions.Load(s));
        MapLandmarkPositions landmarks = Resource(MapLandmarkFormat.FileName, "map landmarks", s => MapLandmarkPositions.Load(s));
        MapSaveMarkerPositions saveMarkers = Resource(MapSaveMarkerFormat.FileName, "save markers", s => MapSaveMarkerPositions.Load(s));
        MapArrowPresentation arrows = Resource(MapArrowFormat.FileName, "map arrows", s => MapArrowPresentation.Load(s));
        MapScreenPresentation screens = Resource(MapScreenDefinitions.FileName, "map screens", s => MapScreenPresentation.Load(s));
        WorldMapArtwork artwork = MultiFileResource<WorldMapArtwork>([WorldMapArtworkFormat.ForegroundFile, WorldMapArtworkFormat.BackgroundFile],
            "world-map PNG artwork", (s, _) => WorldMapCharacters.Read(s[0], s[1]));
        MapSpriteCompositions sprites = MultiFileResource<MapSpriteCompositions>([MapSpriteFormat.JsonFile, MapSpriteFormat.PngFile],
            "map sprites", (s, _) => MapSpriteArtwork.Read(s[0], s[1]));
        MapTileCharacters pauseTiles = Resource(PauseTileAtlasFormat.FileName, "pause artwork", s => MapTileCharacters.Read(s));
        PauseBackdropPresentation pauseBackdrops = Resource(PauseBackdropDefinitions.FileName, "pause backdrop", s => PauseBackdropPresentation.Load(s));
        PauseSelectorPresentation pauseSelectors = Resource(PauseSelectorDefinitions.FileName, "pause selectors", s => PauseSelectorSourcePresentation.Load(s));
        PauseWireframePresentation pauseWireframes = Resource(PauseWireframeDefinitions.FileName, "pause wireframes", s => PauseWireframePresentation.Load(s));
        PauseReserveTankPresentation pauseReserveTanks = Resource(PauseReserveTankDefinitions.FileName, "reserve tank presentation", s => PauseReserveTankSourcePresentation.Load(s));
        PauseReserveUiPresentation pauseReserveUi = Resource(PauseReserveUiDefinitions.FileName, "reserve UI presentation", s => PauseReserveUiFile.Read(s));
        PauseEquipmentBasePresentation pauseEquipmentBase = Resource(PauseEquipmentBaseDefinitions.FileName, "pause equipment base", s => PauseEquipmentBasePresentation.Load(s));
        PauseEquipmentLabelPresentation pauseEquipmentLabels = Resource(PauseEquipmentLabelArtworkDefinitions.FileName, "pause equipment labels", s => PauseEquipmentLabelPresentation.Load(s));
        EscapeTimerPresentation escapeTimer = Resource(EscapeTimerPresentationDefinitions.FileName, "escape timer presentation", s => EscapeTimerPresentation.Load(s));
        EscapeTimerTileAtlas escapeTimerTiles = Resource(EscapeTimerTileAtlasFormat.FileName, "escape timer artwork", s => EscapeTimerCharacters.Read(s));
        GameplayHudPresentation gameplayHud = Resource(GameplayHudDefinitions.FileName, "gameplay HUD presentation", s => GameplayHudPresentationFile.Read(s));
        GameOverPresentation gameOver = Resource(GameOverPresentationDefinitions.FileName, "game-over presentation", s => GameOverSourcePresentation.Load(s));
        GameOptionsPresentation gameOptions = Resource(GameOptionsPresentationDefinitions.FileName, "options-menu presentation", s => GameOptionsSourcePresentation.Load(s));
        FileSelectPresentation fileSelect = Resource(FileSelectPresentationDefinitions.FileName, "file-select presentation", s => FileSelectSourcePresentation.Load(s));
        GameplayMessageTitlePresentation gameplayMessageTitles = Resource(GameplayMessageTitleDefinitions.FileName, "gameplay-message titles", s => GameplayMessageTitlePresentation.Load(s));
        GameplayMessagePanelPresentation gameplayMessagePanels = Resource(GameplayMessagePanelDefinitions.FileName, "gameplay-message panels", s => GameplayMessagePanelPresentation.Load(s));
        GameplayMessageNoticePresentation gameplayMessageNotices = Resource(GameplayMessageNoticeDefinitions.FileName, "gameplay-message notices", s => GameplayMessageNoticePresentation.Load(s));
        EscapeTypewriterPresentation escapeTypewriter = Resource(EscapeTypewriterDefinitions.FileName, "escape typewriter", s => EscapeTypewriterPresentation.Load(s));
        IntroNarrationPresentation introNarration = Resource(IntroNarrationDefinitions.FileName, "intro narration", s => IntroNarrationPresentation.Load(s));
        IntroFontAtlas introFont = Resource(IntroFontAtlasFormat.FileName, "intro font", s => IntroFontCharacters.Read(s));
        EndingTextPresentation endingText = Resource(EndingTextDefinitions.FileName, "ending text", s => EndingTextPresentation.Load(s));
        EndingFontAtlas endingFont = Resource(EndingFontAtlasFormat.FileName, "ending font", s => EndingFontCharacters.Read(s));
        CreditsPresentation staffCredits = Resource(CreditsPresentationDefinitions.FileName, "staff credits", s => CreditsPresentation.Load(s));
        TitleGraphicsPresentation titleGraphics = MultiFileResource<TitleGraphicsPresentation>(
            [TitleGraphicsFormat.FloorTilesFile, TitleGraphicsFormat.FloorMapFile, TitleGraphicsFormat.ObjectTilesFile, TitleGraphicsFormat.BabyTilesFile],
            "title graphics", (s, _) => TitleGraphicsCharacters.Read(s[0], s[1], s[2], s[3]));
        TitlePalettePresentation titlePalette = Resource(TitlePaletteFormat.FileName, "title palette", s => TitlePaletteFile.Read(s));
        TitleGradientPresentation titleGradient = Resource(TitleGradientFormat.FileName, "title gradient", s => TitleGradientPresentation.Load(s));
        // Version-seventeen overrides predate the Samus-in-heat rows. Preserve
        // their edits while inheriting only those new rows from current stock.
        RoomPaletteFxPresentation roomPaletteFx = MigratingResource<RoomPaletteFxPresentation>(RoomPaletteFxPresentationFormat.FileName,
            "room palette effects", (s, current) => RoomPaletteFxPresentationFile.Read(s, current), StockFingerprint.StockAndSelected);
        MotherBrainHealthPalettePresentation motherBrainHealthPalette = Resource(MotherBrainHealthPaletteFormat.FileName, "Mother Brain health palette", s => MotherBrainHealthPaletteFile.Read(s));
        // A version-two user override predates the fake-death rows. Preserve its edits
        // and fill only that new color family from verified, current stock content.
        MotherBrainRainbowPalettePresentation motherBrainRainbowPalette = MigratingResource<MotherBrainRainbowPalettePresentation>(MotherBrainRainbowPaletteFormat.FileName,
            "Mother Brain rainbow palette", (s, current) => MotherBrainRainbowPaletteFile.Read(s, current), StockFingerprint.StockAndSelected);
        MotherBrainRoomColorPresentation motherBrainRoomColors = MigratingResource<MotherBrainRoomColorPresentation>(MotherBrainRoomColorFormat.FileName,
            "Mother Brain room colors", (s, current) => MotherBrainRoomColorFile.Read(s, current), StockFingerprint.StockAndSelected);
        RoomFxAnimatedTileAtlas roomFxAnimatedTiles = MigratingResource<RoomFxAnimatedTileAtlas>(RoomFxAnimatedTileAtlasFormat.FileName,
            "room-FX presentation", (s, current) => RoomFxAnimatedTileAtlasPng.Read(s, current), StockFingerprint.SelectedOnly);
        RoomFxLayer3Pages roomFxLayer3Tilemaps = Resource(RoomFxLayer3TilemapFormat.FileName, "room-FX presentation", s => RoomFxLayer3Pages.Load(s));
        RoomFxBlendInks roomFxPaletteBlends = Resource(RoomFxPaletteBlendDefinitions.FileName, "room-FX presentation", s => RoomFxPaletteBlendFile.Read(s));
        InstalledPowerBombFixedColors powerBombFixedColors = Resource(PowerBombFixedColorFormat.FileName, "Power Bomb fixed colors", s => PowerBombFixedColorFile.Read(s));
        InstalledSamusVisorColors samusVisorColors = Resource(SamusVisorColorFormat.FileName, "Samus visor colors", s => SamusVisorColorFile.Read(s));
        InstalledSamusHurtColors samusHurtColors = Resource(SamusHurtColorFormat.FileName, "Samus hurt colors", s => SamusHurtColorFile.Read(s));
        InstalledSamusSuitColors samusSuitColors = Resource(SamusSuitColorFormat.FileName, "Samus suit colors", s => SamusSuitColorFile.Read(s));
        SamusFullBodyCycleColours samusFullBodyCycleColors = Resource(SamusFullBodyCycleColorFormat.FileName, "Samus full-body cycle colors", s => SamusFullBodyCycleColorFile.Read(s).ToColours());
        InstalledCrystalFlashColors crystalFlashColors = Resource(CrystalFlashColorFormat.FileName, "Crystal Flash colors", s => CrystalFlashColorFile.Read(s));
        SamusChargeColours samusChargeColors = Resource(SamusChargeColorFormat.FileName, "Samus charge colors", s => SamusChargeColorFile.Read(s).ToColours());
        CeresRidleyColorCatalog ceresRidleyColors = MigratingResource<CeresRidleyColorCatalog>(CeresRidleyColorFormat.FileName,
            "Ceres Ridley colors", (s, current) => CeresRidleyColorFile.Read(s, current), StockFingerprint.SelectedOnly);
        CeresRidleyFloorColorCatalog ceresRidleyFloorColors = Resource(CeresRidleyFloorColorFormat.FileName, "Ceres Ridley Mode-7 colors", s => CeresRidleyMode7ColorFile.Read(s));
        CeresRidleyGetawayBoostDelays ceresRidleyGetawayBoostDelays = Resource(CeresRidleyGetawayBoostDelayFormat.FileName, "Ceres Ridley getaway boost delays", s => CeresRidleyGetawayBoostDelayFiles.Read(s));
        SamusHyperBeamColours samusHyperBeamColors = Resource(SamusHyperBeamColorFormat.FileName, "Samus Hyper Beam colors", s => SamusHyperBeamColorFile.Read(s).ToColours());
        stock.RequireAllManifestFilesRead();
        var catalog = new InstalledAreaMapPresentation(new AreaMapPresentationParts { Areas = areas, HiddenCells = new(MapTileWords.PauseBlank.ToCell(), MapTileWords.FileSelectUndownloadedBlank.ToCell()), ContentIdentity = Convert.ToHexString(identity.GetHashAndReset()), HighlightCycle = cycle, Palettes = palettes, Labels = labels, Stations = stations, Landmarks = landmarks, SaveMarkers = saveMarkers, Arrows = arrows, WorldArtwork = artwork, Sprites = sprites, PauseSelectors = pauseSelectors, PauseReserveTanks = pauseReserveTanks, PauseReserveArrow = pauseReserveUi.ArrowColours, EscapeTimer = escapeTimer, EscapeTimerTiles = escapeTimerTiles, GameplayHud = gameplayHud, GameOver = gameOver, GameOptions = gameOptions, FileSelect = fileSelect, EscapeTypewriter = escapeTypewriter, IntroNarration = introNarration, IntroFont = introFont, EndingText = endingText.Typewriter(), EndingFont = endingFont, TitleGraphics = titleGraphics, TitlePalette = titlePalette, TitleGradient = titleGradient, RoomPaletteFx = roomPaletteFx, MotherBrainHealthPalette = motherBrainHealthPalette, MotherBrainRainbowPalette = motherBrainRainbowPalette, MotherBrainRoomColors = motherBrainRoomColors, RoomFxAnimatedTiles = roomFxAnimatedTiles, RoomFxPaletteBlends = roomFxPaletteBlends, PowerBombFixedColors = powerBombFixedColors, SamusVisorColors = samusVisorColors, SamusHurtColors = samusHurtColors, SamusSuitColors = samusSuitColors, SamusFullBodyCycleColors = samusFullBodyCycleColors, CrystalFlashColors = crystalFlashColors, SamusChargeColors = samusChargeColors, CeresRidleyColors = ceresRidleyColors, CeresRidleyFloorColors = ceresRidleyFloorColors, CeresRidleyGetawayBoostDelays = ceresRidleyGetawayBoostDelays, SamusHyperBeamColors = samusHyperBeamColors },
            new InstalledPresentationSources(new MapCharacterArtwork(tiles, hudTiles, pauseTiles),
                new GameplayMessageArtwork(gameplayMessageTitles, gameplayMessagePanels, gameplayMessageNotices), roomFxLayer3Tilemaps, staffCredits,
                endingText, screens, new PauseScreenArtwork(pauseBackdrops, pauseWireframes, pauseEquipmentBase, pauseEquipmentLabels, pauseReserveUi)));
        return catalog;

        T Resource<T>(string file, string description, Func<Stream, T> compile) where T : class =>
            MultiFileResource<T>([file], description, (streams, _) => compile(streams[0]));

        T MigratingResource<T>(string file, string description, Func<Stream, T?, T> compile, StockFingerprint fingerprint) where T : class =>
            MultiFileResource<T>([file], description, (streams, current) => compile(streams[0], current), fingerprint);

        // Stock is hash-checked and compiled exactly once. An unedited resource reuses that
        // compiled stock value; an edited one compiles its override against current stock.
        T MultiFileResource<T>(string[] files, string description, Func<Stream[], T?, T> compile, StockFingerprint fingerprint = StockFingerprint.SelectedOnly) where T : class
        {
            ArraySegment<byte>[] stockBytes = files.Select(stock.Read).ToArray();
            T stockValue = Compile(files.Select(file => Path.Combine(stockDirectory, file)).ToArray(), stockBytes, null);
            if (fingerprint == StockFingerprint.StockAndSelected)
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
        new(bytes.Array ?? throw new ArgumentException("A stock resource view must wrap an array.", nameof(bytes)),
            bytes.Offset, bytes.Count, writable: false, publiclyVisible: true);

    /// <summary>Installer integrity check; never repairs files or touches the override directory.</summary>
    public static void ValidateStock(string directory) => _ = Load(directory, null);

    /// <summary>Reads stock files that must match the extraction manifest's SHA-256 values.</summary>
    private sealed class VerifiedStockReader : IDisposable
    {
        /// <summary>Root of the extracted stock files subject to manifest checks.</summary>
        private readonly string directory;
        /// <summary>Expected digests and provenance for the stock content.</summary>
        private readonly AreaMapCatalogManifest manifest;
        /// <summary>Names actually verified during this catalog load.</summary>
        private readonly HashSet<string> read = new(StringComparer.Ordinal);
        /// <summary>Buffers retained only for one load and returned to the shared pool on disposal.</summary>
        private readonly List<byte[]> rented = [];

        /// <summary>Starts a manifest-bound read session in the extracted stock directory.</summary>
        private VerifiedStockReader(string directory, AreaMapCatalogManifest manifest)
        {
            this.directory = directory;
            this.manifest = manifest;
        }

        /// <summary>Loads and validates the manifest's version and required resource count.</summary>
        public static VerifiedStockReader Open(string directory)
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
                throw new InvalidDataException($"Map catalog manifest must contain the supported version, seven maps and all {AreaMapCatalogFormat.SharedResourceCount} shared presentation resource hashes.");
            return new VerifiedStockReader(directory, manifest);
        }

        /// <summary>Rents a buffer, reads one listed stock file, and checks its SHA-256 digest.</summary>
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

        /// <summary>Returns every stock buffer rented during this load to the shared pool.</summary>
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
