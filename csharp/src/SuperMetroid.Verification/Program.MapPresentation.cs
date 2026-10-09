using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>Confirms editable map colors project into HUD and pause views without changing compiled reveal rules.</summary>
    private static void VerifyMapPresentation()
    {
        Suite(nameof(VerifyIndexedPng), () => VerifyIndexedPng());
        Suite(nameof(VerifyQueuedVramAssets), () => VerifyQueuedVramAssets());
        var rules = new PresentationMapRules();
        var cell = new MapPresentationCell { TileColumn = 17, TileRow = 2, Palette = 3,
            Priority = true, FlipX = true, FlipY = false };
        var document = new MapPresentationDocument { Version = 1, Area = "Crateria",
            Cells = Enumerable.Repeat(cell, 64 * 32).ToArray() };
        string Json(MapPresentationDocument value) => JsonSerializer.Serialize(value,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        AreaMapPresentationAsset Load(string text) => AreaMapPresentationAsset.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(text)), rules);
        var map = Load(Json(document));
        AssertEqual((ushort)0x6c51, map.GetTile(5, 6).Raw, "map JSON compiles only presentation attributes");
        AssertTrue(map.RevealsCellAbove(5, 6), "replaced slope art retains its compiled reveal rule");
        AssertTrue(!map.IsDiscoverable(4, 6), "new artwork cannot create exploration cells");

        var system = new Bank80SystemState();
        byte[] projected = AreaMapTilemapBuilder.Build(map, system, MapTileWords.PauseBlank, MapRevealMode.Secret);
        ushort Word(int x, int y) => System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(
            projected.AsSpan(AreaMapLayout.GetTilemapWordIndex(x, y) * 2));
        AssertEqual(map.GetTile(5, 6).Raw, Word(5, 6), "shared pause/file-select projection uses editable art");
        AssertEqual(MapTileWords.PauseBlank.Raw, Word(4, 6), "secret reveal uses rules, not painted cells");
        AssertTrue(!system.IsMapTileExplored(AreaId.Crateria, 5, 6), "projection never mutates exploration");

        if (File.Exists("Super Metroid.smc"))
        {
            // HUD initialization now requires installed presentation. The import-only
            // cartridge reader supplies its stock fixture; minimap updates below use
            // the ROM-free bus and still exercise the production HUD owner.
            var source = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
            var hudArt = GameplayHudPresentation.Load(new MemoryStream(
                SuperMetroid.AssetExtraction.GameplayHudPresentationExtractor.Extract(source)));
            var hud = new HudState();
            hud.BindPresentation(hudArt);
            hud.Initialize(new TestAddressSpace(), HudSnapshot.CeresDebug);
            hud.UpdateMinimap(new ForbiddenMapBus(), system, AreaId.Crateria, 5, 5, 16, 16, 128, 128, 8,
                MapRevealMode.Secret, map);
            AssertEqual(map.GetTile(5, 6).ForHud(true).Raw, hud.Tiles[60], "live minimap consumes edited cells without cartridge reads");
            AssertTrue(system.IsMapTileExplored(AreaId.Crateria, 5, 5), "art replacement preserves slope corner exploration");

            var edited = document with { Cells = document.Cells.Select(c => c with { TileColumn = 19 }).ToArray() };
            var reloaded = Load(Json(edited));
            hud.UpdateMinimap(new ForbiddenMapBus(), system, AreaId.Crateria, 5, 5, 16, 16, 128, 128, 8,
                MapRevealMode.Secret, reloaded);
            AssertEqual(reloaded.GetTile(5, 6).ForHud(true).Raw, hud.Tiles[60], "reloaded edit changes live minimap without ROM patch");
        }
        AssertThrows<InvalidDataException>(() => Load(Json(document with { Version = 2 })), "reject unsupported map schema");
        AssertThrows<InvalidDataException>(() => Load(Json(document with { Area = "Norfair" })), "reject wrong area");
        AssertThrows<InvalidDataException>(() => Load(Json(document with { Cells = [cell] })), "reject incomplete layout");
        AssertThrows<InvalidDataException>(() => Load(Json(document).Replace("\"tileColumn\":17", "\"tileColumn\":32")), "reject invalid atlas cell");
        AssertThrows<InvalidDataException>(() => Load(Json(document).Replace("\"version\":1", "\"version\":1,\"revealEverything\":true")), "reject editable gameplay commands");
        if (File.Exists("Super Metroid.smc"))
        {
            var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
            foreach (AreaId area in Enum.GetValues<AreaId>())
            {
                var stock = SuperMetroid.AssetExtraction.AreaMapImporter.Load(bus, area);
                using var json = new MemoryStream();
                AreaMapPresentationAsset.Write(json, stock);
                json.Position = 0;
                var restored = AreaMapPresentationAsset.Load(json, stock);
                for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
                for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
                {
                    AssertEqual(stock.GetTile(x, y).Raw, restored.GetTile(x, y).Raw, "stock map JSON retains tile/attributes");
                    AssertEqual(stock.IsDiscoverable(x, y), restored.IsDiscoverable(x, y), "stock discovery rules survive import");
                    AssertEqual(stock.RevealsCellAbove(x, y), restored.RevealsCellAbove(x, y), "stock slope rules survive import");
                }
            }
            Console.WriteLine("Map presentation: all 14,336 retail cell words round-trip exactly.");
            VerifyMapPresentationCatalog(bus);
        }
        else Console.WriteLine("Map presentation: retail round-trip skipped; source ROM unavailable.");
        Console.WriteLine("Map presentation: JSON edits reach minimap/shared projection; independent exploration and strict validation pass.");
    }

    /// <summary>Supplies fixed discovery and reveal answers for map-presentation fixture cells.</summary>
    private sealed class PresentationMapRules : IAreaMapView
    {
        /// <summary>Area represented by the deterministic map-rule fixture.</summary>
        public AreaId Area => AreaId.Crateria;
        /// <summary>Returns the fixture's neutral tile word; art is supplied by the document under test.</summary>
        public MapTileWord GetTile(int x, int y) => MapTileWords.PauseBlank;
        /// <summary>Marks only the two fixture cells used to confirm discovery remains rule-owned.</summary>
        public bool IsDiscoverable(int x, int y) => x == 5 && y is 5 or 6;
        /// <summary>Marks the fixture's station-revealed test cell.</summary>
        public bool IsRevealedByMapStation(int x, int y) => x == 5 && y == 6;
        /// <summary>Marks the fixture's slope-like cell that reveals the tile above.</summary>
        public bool RevealsCellAbove(int x, int y) => x == 5 && y == 6;
    }

    /// <summary>Address space that makes any attempted map-presentation bus access fail immediately.</summary>
    private sealed class ForbiddenMapBus : ISnesAddressSpace
    {
        /// <summary>Fails the test if live presentation attempts any cartridge read.</summary>
        public static byte ReadByte(int address) => throw new InvalidOperationException($"Unexpected map ROM read {address:X6}.");
        /// <summary>Fails the test if a read-only map presentation attempts a bus write.</summary>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException("Unexpected map bus write.");
    }

    /// <summary>
    /// Full installed presentation required by real-room override checks. This is loaded
    /// once from the same installation as the maps, rather than silently allowing test
    /// runtimes to fall back to cartridge graphics when a room is initialized.
    /// </summary>
    private sealed class MapPresentationInstalledRoomAssets
    {
        /// <summary>Installed object-character graphics required to initialize real rooms.</summary>
        private readonly RoomCharacterAtlas standardObjects;
        /// <summary>Per-room character graphics bound to the runtime.</summary>
        private readonly RoomCharacterAtlasCatalog roomCharacters;
        /// <summary>Per-room palette data bound to the runtime.</summary>
        private readonly RoomStaticPaletteCatalog roomPalettes;
        /// <summary>Room metatile definitions used during room setup.</summary>
        private readonly RoomMetatileCatalog roomMetatiles;
        /// <summary>Room-specific visual layout data.</summary>
        private readonly RoomVisualLayoutCatalog visualLayouts;
        /// <summary>Installed background tilemaps used by room rendering.</summary>
        private readonly RoomBackgroundTilemapCatalog backgrounds;
        /// <summary>Installed sky tilemaps used by room rendering.</summary>
        private readonly RoomSkyTilemapCatalog skies;
        /// <summary>Enemy sprite tile artwork required by active-room initialization.</summary>
        private readonly EnemyTileArtworkCatalog enemyTiles;
        /// <summary>Beam projectile tiles required by the enemy presentation.</summary>
        private readonly BeamTileCatalog beamTiles;
        /// <summary>Samus body artwork required by room startup.</summary>
        private readonly SamusBodyArtworkCatalog samusBody;
        /// <summary>PLM elevator-platform visual catalog required by room population.</summary>
        private readonly RoomPlmElevatorPlatformVisualCatalog elevatorPlatform;

        /// <summary>Installed gameplay base palette bound for HUD and room initialization.</summary>
        public GameplayBasePaletteCatalog InitialPalettes { get; }
        /// <summary>Enemy tile catalog used by room-entry checks.</summary>
        public EnemyTileArtworkCatalog EnemyTiles => enemyTiles;
        /// <summary>Samus body catalog used by room-entry checks.</summary>
        public SamusBodyArtworkCatalog SamusBody => samusBody;

        /// <summary>Loads the installed graphics and palette resources needed for real-room verification.</summary>
        /// <param name="installation">Installation that owns the extracted asset catalogs.</param>
        public MapPresentationInstalledRoomAssets(SuperMetroid.AssetExtraction.GameInstallation installation)
        {
            InitialPalettes = installation.LoadGameplayBasePalettes();
            standardObjects = installation.LoadStandardObjects();
            roomCharacters = installation.LoadRoomCharacters();
            roomPalettes = installation.LoadRoomPalettes();
            roomMetatiles = installation.LoadRoomMetatiles();
            visualLayouts = installation.LoadRoomVisualLayouts();
            backgrounds = installation.LoadRoomBackgroundTilemaps();
            skies = installation.LoadRoomSkyTilemaps();
            enemyTiles = installation.LoadEnemyTiles();
            beamTiles = installation.LoadProjectiles().BeamTiles;
            samusBody = installation.LoadSamusBodyArt();
            elevatorPlatform = installation.LoadRoomPlmElevatorPlatformVisuals();
        }

        /// <summary>Attaches the installed room resources to a runtime before initializing its room.</summary>
        /// <param name="runtime">Runtime whose rendering owners need the extracted catalogs.</param>
        public void Bind(SuperMetroidRuntime runtime)
        {
            runtime.StandardObjectArt = standardObjects;
            runtime.RoomCharacterArt = roomCharacters;
            runtime.RoomPaletteArt = roomPalettes;
            runtime.RoomMetatileArt = roomMetatiles;
            runtime.RoomVisualLayouts = visualLayouts;
            runtime.RoomBackgroundTilemapArt = backgrounds;
            runtime.RoomSkyTilemapArt = skies;
            runtime.Enemies.TileArtwork = enemyTiles;
            runtime.BeamArtwork = beamTiles;
            runtime.SamusBodyArt = samusBody;
            runtime.RoomPlmElevatorPlatformVisuals = elevatorPlatform;
        }
    }

    /// <summary>Checks map-catalog round trips, override identity, and live content rebind behavior.</summary>
    private static void VerifyMapPresentationCatalog(ISnesAddressSpace bus)
    {
        using var temporaryDirectory = new TestTempDirectory("map-catalog");
        string root = temporaryDirectory.Root;
        string stock = Path.Combine(root, "game", "maps"), repaired = Path.Combine(root, "replacement-stock"), overrides = Path.Combine(root, "overrides", "maps");
        var rules = Enum.GetValues<AreaId>().ToDictionary(area => area, area => SuperMetroid.AssetExtraction.AreaMapImporter.Load(bus, area));
        SuperMetroid.AssetExtraction.GameInstallation installation =
            SuperMetroid.AssetExtraction.GameAssetInstaller.Install(
                Path.GetFullPath("Super Metroid.smc"), root);
        var original = installation.LoadMaps();
        var fixtureAssets = new MapPresentationInstalledRoomAssets(installation);
        GameplayBasePaletteCatalog initialPalettes = fixtureAssets.InitialPalettes;
        var reopened = AreaMapPresentationCatalog.Load(stock, overrides);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            var actual = original.Get(area);
            AssertEqual(rules[area].GetTile(x, y), actual.GetTile(x, y), "ROM-free catalog retains stock cells");
            AssertEqual(rules[area].IsDiscoverable(x, y), actual.IsDiscoverable(x, y), "ROM-free stock discovery parity");
            AssertEqual(rules[area].IsRevealedByMapStation(x, y), actual.IsRevealedByMapStation(x, y), "ROM-free station mask parity");
            AssertEqual(rules[area].RevealsCellAbove(x, y), actual.RevealsCellAbove(x, y), "ROM-free stock slope parity");
        }
        AssertEqual(original.ContentIdentity, reopened.ContentIdentity, "map catalog identity stable across reload");
        Directory.CreateDirectory(overrides);
        Suite(nameof(VerifyTitleGraphicsOverride), () => VerifyTitleGraphicsOverride(bus, stock, overrides, original));
        Suite(nameof(VerifyTitleSpriteCompositionOverride), () => VerifyTitleSpriteCompositionOverride(bus, stock, overrides, original));
        Suite(nameof(VerifyTitlePaletteOverride), () => VerifyTitlePaletteOverride(bus, stock, overrides, original));
        Suite(nameof(VerifyTitleGradientOverride), () => VerifyTitleGradientOverride(bus, stock, overrides, original));
        Suite(nameof(VerifyRoomPaletteFxOverride), () => VerifyRoomPaletteFxOverride(bus, stock, overrides, original, initialPalettes));
        Suite(nameof(VerifyRoomFxAnimatedTileArtworkOverride), () => VerifyRoomFxAnimatedTileArtworkOverride(bus, stock, overrides, original, initialPalettes));
        Suite(nameof(VerifyRoomFxLayer3TilemapOverride), () => VerifyRoomFxLayer3TilemapOverride(stock, overrides, original));
        Suite(nameof(VerifyRoomFxPaletteBlendOverride), () => VerifyRoomFxPaletteBlendOverride(stock, overrides, original));
        Suite(nameof(VerifyPowerBombFixedColorOverride), () => VerifyPowerBombFixedColorOverride(stock, overrides, original));
        Suite(nameof(VerifySamusVisorColorOverride), () => VerifySamusVisorColorOverride(stock, overrides, original, bus,
            initialPalettes, fixtureAssets));
        Suite(nameof(VerifySamusHurtColorOverride), () => VerifySamusHurtColorOverride(stock, overrides, original, bus));
        Suite(nameof(VerifySamusSuitColorOverride), () => VerifySamusSuitColorOverride(stock, overrides, original, bus, initialPalettes, fixtureAssets));
        Suite(nameof(VerifySamusFullBodyCycleColorOverride), () => VerifySamusFullBodyCycleColorOverride(stock, overrides, original, bus, initialPalettes, fixtureAssets));
        Suite(nameof(VerifyCrystalFlashColorOverride), () => VerifyCrystalFlashColorOverride(stock, overrides, original, bus));
        Suite(nameof(VerifySamusChargeColorOverride), () => VerifySamusChargeColorOverride(stock, overrides, original, bus));
        Suite(nameof(VerifyCeresRidleyColorOverride), () => VerifyCeresRidleyColorOverride(stock, overrides, original, bus, initialPalettes,
            fixtureAssets));
        Suite(nameof(VerifyCeresRidleyMode7ColorOverride), () => VerifyCeresRidleyMode7ColorOverride(stock, overrides, original, bus, initialPalettes));
        Suite(nameof(VerifySamusHyperBeamColorOverride), () => VerifySamusHyperBeamColorOverride(stock, overrides, original, bus, initialPalettes,
            fixtureAssets));
        Suite(nameof(VerifyMotherBrainHealthPaletteOverride), () => VerifyMotherBrainHealthPaletteOverride(bus, stock, overrides, original, initialPalettes));
        Suite(nameof(VerifyMotherBrainRainbowPaletteOverride), () => VerifyMotherBrainRainbowPaletteOverride(bus, stock, overrides, original, initialPalettes));
        Suite(nameof(VerifyMotherBrainRoomColors), () => VerifyMotherBrainRoomColors(bus, stock, overrides, original, initialPalettes));
        string name = AreaMapCatalogFormat.FileName(AreaId.Crateria);
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var document = JsonSerializer.Deserialize<MapPresentationDocument>(File.ReadAllText(Path.Combine(stock, name)), options)!;
        document.Cells[0] = document.Cells[0] with { TileColumn = (document.Cells[0].TileColumn + 1) % MapPresentationFormat.AtlasColumns };
        int visibleCell = Enumerable.Range(0, document.Cells.Length).First(i => rules[AreaId.Crateria].IsDiscoverable(i % 64, i / 64));
        document.Cells[visibleCell] = document.Cells[visibleCell] with { Palette = (document.Cells[visibleCell].Palette + 1) % 8 };
        string replacement = Path.Combine(overrides, name);
        File.WriteAllText(replacement, JsonSerializer.Serialize(document, options));
        byte[] editedBytes = File.ReadAllBytes(replacement);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
        for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
        {
            AssertEqual(original.Get(area).IsDiscoverable(x, y), edited.Get(area).IsDiscoverable(x, y), "override cannot alter discovery");
            AssertEqual(original.Get(area).IsRevealedByMapStation(x, y), edited.Get(area).IsRevealedByMapStation(x, y), "override cannot alter station reveal");
            AssertEqual(original.Get(area).RevealsCellAbove(x, y), edited.Get(area).RevealsCellAbove(x, y), "override cannot alter slope exploration");
        }
        Suite(nameof(VerifyLiveMapCatalog), () => VerifyLiveMapCatalog(bus, original, edited, rules.Values.ToArray(), initialPalettes,
            fixtureAssets));
        AssertTrue(edited.ContentIdentity != original.ContentIdentity, "map override changes content identity");
        AssertTrue(edited.Get(AreaId.Crateria).GetTile(0, 0) != original.Get(AreaId.Crateria).GetTile(0, 0), "catalog prefers valid override");
        SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, repaired, "test-provenance");
        var afterRepair = AreaMapPresentationCatalog.Load(repaired, overrides);
        AssertEqual(edited.ContentIdentity, afterRepair.ContentIdentity, "re-extracted stock preserves selected override");
        AssertTrue(editedBytes.AsSpan().SequenceEqual(File.ReadAllBytes(replacement)), "stock extraction and reload never rewrite override bytes");
        Suite(nameof(VerifyBundledMapMaskValidation), () => VerifyBundledMapMaskValidation(repaired));
        Suite(nameof(VerifyMapAtlasIntegration), () => VerifyMapAtlasIntegration(bus, stock, Path.Combine(root, "atlas-overrides"), original, rules.Values.ToArray()));
        Suite(nameof(VerifyHudAtlasIntegration), () => VerifyHudAtlasIntegration(bus, stock, Path.Combine(root, "hud-overrides"), original,
            rules.Values.ToArray(), initialPalettes, fixtureAssets));
        Suite(nameof(VerifyMapPaletteCycleIntegration), () => VerifyMapPaletteCycleIntegration(bus, stock, Path.Combine(root, "cycle-overrides"), original, rules.Values.ToArray()));
        Suite(nameof(VerifyMapStaticPaletteIntegration), () => VerifyMapStaticPaletteIntegration(bus, stock, Path.Combine(root, "palette-overrides"), original, rules.Values.ToArray()));
        Suite(nameof(VerifyWorldMapLabels), () => VerifyWorldMapLabels(bus, stock, Path.Combine(root, "label-overrides"), original));
        Suite(nameof(VerifyMapStationLayout), () => VerifyMapStationLayout(bus, stock, Path.Combine(root, "station-overrides"), original));
        Suite(nameof(VerifyMapLandmarks), () => VerifyMapLandmarks(bus, stock, Path.Combine(root, "landmark-overrides"), original));
        Suite(nameof(VerifyMapSaveMarkers), () => VerifyMapSaveMarkers(bus, stock, Path.Combine(root, "save-marker-overrides"), original));
        Suite(nameof(VerifyMapArrows), () => VerifyMapArrows(bus, stock, Path.Combine(root, "arrow-overrides"), original));
        Suite(nameof(VerifyMapScreens), () => VerifyMapScreens(bus, stock, Path.Combine(root, "screen-overrides"), original));
        Suite(nameof(VerifyMapSprites), () => VerifyMapSprites(bus, stock, Path.Combine(root, "sprite-overrides"), original));
        Suite(nameof(VerifyPauseTileAtlas), () => VerifyPauseTileAtlas(bus, stock, Path.Combine(root, "pause-art-overrides"), original));
        Suite(nameof(VerifyCompiledPauseEquipmentRules), () => VerifyCompiledPauseEquipmentRules(bus, original));
        Suite(nameof(VerifyPauseBackdrops), () => VerifyPauseBackdrops(bus, stock, Path.Combine(root, "pause-backdrop-overrides"), original));
        Suite(nameof(VerifyPauseWireframes), () => VerifyPauseWireframes(bus, stock, Path.Combine(root, "pause-wireframe-overrides"), original));
        Suite(nameof(VerifyPauseSelectors), () => VerifyPauseSelectors(bus, stock, Path.Combine(root, "pause-selector-overrides"), original));
        Suite(nameof(VerifyPauseReserveTankAssets), () => VerifyPauseReserveTankAssets(bus, stock, Path.Combine(root, "pause-reserve-tank-overrides"), original));
        Suite(nameof(VerifyPauseReserveUiAssets), () => VerifyPauseReserveUiAssets(bus, stock, Path.Combine(root, "pause-reserve-ui-overrides"), original));
        Suite(nameof(VerifyPauseEquipmentBaseAssets), () => VerifyPauseEquipmentBaseAssets(bus, stock, Path.Combine(root, "pause-equipment-base-overrides"), original));
        Suite(nameof(VerifyPauseEquipmentLabelAssets), () => VerifyPauseEquipmentLabelAssets(bus, stock, Path.Combine(root, "pause-equipment-label-overrides"), original));
        Suite(nameof(VerifyEscapeTimerPresentationAssets), () => VerifyEscapeTimerPresentationAssets(bus, stock,
            Path.Combine(root, "escape-timer-overrides"), original, initialPalettes));
        Suite(nameof(VerifyGameplayHudPresentationAssets), () => VerifyGameplayHudPresentationAssets(bus, stock,
            Path.Combine(root, "gameplay-hud-overrides"), original, initialPalettes));
        Suite(nameof(VerifyGameOverPresentationAssets), () => VerifyGameOverPresentationAssets(bus, stock, Path.Combine(root, "game-over-overrides"), original));
        Suite(nameof(VerifyGameOptionsPresentationAssets), () => VerifyGameOptionsPresentationAssets(bus, stock, Path.Combine(root, "game-options-overrides"), original));
        Suite(nameof(VerifyFileSelectPresentationAssets), () => VerifyFileSelectPresentationAssets(bus, stock, Path.Combine(root, "file-select-overrides"), original));
        Suite(nameof(VerifyGameplayMessagePanelAssets), () => VerifyGameplayMessagePanelAssets(bus, stock, Path.Combine(root, "gameplay-message-panel-overrides"), original));
        Suite(nameof(VerifyGameplayMessageNoticeAssets), () => VerifyGameplayMessageNoticeAssets(bus, stock, Path.Combine(root, "gameplay-message-notice-overrides"), original));
        Suite(nameof(VerifyEscapeTypewriterAssets), () => VerifyEscapeTypewriterAssets(bus, stock, Path.Combine(root, "escape-typewriter-overrides"), original));
        Suite(nameof(VerifyIntroNarrationAssets), () => VerifyIntroNarrationAssets(bus, stock, Path.Combine(root, "intro-narration-overrides"), original));
        Suite(nameof(VerifyIntroFontAssets), () => VerifyIntroFontAssets(bus, stock, Path.Combine(root, "intro-font-overrides"), original));
        Suite(nameof(VerifyEndingTextAssets), () => VerifyEndingTextAssets(bus, stock, Path.Combine(root, "ending-text-overrides"), original));
        Suite(nameof(VerifyEndingFontAssets), () => VerifyEndingFontAssets(bus, stock, Path.Combine(root, "ending-font-overrides"), original));
        Suite(nameof(VerifyStaffCreditsAssets), () => VerifyStaffCreditsAssets(stock, Path.Combine(root, "staff-credits-overrides"), original));
        // Stronger than composing individual range guards: no bus read or write
        // is permitted anywhere in this complete installed saved-map lifecycle.
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(bus, new ForbiddenMapBus(), original, original, verifyCapturedRendering: true));
        Suite(nameof(VerifyMapLoadAnchors), () => VerifyMapLoadAnchors(bus, original));
        Suite(nameof(VerifyCompiledMapScrollControls), () => VerifyCompiledMapScrollControls(bus, original));
        AssertThrows<IOException>(() => SuperMetroid.AssetExtraction.MapPresentationExtractor.Extract(bus, stock, "test-provenance"), "stock importer refuses overwrite");
        File.WriteAllText(replacement, "{ broken JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "corrupt override fails instead of selecting stock");
        AssertEqual("{ broken JSON", File.ReadAllText(replacement), "invalid override retained for user repair");
        File.WriteAllBytes(replacement, editedBytes);
        File.WriteAllText(Path.Combine(stock, name), "corrupt stock");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides), "stock integrity failure remains visible with override present");
        Console.WriteLine("Map catalog: deterministic reload, override precedence/identity, stock replacement preservation and corruption errors pass.");
    }

    /// <summary>Rejects cartridge reads while recording the expected writes from installed palette data.</summary>
    private sealed class PaletteReadForbiddenBus : ISnesAddressSpace
    {
        /// <summary>Captures writes to the native trailing-word destination for verification.</summary>
        public Dictionary<int, byte> Writes { get; } = new();
        /// <summary>Fails because installed Mother Brain palette presentation must not consult cartridge ROM.</summary>
        public static byte ReadByte(int address) => throw new InvalidOperationException(
            $"Installed Mother Brain palette unexpectedly read cartridge address ${address:X6}.");
        /// <summary>Records the expected trailing-word write and rejects writes to any other address.</summary>
        public void WriteByte(int address, byte value)
        {
            if (address is not (MotherBrainDrainedPaletteRomData.TrailingWordWram or
                MotherBrainDrainedPaletteRomData.TrailingWordWram + 1))
                throw new InvalidOperationException($"Unexpected Mother Brain palette write ${address:X6}.");
            Writes[address] = value;
        }
    }

    /// <summary>Checks that an installed Mother Brain rainbow-palette edit changes the active presentation.</summary>
    private static void VerifyMotherBrainRainbowPaletteOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original,
        GameplayBasePaletteCatalog initialPalettes)
    {
        string source = Path.Combine(stock, MotherBrainRainbowPaletteFormat.FileName);
        MotherBrainRainbowPaletteDocument document =
            JsonSerializer.Deserialize<MotherBrainRainbowPaletteDocument>(
                File.ReadAllBytes(source), MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted Mother Brain rainbow palette is null.");
        Suite(nameof(VerifyMotherBrainFakeDeathPalette), () => VerifyMotherBrainFakeDeathPalette(bus, original.MotherBrainRainbowPalette));
        for (int frame = 0; frame < MotherBrainRainbowPaletteFormat.RainbowFrameCount; frame++)
        {
            var native = new SnesCgram();
            var installed = new SnesCgram();
            int pointer = MotherBrainRainbowPaletteRomData.PointerTable + frame * sizeof(ushort);
            int palette = MotherBrainRainbowPaletteRomData.SourceBank |
                bus.ReadByte(pointer) | bus.ReadByte(pointer + 1) << 8;
            SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(native, bus, palette, MotherBrainRainbowPaletteRomData.ColorCount,
                MotherBrainRainbowPaletteRomData.BodyColor);
            SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(native, bus, palette, MotherBrainRainbowPaletteRomData.ColorCount,
                MotherBrainRainbowPaletteRomData.BrainColor);
            SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(native, bus, palette + MotherBrainRainbowPaletteRomData.ColorCount * sizeof(ushort),
                MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.SecondaryColor);
            original.MotherBrainRainbowPalette.ApplyRainbow(installed, frame);
            AssertTrue(native.Colors.SequenceEqual(installed.Colors),
                $"Mother Brain rainbow frame {frame} matches all native CGRAM colors");
        }

        foreach (bool draining in new[] { true, false })
        for (int frame = 0; frame < MotherBrainRainbowPaletteFormat.GreyFrameCount; frame++)
        {
            var native = new SnesCgram();
            var installed = new SnesCgram();
            // Revival writes thirteen colors and deliberately leaves the last two from
            // the preceding phase. Seed both CGRAM images to catch an overlong copy.
            native.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + 13, 0x1234);
            native.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + 14, 0x1235);
            native.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + 13, 0x2234);
            native.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + 14, 0x2235);
            installed.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + 13, 0x1234);
            installed.SetColor(MotherBrainRainbowPaletteRomData.BodyColor + 14, 0x1235);
            installed.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + 13, 0x2234);
            installed.SetColor(MotherBrainRainbowPaletteRomData.BrainColor + 14, 0x2235);
            int table = draining ? MotherBrainDrainedPaletteRomData.ToGreyTable :
                MotherBrainDrainedPaletteRomData.FromGreyTable;
            int count = draining ? MotherBrainDrainedPaletteRomData.DrainedColors :
                MotherBrainDrainedPaletteRomData.RevivalColors;
            int pointer = table + frame * sizeof(ushort);
            int palette = MotherBrainRainbowPaletteRomData.SourceBank |
                bus.ReadByte(pointer) | bus.ReadByte(pointer + 1) << 8;
            SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(native, bus, palette, count, MotherBrainRainbowPaletteRomData.BodyColor);
            SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(native, bus, palette, count, MotherBrainRainbowPaletteRomData.BrainColor);
            SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(native, bus, palette + count * sizeof(ushort),
                MotherBrainDrainedPaletteRomData.BackLegCount,
                MotherBrainDrainedPaletteRomData.BackLegColor);
            var guard = new PaletteReadForbiddenBus();
            if (draining) original.MotherBrainRainbowPalette.ApplyToGrey(guard, installed, frame);
            else original.MotherBrainRainbowPalette.ApplyFromGrey(guard, installed, frame);
            AssertTrue(native.Colors.SequenceEqual(installed.Colors),
                $"Mother Brain {(draining ? "drain" : "revival")} frame {frame} retains exact native CGRAM and preserved colors");
            int tail = palette + (count + MotherBrainDrainedPaletteRomData.BackLegCount) * sizeof(ushort);
            AssertEqual(bus.ReadByte(tail), guard.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram],
                "grey transition publishes native trailing WRAM low byte");
            AssertEqual(bus.ReadByte(tail + 1), guard.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram + 1],
                "grey transition publishes native trailing WRAM high byte");
            AssertEqual(2, guard.Writes.Count, "grey transition writes only its two native WRAM bytes");
        }

        var nativeNormal = new SnesCgram();
        var installedNormal = new SnesCgram();
        SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(nativeNormal, bus, MotherBrainRainbowPaletteRomData.NormalBrainSource,
            MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.BodyColor);
        SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(nativeNormal, bus, MotherBrainRainbowPaletteRomData.NormalBrainSource,
            MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.BrainColor);
        SuperMetroid.AssetExtraction.CartridgePaletteImporter.LoadToCgram(nativeNormal, bus, MotherBrainRainbowPaletteRomData.NormalSecondarySource,
            MotherBrainRainbowPaletteRomData.ColorCount, MotherBrainRainbowPaletteRomData.SecondaryColor);
        original.MotherBrainRainbowPalette.ApplyNormal(installedNormal);
        AssertTrue(nativeNormal.Colors.SequenceEqual(installedNormal.Colors),
            "Mother Brain normal restoration matches both fixed native sources");
        AssertEqual(MotherBrainBeamRomData.InitialColor,
            original.MotherBrainRainbowPalette.BeamInitialColor,
            "installed beam initial fixed color matches the cartridge");
        for (int frame = 0; frame < MotherBrainRainbowPaletteFormat.BeamCycleColorCount; frame++)
        {
            int cursor = frame * MotherBrainBeamRomData.ColorStride;
            int address = MotherBrainBeamRomData.ColorTable + cursor;
            ushort nativeBeamColor = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
            AssertEqual(nativeBeamColor, original.MotherBrainRainbowPalette.BeamColorWord(cursor),
                $"installed beam HDMA color {frame} matches bank-$88");
        }
        AssertEqual(ushort.MaxValue, original.MotherBrainRainbowPalette.BeamColorWord(
                MotherBrainRainbowPaletteFormat.BeamCycleColorCount * MotherBrainBeamRomData.ColorStride),
            "installed beam color loop retains its native signed terminator");
        var nativeBeamSequence = new MotherBrainRainbowBeamHdmaState
        {
            // Source words were compared independently against bank $88 above;
            // playback itself must use the installed catalog on both paths.
            PresentationColors = original.MotherBrainRainbowPalette,
        };
        var installedBeamSequence = new MotherBrainRainbowBeamHdmaState
        {
            PresentationColors = original.MotherBrainRainbowPalette,
        };
        var installedBeamReadGuard = new PaletteReadForbiddenBus();
        for (int frame = 0; frame < MotherBrainRainbowPaletteFormat.BeamCycleColorCount + 3; frame++)
        {
            nativeBeamSequence.Step(bus, true, 100, 95, SnesAngle.QuarterTurn, 0x4000);
            installedBeamSequence.Step(installedBeamReadGuard, true, 100, 95,
                SnesAngle.QuarterTurn, 0x4000);
            AssertTrue(nativeBeamSequence.Color == installedBeamSequence.Color &&
                nativeBeamSequence.ColorCursor == installedBeamSequence.ColorCursor &&
                nativeBeamSequence.Windows.SequenceEqual(installedBeamSequence.Windows),
                $"installed beam frame {frame} matches native color, cursor, and geometry through loop reset");
        }

        PaletteRgb5 beam = document.Rainbow[0].Body[0];
        document.Rainbow[0].Body[0] = beam with { Red = beam.Red == 31 ? 30 : beam.Red + 1 };
        PaletteRgb5 drain = document.ToGrey[0].Body[0];
        document.ToGrey[0].Body[0] = drain with { Blue = drain.Blue == 31 ? 30 : drain.Blue + 1 };
        PaletteRgb5 authoredTail = document.ToGrey[0].TrailingColor!;
        document.ToGrey[0] = document.ToGrey[0] with
        {
            TrailingColor = authoredTail with
            {
                Green = authoredTail.Green == 31 ? 30 : authoredTail.Green + 1,
            },
        };
        PaletteRgb5 revive = document.FromGrey[0].BackLegs[0];
        document.FromGrey[0].BackLegs[0] = revive with { Green = revive.Green == 31 ? 30 : revive.Green + 1 };
        PaletteRgb5 fakeDeath = document.FakeDeathToGrey![0][0];
        document.FakeDeathToGrey[0][0] = fakeDeath with
        {
            Red = fakeDeath.Red == 31 ? 30 : fakeDeath.Red + 1,
        };
        PaletteRgb5 fakeDeathRevival = document.FromGrey[0].Body[0];
        document.FromGrey[0].Body[0] = fakeDeathRevival with
        {
            Blue = fakeDeathRevival.Blue == 31 ? 30 : fakeDeathRevival.Blue + 1,
        };
        PaletteRgb5 normal = document.Normal.Body[0];
        document.Normal.Body[0] = normal with { Red = normal.Red == 31 ? 30 : normal.Red + 1 };
        document = document with
        {
            BeamInitial = document.BeamInitial with
            {
                Blue = document.BeamInitial.Blue == 31 ? 30 : document.BeamInitial.Blue + 1,
            },
        };
        PaletteRgb5 hdmaColor = document.BeamCycle[0];
        document.BeamCycle[0] = hdmaColor with
        {
            Red = hdmaColor.Red == 31 ? 30 : hdmaColor.Red + 1,
        };
        string replacement = Path.Combine(overrides, MotherBrainRainbowPaletteFormat.FileName);
        using (var stream = File.Create(replacement))
            MotherBrainRainbowPalettePresentation.Write(stream, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Mother Brain rainbow edit changes installed-content identity");
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus,
            initialPaletteArt: initialPalettes)
        {
            MapPresentation = edited,
        };
        var stockOutput = new SnesCgram();
        var editedOutput = new SnesCgram();
        original.MotherBrainRainbowPalette.ApplyRainbow(stockOutput, 0);
        runtime.Enemies.MotherBrainRainbowColors!.ApplyRainbow(editedOutput, 0);
        AssertTrue(stockOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor] !=
            editedOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor],
            "installed rainbow override reaches the room-enemy binding");
        stockOutput.Clear();
        editedOutput.Clear();
        var stockTailBus = new PaletteReadForbiddenBus();
        var editedTailBus = new PaletteReadForbiddenBus();
        original.MotherBrainRainbowPalette.ApplyToGrey(stockTailBus, stockOutput, 0);
        runtime.Enemies.MotherBrainRainbowColors.ApplyToGrey(editedTailBus, editedOutput, 0);
        AssertTrue(stockOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor] !=
            editedOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor],
            "drain override reaches the native body destination");
        AssertTrue(stockTailBus.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram] !=
            editedTailBus.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram] ||
            stockTailBus.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram + 1] !=
            editedTailBus.Writes[MotherBrainDrainedPaletteRomData.TrailingWordWram + 1],
            "drain trailing-color override reaches native WRAM publication");
        stockOutput.Clear();
        editedOutput.Clear();
        original.MotherBrainRainbowPalette.ApplyFromGrey(new PaletteReadForbiddenBus(), stockOutput, 0);
        runtime.Enemies.MotherBrainRainbowColors.ApplyFromGrey(new PaletteReadForbiddenBus(), editedOutput, 0);
        AssertTrue(stockOutput.Colors[MotherBrainDrainedPaletteRomData.BackLegColor] !=
            editedOutput.Colors[MotherBrainDrainedPaletteRomData.BackLegColor],
            "revival override reaches the native rear-leg destination");
        foreach (bool toGrey in new[] { true, false })
        {
            var stockFade = CaptureFakeDeathPaletteFrame(original.MotherBrainRainbowPalette, toGrey);
            var editedFade = CaptureFakeDeathPaletteFrame(runtime.Enemies.MotherBrainRainbowColors, toGrey);
            AssertTrue(stockFade.Colors[MotherBrainFakeDeathPaletteRomData.BrainColor] !=
                editedFade.Colors[MotherBrainFakeDeathPaletteRomData.BrainColor],
                "fake-death edited color reaches the production fade path");
            AssertEqual(1, stockFade.Colors.Zip(editedFade.Colors).Count(pair =>
                pair.First != pair.Second),
                "fake-death edit changes only one CGRAM word");
            AssertEqual(stockFade.FunctionTimer, editedFade.FunctionTimer,
                "fake-death edit preserves native frame timer");
            AssertEqual(stockFade.Function, editedFade.Function,
                "fake-death edit preserves native phase selection");
        }
        stockOutput.Clear();
        editedOutput.Clear();
        original.MotherBrainRainbowPalette.ApplyNormal(stockOutput);
        runtime.Enemies.MotherBrainRainbowColors.ApplyNormal(editedOutput);
        AssertTrue(stockOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor] !=
            editedOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor],
            "normal-restoration override reaches the native body destination");
        var stockBeam = new MotherBrainRainbowBeamHdmaState
        {
            PresentationColors = original.MotherBrainRainbowPalette,
        };
        var editedBeam = new MotherBrainRainbowBeamHdmaState
        {
            PresentationColors = runtime.Enemies.MotherBrainRainbowColors,
        };
        var beamReadGuard = new PaletteReadForbiddenBus();
        stockBeam.Step(beamReadGuard, true, 100, 95, SnesAngle.QuarterTurn, 0x4000);
        editedBeam.Step(beamReadGuard, true, 100, 95, SnesAngle.QuarterTurn, 0x4000);
        AssertTrue(stockBeam.Color != editedBeam.Color &&
            stockBeam.Windows.SequenceEqual(editedBeam.Windows),
            "beam initial-color edit changes only color, not HDMA geometry");
        stockBeam.Step(beamReadGuard, true, 100, 95, SnesAngle.QuarterTurn, 0x4000);
        editedBeam.Step(beamReadGuard, true, 100, 95, SnesAngle.QuarterTurn, 0x4000);
        AssertTrue(stockBeam.Color != editedBeam.Color &&
            stockBeam.ColorCursor == editedBeam.ColorCursor &&
            stockBeam.Windows.SequenceEqual(editedBeam.Windows),
            "beam cycle edit reaches the live HDMA frame without a ROM read or timing change");

        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Rainbow = document.Rainbow.Take(9).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject truncated Mother Brain rainbow loop");
        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                ToGrey = document.ToGrey.Select((item, index) => index == 0 ?
                    item with { TrailingColor = null } : item).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject missing Mother Brain grey trailing color");
        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Normal = document.Normal with { Body = document.Normal.Body.Take(14).ToArray() },
            }, MapPresentationFormat.JsonOptions))), "reject truncated Mother Brain normal palette");
        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                BeamCycle = document.BeamCycle.Take(37).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject truncated Mother Brain beam color cycle");
        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                FakeDeathToGrey = document.FakeDeathToGrey!.Take(7).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject truncated Mother Brain fake-death grey fade");
        // Older override documents remain loadable after stock regeneration, with only
        // the newly introduced fake-death colors supplied by the current stock file.
        byte[] legacy = JsonSerializer.SerializeToUtf8Bytes(document with
        {
            Version = MotherBrainRainbowPaletteFormat.PreFakeDeathVersion,
            FakeDeathToGrey = null,
        }, MapPresentationFormat.JsonOptions);
        File.WriteAllBytes(replacement, legacy);
        AreaMapPresentationCatalog migrated = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(migrated.MotherBrainRainbowPalette.BeamInitialColor !=
            original.MotherBrainRainbowPalette.BeamInitialColor,
            "version-two Mother Brain override preserves the existing edited beam color");
        AssertTrue(CaptureFakeDeathPaletteFrame(migrated.MotherBrainRainbowPalette, toGrey: true)
            .Colors.SequenceEqual(CaptureFakeDeathPaletteFrame(original.MotherBrainRainbowPalette,
                toGrey: true).Colors),
            "version-two override inherits the new fake-death colors from verified stock");
        File.Delete(replacement);
        AssertEqual(original.ContentIdentity, AreaMapPresentationCatalog.Load(stock, overrides).ContentIdentity,
            "removing Mother Brain rainbow override restores installed-content identity");
        Console.WriteLine("Mother Brain palette: 10 rainbow, 38 HDMA beam, 16 drain/revival and eight fake-death grey frames; native CGRAM parity, ROM-free fades, live edits, legacy overrides and strict validation pass.");
    }

    /// <summary>Checks that an installed Mother Brain health-palette edit reaches runtime CGRAM.</summary>
    private static void VerifyMotherBrainHealthPaletteOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original,
        GameplayBasePaletteCatalog initialPalettes)
    {
        string source = Path.Combine(stock, MotherBrainHealthPaletteFormat.FileName);
        MotherBrainHealthPaletteDocument document =
            JsonSerializer.Deserialize<MotherBrainHealthPaletteDocument>(
                File.ReadAllBytes(source), MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted Mother Brain health palette is null.");
        MotherBrainHealthPalettePresentation extracted =
            MotherBrainHealthPalettePresentation.Load(new MemoryStream(
                SuperMetroid.AssetExtraction.MotherBrainHealthPaletteExtractor.Extract(bus),
                writable: false));
        foreach (var (health, state) in new (ushort, int)[]
            { (36000, 0), (9000, 0), (8999, 1), (5400, 1), (5399, 2), (1800, 2), (1799, 3), (0, 3) })
        {
            var native = new SnesCgram();
            var installed = new SnesCgram();
            MotherBrainHealthPalette.Apply(bus, native, health, extracted);
            MotherBrainHealthPalette.Apply(new ForbiddenMapBus(), installed, health,
                original.MotherBrainHealthPalette);
            AssertTrue(native.Colors.SequenceEqual(installed.Colors),
                $"Mother Brain health {health} stock colors exactly match the native three-copy output");
            AssertEqual((ushort)0, installed.Colors[MotherBrainRainbowPaletteRomData.BrainColor - 1],
                "Mother Brain health palette leaves transparent color untouched");
            AssertEqual((ushort)0, installed.Colors[MotherBrainRainbowPaletteRomData.SecondaryColor +
                MotherBrainRainbowPaletteRomData.ColorCount],
                "Mother Brain health palette leaves adjacent colors untouched");
            AssertEqual(state, health >= MotherBrainHealthPaletteRomData.FirstThreshold ? 0 :
                health >= MotherBrainHealthPaletteRomData.SecondThreshold ? 1 :
                health >= MotherBrainHealthPaletteRomData.FinalThreshold ? 2 : 3,
                "native health thresholds still choose the four damage states");
        }

        PaletteRgb5 body = document.Body[2][0];
        document.Body[2][0] = body with { Red = body.Red == 31 ? 30 : body.Red + 1 };
        PaletteRgb5 leg = document.BackLegs[2][0];
        document.BackLegs[2][0] = leg with { Blue = leg.Blue == 31 ? 30 : leg.Blue + 1 };
        string replacement = Path.Combine(overrides, MotherBrainHealthPaletteFormat.FileName);
        using (var stream = File.Create(replacement))
            MotherBrainHealthPalettePresentation.Write(stream, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Mother Brain health palette override changes installed-content identity");
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus,
            initialPaletteArt: initialPalettes)
        {
            MapPresentation = edited,
        };
        var output = new SnesCgram();
        var stockOutput = new SnesCgram();
        MotherBrainHealthPalette.Apply(new ForbiddenMapBus(), stockOutput, 5399,
            original.MotherBrainHealthPalette);
        MotherBrainHealthPalette.Apply(new ForbiddenMapBus(), output, 5399,
            runtime.Enemies.MotherBrainHealthColors);
        AssertTrue(output.Colors[MotherBrainRainbowPaletteRomData.BodyColor] !=
            stockOutput.Colors[MotherBrainRainbowPaletteRomData.BodyColor],
            "installed Mother Brain body edit reaches the live enemy binding");
        AssertTrue(output.Colors[MotherBrainRainbowPaletteRomData.SecondaryColor] !=
            stockOutput.Colors[MotherBrainRainbowPaletteRomData.SecondaryColor],
            "installed Mother Brain leg edit reaches the live enemy binding");
        AssertEqual(output.Colors[MotherBrainRainbowPaletteRomData.BodyColor],
            output.Colors[MotherBrainRainbowPaletteRomData.BrainColor],
            "body and brain still share the native palette");

        AssertThrows<InvalidDataException>(() => MotherBrainHealthPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Version = MotherBrainHealthPaletteFormat.Version + 1,
            }, MapPresentationFormat.JsonOptions))), "reject unsupported Mother Brain palette version");
        AssertThrows<InvalidDataException>(() => MotherBrainHealthPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Body = document.Body.Take(3).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject incomplete Mother Brain damage states");
        AssertThrows<InvalidDataException>(() => MotherBrainHealthPalettePresentation.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                BackLegs = document.BackLegs.Select((colors, index) => index == 0 ?
                    colors.Take(14).ToArray() : colors).ToArray(),
            }, MapPresentationFormat.JsonOptions))), "reject truncated Mother Brain rear-leg palette");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing Mother Brain health palette override restores installed-content identity");
        Console.WriteLine("Mother Brain health palette: 8 threshold boundaries, ROM-free stock copies, live body/leg edits, schema checks and content identity pass.");
    }

    /// <summary>Checks installed room palette-FX overrides against live room palette output.</summary>
    private static void VerifyRoomPaletteFxOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original,
        GameplayBasePaletteCatalog initialPalettes)
    {
        string source = Path.Combine(stock, RoomPaletteFxPresentationFormat.FileName);
        RoomPaletteFxPresentationDocument document =
            JsonSerializer.Deserialize<RoomPaletteFxPresentationDocument>(
                File.ReadAllBytes(source),
                MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted room palette-FX document is null.");
        PaletteRgb5 color = document.NorfairForegroundAndHeatPhase[0][0];
        document.NorfairForegroundAndHeatPhase[0][0] = color with
        {
            Red = color.Red == 31 ? 30 : color.Red + 1,
        };
        PaletteRgb5 waterfall = document.MaridiaBackgroundWaterfalls[0][0];
        document.MaridiaBackgroundWaterfalls[0][0] = waterfall with
        {
            Blue = waterfall.Blue == 31 ? 30 : waterfall.Blue + 1,
        };
        PaletteRgb5 greenLight = document.WreckedShipGreenLights[0][0];
        document.WreckedShipGreenLights[0][0] = greenLight with
        {
            Green = greenLight.Green == 31 ? 30 : greenLight.Green + 1,
        };
        PaletteRgb5 redBrinstar = document.RedBrinstarBackgroundGlow[0][0];
        document.RedBrinstarBackgroundGlow[0][0] = redBrinstar with
        {
            Red = redBrinstar.Red == 31 ? 30 : redBrinstar.Red + 1,
        };
        PaletteRgb5 tourian = document.TourianGlow[0][0];
        document.TourianGlow[0][0] = tourian with
        {
            Blue = tourian.Blue == 31 ? 30 : tourian.Blue + 1,
        };
        PaletteRgb5 blueSpore = document.BrinstarBlueSpores[0][0];
        document.BrinstarBlueSpores[0][0] = blueSpore with
        {
            Green = blueSpore.Green == 31 ? 30 : blueSpore.Green + 1,
        };
        PaletteRgb5 bombTorizo = document.BombTorizoBelly[0][0];
        document.BombTorizoBelly[0][0] = bombTorizo with
        {
            Red = bombTorizo.Red == 31 ? 30 : bombTorizo.Red + 1,
        };
        PaletteRgb5 goldenTorizo = document.GoldenTorizoBelly[0][0];
        document.GoldenTorizoBelly[0][0] = goldenTorizo with
        {
            Blue = goldenTorizo.Blue == 31 ? 30 : goldenTorizo.Blue + 1,
        };
        PaletteRgb5 statueGrey = document.TourianStatueGrey[0][0];
        document.TourianStatueGrey[0][0] = statueGrey with
        {
            Green = statueGrey.Green == 31 ? 30 : statueGrey.Green + 1,
        };
        PaletteRgb5 surfaceLightning = document.CrateriaSurfaceLightning[0][0];
        document.CrateriaSurfaceLightning[0][0] = surfaceLightning with
        {
            Blue = surfaceLightning.Blue == 31 ? 30 : surfaceLightning.Blue + 1,
        };
        PaletteRgb5 darkLightning = document.CrateriaUnusedDarkLightning[0][0];
        document.CrateriaUnusedDarkLightning[0][0] = darkLightning with
        {
            Red = darkLightning.Red == 31 ? 30 : darkLightning.Red + 1,
        };
        PaletteRgb5 gunshipLight = document.CeresGunshipEngineLights[0][0];
        document.CeresGunshipEngineLights[0][0] = gunshipLight with
        {
            Green = gunshipLight.Green == 31 ? 30 : gunshipLight.Green + 1,
        };
        PaletteRgb5 navigationLight = document.CeresNavigationLights[0][0];
        document.CeresNavigationLights[0][0] = navigationLight with
        {
            Blue = navigationLight.Blue == 31 ? 30 : navigationLight.Blue + 1,
        };
        PaletteRgb5 fadeInColor = document.PlanetZebesTextFadeIn[0][0];
        document.PlanetZebesTextFadeIn[0][0] = fadeInColor with
        {
            Red = fadeInColor.Red == 31 ? 30 : fadeInColor.Red + 1,
        };
        PaletteRgb5 fadeOutColor = document.PlanetZebesTextFadeOut[0][0];
        document.PlanetZebesTextFadeOut[0][0] = fadeOutColor with
        {
            Green = fadeOutColor.Green == 31 ? 30 : fadeOutColor.Green + 1,
        };
        PaletteRgb5 motherBrainLight = document.OldMotherBrainBackgroundLights[0][0];
        document.OldMotherBrainBackgroundLights[0][0] = motherBrainLight with
        {
            Blue = motherBrainLight.Blue == 31 ? 30 : motherBrainLight.Blue + 1,
        };
        PaletteRgb5 gunshipGlow = document.CinematicGunshipGlow[0][0];
        document.CinematicGunshipGlow[0][0] = gunshipGlow with
        {
            Red = gunshipGlow.Red == 31 ? 30 : gunshipGlow.Red + 1,
        };
        PaletteRgb5 zebesFade = document.ExplodingZebesFade[0][0];
        document.ExplodingZebesFade[0][0] = zebesFade with
        {
            Green = zebesFade.Green == 31 ? 30 : zebesFade.Green + 1,
        };
        PaletteRgb5 unusedFade = document.UnusedCinematicFade[0][0];
        document.UnusedCinematicFade[0][0] = unusedFade with
        {
            Blue = unusedFade.Blue == 31 ? 30 : unusedFade.Blue + 1,
        };
        PaletteRgb5 titleLogo = document.TitleLogoFade[0][0];
        document.TitleLogoFade[0][0] = titleLogo with
        {
            Red = titleLogo.Red == 31 ? 30 : titleLogo.Red + 1,
        };
        PaletteRgb5 nintendoFade = document.NintendoSharedFade[0][0];
        document.NintendoSharedFade[0][0] = nintendoFade with
        {
            Green = nintendoFade.Green == 31 ? 30 : nintendoFade.Green + 1,
        };
        ChangeRed(document.ZebesExplosionForeground);
        ChangeRed(document.ZebesExplosionFinale);
        ChangeRed(document.ZebesExplosionWhiteout);
        ChangeRed(document.ZebesExplosionAfterglow);
        ChangeRed(document.ZebesExplosionLava);
        ChangeRed(document.ZebesExplosionCrust);
        ChangeRed(document.ZebesExplosionGreyClouds);
        ChangeRed(document.ZebesExplosionGunship);
        ChangeRed(document.SamusLoadingPowerSuit);
        ChangeRed(document.SamusLoadingVariaSuit);
        ChangeRed(document.SamusLoadingGravitySuit);
        ChangeRed(document.PostCreditsIconGlare);
        ChangeRed(document.TourianEscapeShutter);
        ChangeRed(document.TourianEscapeBackground);
        ChangeRed(document.TourianEscapeSharedRedFlash);
        ChangeRed(document.OldTourianEscapeRedFlash);
        ChangeRed(document.OldTourianEscapeOrangeRailings);
        ChangeRed(document.OldTourianEscapeYellowPanels);
        ChangeRed(document.UpperCrateriaEscapeRedFlash);
        ChangeRed(document.CrateriaEscapeYellowLightning);
        ChangeRed(document.CrateriaEscapeCreBlockPixel);
        ChangeRed(document.BeaconFlashing);

        string replacement = Path.Combine(overrides, RoomPaletteFxPresentationFormat.FileName);
        using (var stream = File.Create(replacement))
            RoomPaletteFxPresentation.Write(stream, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "room palette-FX override changes installed-content identity");

        NorfairEnvironmentalPaletteFxProgramDefinition definition =
            NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All.Single(item =>
                item.Owner == NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase);
        AssertOverride(definition.DefinitionPointer, definition.ColorByteIndex,
            "selected Norfair");

        MaridiaEnvironmentalPaletteFxProgramDefinition waterfallDefinition =
            MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All.Single(item =>
                item.Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls);
        AssertOverride(waterfallDefinition.DefinitionPointer,
            waterfallDefinition.ColorByteIndex, "selected Maridia");
        AssertOverride(
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.PoweredDefinition,
            WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Wrecked Ship");
        AssertOverride(
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            RedBrinstarGlowPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Red Brinstar");
        AssertOverride(
            TourianGlowPaletteFxProgramMechanicsDefinitions.LiveDefinitionPointer,
            TourianGlowPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Tourian");
        BrinstarBlueSporePaletteFxProgramDefinition blueSporeDefinition =
            BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.All.Single(item =>
                item.Owner == BrinstarBlueSporePaletteOwner.StandardRooms);
        AssertOverride(blueSporeDefinition.DefinitionPointer,
            BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Brinstar blue-spore");
        foreach (TorizoBellyPaletteFxProgramDefinition torizoDefinition in
                 TorizoBellyPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(torizoDefinition.DefinitionPointer,
                TorizoBellyPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
                $"{torizoDefinition.Owner} belly");
        }
        TourianStatueGreyPaletteFxProgramDefinition statueDefinition =
            TourianStatueGreyPaletteFxProgramMechanicsDefinitionsTooling.All[0];
        AssertOverride(statueDefinition.DefinitionPointer,
            statueDefinition.ColorByteIndex, "Tourian statue grey-out");
        foreach (CrateriaLightningPaletteFxProgramDefinition lightningDefinition in
                 CrateriaLightningPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(
                lightningDefinition.DefinitionPointer,
                lightningDefinition.ColorByteIndex,
                $"Crateria {lightningDefinition.Owner}");
        }
        AssertOverride(
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .GunshipEngineDefinitionPointer,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions.GunshipEngineColorIndex,
            "Ceres gunship engine");
        AssertOverride(
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .SpriteNavigationLightsDefinitionPointer,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .SpriteNavigationLightsColorIndex,
            "sprite Ceres navigation lights");
        AssertOverride(
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .BackgroundNavigationLightsDefinitionPointer,
            CeresCinematicLightPaletteFxProgramMechanicsDefinitions
                .BackgroundNavigationLightsColorIndex,
            "background Ceres navigation lights");
        foreach (PlanetZebesTextPaletteFxProgramDefinition fadeDefinition in
                 PlanetZebesTextPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(
                fadeDefinition.DefinitionPointer,
                fadeDefinition.ColorByteIndex,
                $"PLANET ZEBES {fadeDefinition.Owner}");
        }
        foreach (CinematicGlowPaletteFxProgramDefinition glowDefinition in
                 CinematicGlowPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(
                glowDefinition.DefinitionPointer,
                glowDefinition.ColorByteIndex,
                $"cinematic glow {glowDefinition.Owner}");
        }
        AssertOverride(
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            ExplodingZebesFadePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "exploding Zebes fade");
        AssertOverride(
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            UnusedCinematicFadePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "unused cinematic fade");
        AssertOverride(
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            TitleLogoFadePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "title-logo fade");
        foreach ((string owner, ushort definitionPointer,
                     ushort colorByteIndex) in new[]
                 {
                     ("BootLogo",
                         NintendoLogoFadePaletteFxProgramMechanicsDefinitions.BootLogoDefinitionPointer,
                         NintendoLogoFadePaletteFxProgramMechanicsDefinitions.BootLogoColorByteIndex),
                     ("Copyright",
                         NintendoLogoFadePaletteFxProgramMechanicsDefinitions.CopyrightDefinitionPointer,
                         NintendoLogoFadePaletteFxProgramMechanicsDefinitions.CopyrightColorByteIndex),
                 })
        {
            AssertOverride(definitionPointer, colorByteIndex, $"Nintendo shared fade {owner}");
        }
        AssertOverride(
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Zebes explosion foreground");
        AssertOverride(
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            ZebesExplosionFinalePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Zebes explosion finale");
        foreach ((string owner, ushort definitionPointer,
                     ushort colorByteIndex) in new[]
                 {
                     ("WideExplosionBackground",
                         ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                             .WideExplosionBackgroundDefinitionPointer,
                         ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                             .WideExplosionBackgroundColorByteIndex),
                     ("SpaceWhiteout",
                         ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                             .SpaceWhiteoutDefinitionPointer,
                         ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions
                             .SpaceWhiteoutColorByteIndex),
                 })
        {
            AssertOverride(definitionPointer, colorByteIndex,
                $"Zebes explosion whiteout {owner}");
        }
        foreach (ZebesExplosionAmbientPaletteFxProgramDefinition ambientDefinition in
                 ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(ambientDefinition.DefinitionPointer,
                ambientDefinition.ColorByteIndex,
                $"Zebes explosion ambient {ambientDefinition.Owner}");
        }
        foreach (ZebesExplosionLayerFadePaletteFxProgramDefinition layerDefinition in
                 ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(layerDefinition.DefinitionPointer, layerDefinition.ColorByteIndex,
                $"Zebes explosion layer fade {layerDefinition.Owner}");
        }
        AssertOverride(
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Zebes explosion gunship");
        foreach (SamusLoadingSuitPaletteFxProgramDefinition suitDefinition in
                 SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(suitDefinition.DefinitionPointer,
                SamusLoadingSuitPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
                $"Samus loading {suitDefinition.Owner}");
        }
        AssertOverride(
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "post-credits icon glare");
        foreach (TourianEscapeRedFlashPaletteFxProgramDefinition flashDefinition in
                 TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(flashDefinition.DefinitionPointer, flashDefinition.ColorByteIndex,
                $"Tourian escape {flashDefinition.Owner}");
        }
        foreach (TourianEscapeSharedRedFlashPaletteFxProgramDefinition sharedDefinition in
                 TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitionsTooling.All)
        {
            AssertOverride(sharedDefinition.DefinitionPointer, sharedDefinition.ColorByteIndex,
                $"Tourian escape shared {sharedDefinition.Owner}");
        }
        AssertOverride(
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "old Tourian escape red flash");
        foreach (OldTourianEscapeAccentPaletteFxProgramDefinition accentDefinition in
                 OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(accentDefinition.DefinitionPointer, accentDefinition.ColorByteIndex,
                $"old Tourian escape {accentDefinition.Owner}");
        }
        AssertOverride(
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "upper Crateria escape red flash");
        foreach (CrateriaEscapeLightningPaletteFxProgramDefinition lightningDefinition in
                 CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.All)
        {
            AssertOverride(lightningDefinition.DefinitionPointer,
                lightningDefinition.ColorByteIndex,
                $"Crateria escape {lightningDefinition.Owner}");
        }
        AssertOverride(
            BeaconPaletteFxProgramMechanicsDefinitions.DefinitionPointer,
            BeaconPaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            "Crateria and Brinstar beacon flash");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing room palette-FX override restores installed-content identity");
        Console.WriteLine(
            "Room palette-FX override: content identity and forty-seven live palette " +
            "CGRAM outputs " +
            "change immediately, then restore exactly.");

        static void ChangeRed(PaletteRgb5[][] frames)
        {
            PaletteRgb5 color = frames[0][0];
            frames[0][0] = color with
            {
                Red = color.Red == 31 ? 30 : color.Red + 1,
            };
        }

        void AssertOverride(ushort definitionPointer, ushort colorByteIndex,
            string description)
        {
            var stockRuntime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus,
                initialPaletteArt: initialPalettes)
            {
                MapPresentation = original,
            };
            var editedRuntime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus,
                initialPaletteArt: initialPalettes)
            {
                MapPresentation = edited,
            };
            stockRuntime.RoomPaletteFx.SpawnDefinition(bus, definitionPointer, 0);
            editedRuntime.RoomPaletteFx.SpawnDefinition(bus, definitionPointer, 0);
            stockRuntime.RoomPaletteFx.Step(bus, stockRuntime.Cgram, original.RoomPaletteFx, 0, 0, false, false);
            editedRuntime.RoomPaletteFx.Step(bus, editedRuntime.Cgram, edited.RoomPaletteFx, 0, 0, false, false);
            AssertTrue(
                stockRuntime.Cgram.Colors[colorByteIndex / sizeof(ushort)] !=
                editedRuntime.Cgram.Colors[colorByteIndex / sizeof(ushort)],
                $"runtime catalog binding consumes {description} palette-FX override");
        }
    }

    /// <summary>Checks that edited title gradient data changes rendered title scanlines.</summary>
    private static void VerifyTitleGradientOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string source = Path.Combine(stock, TitleGradientFormat.FileName);
        TitleGradientDocument document = JsonSerializer.Deserialize<TitleGradientDocument>(
            File.ReadAllBytes(source),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted title gradient document is null.");
        foreach (TitleGradientVariant variant in document.Variants)
        {
            TitleGradientScanline line = variant.Lines[0];
            variant.Lines[0] = line with { Red = line.Red == 31 ? 30 : line.Red + 1 };
        }

        string replacement = Path.Combine(overrides, TitleGradientFormat.FileName);
        using (var stream = File.Create(replacement))
            TitleGradientPresentation.Write(stream, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "title gradient override changes installed-content identity");
        (TitleGradientLine[] rendered, ushort selectedZoom) = CaptureInstalledTitleGradient(
            bus, edited.TitleGradient, edited.TitlePalette, edited.TitleGraphics);
        AssertTrue(rendered.AsSpan().SequenceEqual(edited.TitleGradient.Resolve(selectedZoom)),
            "production title consumes selected gradient override");
        AssertTrue(!rendered.AsSpan().SequenceEqual(original.TitleGradient.Resolve(selectedZoom)),
            "gradient override visibly changes production title output");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing title gradient override restores installed-content identity");
        Console.WriteLine("Title gradient override: content identity and production title output change immediately, then restore exactly.");
    }

    /// <summary>Checks edited title colors in ordinary and skip-copyright presentation paths.</summary>
    private static void VerifyTitlePaletteOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string source = Path.Combine(stock, TitlePaletteFormat.FileName);
        TitlePaletteDocument document = JsonSerializer.Deserialize<TitlePaletteDocument>(
            File.ReadAllBytes(source),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted title palette document is null.");
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5 color = document.Colors[index];
            document.Colors[index] = color with { Red = color.Red == 31 ? 30 : color.Red + 1 };
        }
        PaletteRgb5 ambient = document.BabyMetroidTubeLight[0][0];
        document.BabyMetroidTubeLight[0][0] = ambient with
        {
            Blue = ambient.Blue == 31 ? 30 : ambient.Blue + 1,
        };
        document = document with
        {
            SkipCopyrightWhite = document.SkipCopyrightWhite with { Red = 0, Green = 31, Blue = 0 },
            SkipCopyrightRed = document.SkipCopyrightRed with { Red = 0, Green = 0, Blue = 31 },
        };

        string replacement = Path.Combine(overrides, TitlePaletteFormat.FileName);
        using (var stream = File.Create(replacement))
            TitlePalettePresentation.Write(stream, document);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "title palette override changes installed-content identity");
        var stockTitle = new TitleSequenceState(bus,
            titleGradientPresentation: original.TitleGradient,
            titlePalettePresentation: original.TitlePalette,
            titleGraphicsPresentation: original.TitleGraphics);
        var editedTitle = new TitleSequenceState(bus,
            titleGradientPresentation: edited.TitleGradient,
            titlePalettePresentation: edited.TitlePalette,
            titleGraphicsPresentation: edited.TitleGraphics);
        AssertTrue(editedTitle.PaletteColors.SequenceEqual(edited.TitlePalette.Colors),
            "production title consumes selected palette override");
        stockTitle.Step(0);
        editedTitle.Step(0);
        TitleScreenAmbientPaletteFxProgramDefinition tube =
            TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All.Single(
                definition => definition.Owner ==
                    TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight);
        AssertTrue(stockTitle.PaletteColors[tube.ColorByteIndex / 2] !=
            editedTitle.PaletteColors[tube.ColorByteIndex / 2],
            "production title consumes selected ambient palette override");
        AssertTrue(!stockTitle.Render().AsSpan().SequenceEqual(editedTitle.Render()),
            "palette override visibly changes production title output");

        var stockSkip = new TitleSequenceState(bus,
            titleGradientPresentation: original.TitleGradient,
            titlePalettePresentation: original.TitlePalette,
            titleGraphicsPresentation: original.TitleGraphics);
        var editedSkip = new TitleSequenceState(bus,
            titleGradientPresentation: edited.TitleGradient,
            titlePalettePresentation: edited.TitlePalette,
            titleGraphicsPresentation: edited.TitleGraphics);
        stockSkip.Step((ushort)SuperMetroid.Core.Input.SnesButton.Start);
        editedSkip.Step((ushort)SuperMetroid.Core.Input.SnesButton.Start);
        for (int frame = 0; frame < 18; frame++)
        {
            stockSkip.Step(0);
            editedSkip.Step(0);
        }
        AssertEqual(stockSkip.Phase, editedSkip.Phase,
            "skip copyright palette override preserves title phase");
        AssertEqual(edited.TitlePalette.SkipCopyrightWhite,
            editedSkip.PaletteColors[TitleSequenceRomData.Palette.CopyrightWhiteIndex],
            "skip route uses edited copyright white");
        AssertEqual(edited.TitlePalette.SkipCopyrightRed,
            editedSkip.PaletteColors[TitleSequenceRomData.Palette.CopyrightRedIndex],
            "skip route uses edited copyright red");
        AssertTrue(!stockSkip.Render().AsSpan().SequenceEqual(editedSkip.Render()),
            "skip copyright palette override changes rendered title output");
        editedSkip.BindTitlePalette(original.TitlePalette);
        AssertEqual(original.TitlePalette.SkipCopyrightWhite,
            editedSkip.PaletteColors[TitleSequenceRomData.Palette.CopyrightWhiteIndex],
            "restored title uses current stock skip white");
        AssertEqual(original.TitlePalette.SkipCopyrightRed,
            editedSkip.PaletteColors[TitleSequenceRomData.Palette.CopyrightRedIndex],
            "restored title uses current stock skip red");
        editedSkip.BindTitlePalette(edited.TitlePalette);
        AssertEqual(edited.TitlePalette.SkipCopyrightWhite,
            editedSkip.PaletteColors[TitleSequenceRomData.Palette.CopyrightWhiteIndex],
            "live title rebind applies edited skip white");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing title palette override restores installed-content identity");
        Console.WriteLine("Title palette override: content identity and production title output change immediately, then restore exactly.");
    }

    /// <summary>Checks that replacement Mode 7 tile art changes the production title render.</summary>
    private static void VerifyTitleGraphicsOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string name = TitleGraphicsFormat.Mode7TilesFile;
        IndexedPngImage image;
        using (var input = File.OpenRead(Path.Combine(stock, name)))
            image = IndexedPng.Read(input, TitleGraphicsFormat.Mode7Width, TitleGraphicsFormat.Mode7Height);
        byte[] pixels = image.Pixels.Select(value => unchecked((byte)(value + 1))).ToArray();
        string replacement = Path.Combine(overrides, name);
        using (var output = File.Create(replacement))
            IndexedPng.Write(output, image.Width, image.Height, pixels, image.Palette);

        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "title graphics override changes installed-content identity");
        var stockTitle = new TitleSequenceState(bus,
            titleGradientPresentation: original.TitleGradient,
            titlePalettePresentation: original.TitlePalette,
            titleGraphicsPresentation: original.TitleGraphics);
        var editedTitle = new TitleSequenceState(bus,
            titleGradientPresentation: edited.TitleGradient,
            titlePalettePresentation: edited.TitlePalette,
            titleGraphicsPresentation: edited.TitleGraphics);
        stockTitle.Step((ushort)SuperMetroid.Core.Input.SnesButton.Start);
        editedTitle.Step((ushort)SuperMetroid.Core.Input.SnesButton.Start);
        for (int frame = 0; frame < 16; frame++)
        {
            stockTitle.Step(0);
            editedTitle.Step(0);
        }
        AssertTrue(!stockTitle.Render().AsSpan().SequenceEqual(editedTitle.Render()),
            "Mode 7 PNG override visibly changes production title output");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing title graphics override restores installed-content identity");
        Console.WriteLine("Title graphics override: content identity and production title output change immediately, then restore exactly.");
    }

    /// <summary>Checks title sprite-layout edits change OAM while preserving native sequence timing.</summary>
    private static void VerifyTitleSpriteCompositionOverride(
        ISnesAddressSpace bus,
        string stock,
        string overrides,
        AreaMapPresentationCatalog original)
    {
        string name = TitleGraphicsFormat.Mode7MapFile;
        TitleMode7MapDocument document = JsonSerializer.Deserialize<TitleMode7MapDocument>(
            File.ReadAllBytes(Path.Combine(stock, name)), MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Extracted title layout is null.");
        ushort yearPointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
            TitleSequenceRomData.TextSequences.Year.InstructionAddress + 6);
        TitleSpriteFrame year = document.Sprites.Single(frame => frame.Pointer == yearPointer);
        year.Parts[0] = year.Parts[0] with { OffsetX = year.Parts[0].OffsetX + 8 };
        string replacement = Path.Combine(overrides, name);
        using (var output = File.Create(replacement))
            TitleGraphicsPresentation.WriteMap(output, document);

        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "title sprite override changes installed-content identity");
        var stockTitle = new TitleSequenceState(bus,
            titleGradientPresentation: original.TitleGradient,
            titlePalettePresentation: original.TitlePalette,
            titleGraphicsPresentation: original.TitleGraphics);
        var editedTitle = new TitleSequenceState(bus,
            titleGradientPresentation: edited.TitleGradient,
            titlePalettePresentation: edited.TitlePalette,
            titleGraphicsPresentation: edited.TitleGraphics);
        bool changedOam = false;
        for (int frame = 0; frame < 100; frame++)
        {
            stockTitle.Step(0);
            editedTitle.Step(0);
            AssertEqual(stockTitle.Phase, editedTitle.Phase,
                "title sprite override preserves the native sequence phase");
            changedOam |= !stockTitle.CaptureRenderSnapshot().Memory.Oam.SequenceEqual(
                editedTitle.CaptureRenderSnapshot().Memory.Oam);
        }
        AssertTrue(changedOam, "title sprite override changes live title OAM");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing title sprite override restores installed-content identity");
        Console.WriteLine("Title sprite override: installed JSON changes live OAM without changing timing, then restores exactly.");
    }

    /// <summary>Checks current map-content rebinding and room startup against a bus that rejects map-ROM reads.</summary>
    private static void VerifyLiveMapCatalog(ISnesAddressSpace bus, AreaMapPresentationCatalog original,
        AreaMapPresentationCatalog edited, AreaMapCartridgeData[] rules,
        GameplayBasePaletteCatalog initialPalettes, MapPresentationInstalledRoomAssets fixtureAssets)
    {
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(bus);
        using var withoutContent = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(withoutContent, game);
        game.BindMapPresentation(original);
        using var withContent = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(withContent, game);
        AssertTrue(withoutContent.ToArray().AsSpan().SequenceEqual(withContent.ToArray()), "map resources do not enter debugger graph");
        withContent.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Frontend.SuperMetroidGame>(withContent);
        AssertTrue(restored.MapPresentationIdentity is null, "restored graph requires host content rebind");
        restored.BindMapPresentation(edited);
        AssertEqual(edited.ContentIdentity, restored.MapPresentationIdentity, "state rebind uses current override identity");

        var guard = new MapDataGuard(bus, rules);
        var runtime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(guard,
            initialPaletteArt: initialPalettes) { MapPresentation = edited };
        fixtureAssets.Bind(runtime);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        AssertEqual(edited.Get(AreaId.Ceres).GetTile(runtime.Hud.MinimapCenterX, runtime.Hud.MinimapCenterY).CharacterIndex,
            (ushort)(runtime.Hud.Tiles[60] & 0x3ff), "Ceres room-entry runtime consumes bound catalog without map ROM reads");
        var room = runtime.ActiveRoom!;
        var pause = new SuperMetroid.Core.Frontend.PauseMenuState(guard, runtime.Samus!, runtime.System,
            room.AreaIndex, room.MapX, room.MapY, mapPresentation: edited);
        ushort x = pause.MapHorizontalScroll, y = pause.MapVerticalScroll;
        pause.BindMapPresentation(original);
        AssertEqual(x, pause.MapHorizontalScroll, "pause content rebind retains horizontal scroll");
        AssertEqual(y, pause.MapVerticalScroll, "pause content rebind retains vertical scroll");
        _ = pause.Render();
        Suite(nameof(VerifyFileSelectMapCatalog), () => VerifyFileSelectMapCatalog(bus, guard, original, edited));
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(bus, guard, original, edited));
        Console.WriteLine("Live map catalog: Ceres room-entry/pause reject map ROM access; debugger rebind excludes stale content and preserves scroll.");
    }

    /// <summary>Checks file-select rendering, edited tilemap rebinding, and graphics-state restoration.</summary>
    private static void VerifyFileSelectMapCatalog(ISnesAddressSpace bus, ISnesAddressSpace guard,
        AreaMapPresentationCatalog original, AreaMapPresentationCatalog edited)
    {
        var system = new Bank80SystemState();
        system.LoadExploredMapBytes(Enumerable.Repeat((byte)255, 7 * 256).ToArray());
        var stock = new SuperMetroid.Core.Frontend.FileSelectRoomMapGraphics(bus, system,
            AreaId.Crateria, mapPresentation: original);
        var installed = new SuperMetroid.Core.Frontend.FileSelectRoomMapGraphics(guard, system, AreaId.Crateria,
            mapPresentation: original);
        AssertTrue(stock.RenderBackgrounds(0, 0).AsSpan().SequenceEqual(installed.RenderBackgrounds(0, 0)),
            "file-select installed stock renders identical pixels without map ROM reads");
        installed.BindMapPresentation(edited);
        byte[] expected = AreaMapTilemapBuilder.Build(edited.Get(AreaId.Crateria), system, MapTileWords.FileSelectUndownloadedBlank);
        byte[] unchanged = AreaMapTilemapBuilder.Build(original.Get(AreaId.Crateria), system, MapTileWords.FileSelectUndownloadedBlank);
        AssertTrue(!expected.AsSpan().SequenceEqual(unchanged), "override changes a visible file-select cell");
        for (int i = 0; i < expected.Length / 2; i++)
            AssertEqual(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(expected.AsSpan(i * 2)),
                installed.Vram.ReadWord(SuperMetroid.Core.Frontend.MenuPpuState.Bg1TilemapWord + i), "file-select rebind writes exact edited tilemap");
        using var snapshot = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(snapshot, installed);
        snapshot.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroid.Core.Frontend.FileSelectRoomMapGraphics>(snapshot);
        restored.BindMapPresentation(original);
        AssertTrue(stock.RenderBackgrounds(0, 0).AsSpan().SequenceEqual(restored.RenderBackgrounds(0, 0)),
            "restored file-select graphics accept current stock without stale override pixels");
        Console.WriteLine("File-select map: exact stock pixels, edited BG1 words, and graphics-state rebind pass.");
    }

    /// <summary>Address-space wrapper that rejects cartridge reads from map assets while forwarding unrelated access.</summary>
    /// <param name="source">Underlying address space used for permitted reads and writes.</param>
    /// <param name="maps">Cartridge map ranges that must remain unread during live presentation checks.</param>
    private sealed class MapDataGuard(ISnesAddressSpace source, AreaMapCartridgeData[] maps) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Throws when an address falls within a map or other installed-presentation source range.</summary>
        /// <param name="address">Bus address about to be read.</param>
        private void RejectMapSource(int address)
        {
            if ((address >= MapStaticPalettesRomData.PausePalette && address < MapStaticPalettesRomData.PausePalette + SnesCgram.ByteCount) ||
                (address >= SuperMetroid.Core.Frontend.FileSelectMapRomData.EntryPalette && address < SuperMetroid.Core.Frontend.FileSelectMapRomData.EntryPalette + SnesCgram.ByteCount) ||
                (address >= SuperMetroid.Core.Frontend.FileSelectMapRomData.PaletteColors && address < MapStaticPalettesRomData.WorldPaletteDataEnd))
                throw new InvalidOperationException($"Live presentation read static map palette ROM at {address:X6}.");
            if ((address >= SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteTiming &&
                address <= SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteTiming + SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteFrameCount * SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteTimingStride) ||
                (address >= SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteColors &&
                address < SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteColors + SuperMetroid.Core.Frontend.MapAnimationRomData.PaletteFrameCount * MapPaletteCycleFormat.ColorCount * 2))
                throw new InvalidOperationException($"Live presentation read map palette-cycle ROM at {address:X6}.");
            if (address >= HudTileAtlasFormat.SourceAddress && address < HudTileAtlasFormat.SourceAddress + HudTileAtlasFormat.TransferByteCount)
                throw new InvalidOperationException($"Live presentation read HUD atlas ROM at {address:X6}.");
            if (address >= MapTileAtlasFormat.SourceAddress && address < MapTileAtlasFormat.SourceAddress + MapTileAtlasFormat.ByteCount)
                throw new InvalidOperationException($"Live presentation read map atlas ROM at {address:X6}.");
            if ((address >= AreaMapRomData.TilemapPointerTable && address < AreaMapRomData.TilemapPointerTable + AreaIds.RetailCount * 3) ||
                (address >= AreaMapRomData.StationRevealMaskPointerTable && address < AreaMapRomData.StationRevealMaskPointerTable + AreaIds.RetailCount * 2) ||
                maps.Any(map => (address >= map.TilemapAddress && address < map.TilemapAddress + AreaMapRomData.TilemapByteCount) ||
                    (address >= map.StationRevealMaskAddress && address < map.StationRevealMaskAddress + AreaMapRomData.StationRevealMaskByteCount)))
                throw new InvalidOperationException($"Live presentation read map ROM at {address:X6}.");
        }

        /// <summary>Rejects forbidden presentation ranges, then forwards the ordinary address-space read.</summary>
        public byte ReadByte(int address)
        {
            RejectMapSource(address);
            return source.ReadByte(address);
        }

        /// <summary>Rejects forbidden presentation ranges, then forwards an import-capable cartridge read.</summary>
        public byte ReadCartridgeByte(int address)
        {
            RejectMapSource(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }

        /// <summary>Forwards a WRAM read without applying cartridge map-range checks.</summary>
        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Map-data guard source does not expose WRAM.")).ReadWorkRamByte(address);

        /// <summary>Forwards an SRAM read without applying cartridge map-range checks.</summary>
        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Map-data guard source does not expose SRAM.")).ReadSaveRamByte(address);

        /// <summary>Forwards a bus write to the wrapped address space.</summary>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
