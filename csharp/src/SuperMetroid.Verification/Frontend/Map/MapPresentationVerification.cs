using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Samus;
using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Bosses.Ridley.CeresRidleyColorsVerification;
using static SuperMetroid.Verification.Bosses.Ridley.CeresRidleyMode7ColorsVerification;
using static SuperMetroid.Verification.Samus.Abilities.CrystalFlashColorsVerification;
using static SuperMetroid.Verification.Frontend.Ending.Artwork.EndingTextPresentationVerification;
using static SuperMetroid.Verification.Areas.Escape.Artwork.EscapeTimerPresentationVerification;
using static SuperMetroid.Verification.Areas.Escape.Artwork.EscapeTypewriterPresentationVerification;
using static SuperMetroid.Verification.Frontend.FileSelect.Artwork.FileSelectPresentationVerification;
using static SuperMetroid.Verification.Frontend.Options.Artwork.GameOptionsPresentationVerification;
using static SuperMetroid.Verification.Frontend.GameOver.GameOverMenuIndexesVerification;
using static SuperMetroid.Verification.Frontend.GameOver.Artwork.GameOverPresentationVerification;
using static SuperMetroid.Verification.Runtime.GameplayHudPresentationVerification;
using static SuperMetroid.Verification.Runtime.GameplayMessageNoticesVerification;
using static SuperMetroid.Verification.Runtime.GameplayMessagePanelsVerification;
using static SuperMetroid.Verification.Hud.HudAtlasVerification;
using static SuperMetroid.Verification.Assets.IndexedPngVerification;
using static SuperMetroid.Verification.Frontend.InstalledFileSelectMapVerification;
using static SuperMetroid.Verification.Frontend.Intro.Artwork.IntroNarrationPresentationVerification;
using static SuperMetroid.Verification.Frontend.Map.MapAtlasVerification;
using static SuperMetroid.Verification.Frontend.Map.MapLoadAnchorsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapMaskValidationVerification;
using static SuperMetroid.Verification.Frontend.Map.Artwork.MapPaletteCycleVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSaveMarkersVerification;
using static SuperMetroid.Verification.Frontend.Map.MapScreensVerification;
using static SuperMetroid.Verification.Frontend.Map.MapScrollControlsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Frontend.Map.Artwork.MapStaticPalettesVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainFakeDeathPaletteVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainRoomColorsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseBackdropsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentBaseAssetsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentLabelsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveTankAssetsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseReserveUiAssetsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseSelectorsVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseTileAtlasVerification;
using static SuperMetroid.Verification.Frontend.Pause.PauseWireframesVerification;
using static SuperMetroid.Verification.Samus.Palettes.PowerBombGlowColorsVerification;
using static SuperMetroid.Verification.Hardware.QueuedVramAssetsVerification;
using static SuperMetroid.Verification.Fx.RoomFxLayer3TilemapsVerification;
using static SuperMetroid.Verification.Fx.RoomFxPaletteBlendsVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusChargeColorsVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusFullBodyCycleColorsVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusHurtColorsVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusHyperBeamColorsVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusSuitColorsVerification;
using static SuperMetroid.Verification.Samus.Palettes.SamusVisorColorsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.FullRunSuitesVerification;
using static SuperMetroid.Verification.Frontend.Map.WorldMapLabelsVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Verification.Bosses.MotherBrain;

namespace SuperMetroid.Verification.Frontend.Map;

using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Frontend.Map;

using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
/// <summary>Checks the installed map and related presentation assets while keeping exploration rules and live gameplay reads separate from artwork.</summary>
internal static class MapPresentationVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Checks retail roundtrips and rejection of malformed map JSON and gameplay rules in artwork JSON.</summary>
    internal static void VerifyMapPresentation()
    {
        Suite(nameof(VerifyIndexedPng), () => VerifyIndexedPng());
        Suite(nameof(VerifyQueuedVramAssets), () => VerifyQueuedVramAssets());
        var rules = new PresentationMapRules();
        var cell = new AreaMapDocumentCell { Kind = MapCellKind.Room, Slope = MapCellSlope.None,
            Top = MapCellEdge.Wall, Bottom = MapCellEdge.Open, Left = MapCellEdge.Wall, Right = MapCellEdge.ElevatorShaft };
        var document = new MapPresentationDocument { Version = 2, Area = "Crateria",
            Cells = Enumerable.Repeat(cell, 64 * 32).ToArray() };
        string Json(MapPresentationDocument value) => JsonSerializer.Serialize(value,
            CamelCaseJson);
        AreaMapPresentationAsset Load(string text) => AreaMapPresentationFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes(text)), rules);

        AssertEqual(new MapCell(MapCellKind.Room, new(MapCellEdge.Wall, MapCellEdge.Open, MapCellEdge.Wall, MapCellEdge.ElevatorShaft)),
            Load(Json(document)).Cell(new(5, 5)), "a document cell reads as the cell it names");
        AssertThrows<InvalidDataException>(() => Load(Json(document with { Version = 1 })), "reject the character-numbered map schema");
        AssertThrows<InvalidDataException>(() => Load(Json(document with { Area = "Norfair" })), "reject wrong area");
        AssertThrows<InvalidDataException>(() => Load(Json(document with { Cells = [cell] })), "reject incomplete layout");
        AssertThrows<InvalidDataException>(() => Load(Json(document).Replace("\"kind\":\"Room\"", "\"kind\":\"Tower\"")), "reject an undefined cell kind");
        AssertThrows<InvalidDataException>(() => Load(Json(document).Replace("\"top\":\"Wall\"", "\"top\":7")), "reject an undefined edge");
        AssertThrows<InvalidDataException>(() => Load(Json(document).Replace("\"version\":2", "\"version\":2,\"revealEverything\":true")), "reject editable gameplay commands");
        if (File.Exists("Super Metroid.smc"))
        {
            var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
            foreach (AreaId area in AreaIds.All)
            {
                var stock = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, area);
                using var json = new MemoryStream();
                AreaMapPresentationFile.Write(json, stock);
                json.Position = 0;
                var restored = AreaMapPresentationFile.Read(json, stock);
                for (int y = 0; y < AreaMapSize.HeightInMapTiles; y++)
                for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
                {
                    AssertEqual(stock.Cell(new(checked((ushort)x), checked((ushort)y))), restored.Cell(new(checked((ushort)x), checked((ushort)y))), "stock map JSON retains each cell");
                    AssertEqual(stock.IsDiscoverable(new(checked((ushort)x), checked((ushort)y))), restored.IsDiscoverable(new(checked((ushort)x), checked((ushort)y))), "stock discovery rules survive import");
                    AssertEqual(stock.RevealsCellAbove(new(checked((ushort)x), checked((ushort)y))), restored.RevealsCellAbove(new(checked((ushort)x), checked((ushort)y))), "stock slope rules survive import");
                }
            }
            Console.WriteLine("Map presentation: all 14,336 retail cells round-trip exactly.");
        }
        else Console.WriteLine("Map presentation: retail round-trip skipped; source ROM unavailable.");
        Console.WriteLine("Map presentation: strict validation passes.");
    }

    /// <summary>Tiny fixed Crateria rule set the synthetic map documents of the validation checks load against.</summary>
    internal sealed class PresentationMapRules : IAreaMapView
    {
        /// <summary>Places the fixture's cells in Crateria so its projection uses a concrete area identity.</summary>
        public AreaId Area => AreaId.Crateria;
        /// <summary>Provides the empty cell for every fixture cell.</summary>
        public MapCell Cell(Position16<MapTiles> cell) => MapCell.Empty;
        /// <summary>Restricts exploration to the two fixture cells in column five.</summary>
        public bool IsDiscoverable(Position16<MapTiles> cell) => cell.X == 5 && cell.Y is 5 or 6;
        /// <summary>Reveals only the lower fixture cell when a map station has been used.</summary>
        public bool IsRevealedByMapStation(Position16<MapTiles> cell) => cell == new Position16<MapTiles>(5, 6);
        /// <summary>Gives the lower fixture cell the rule to reveal its cell above.</summary>
        public bool RevealsCellAbove(Position16<MapTiles> cell) => cell == new Position16<MapTiles>(5, 6);
    }

    /// <summary>
    /// Full installed presentation required by real-room checks. This is loaded
    /// once from the same installation as the maps, rather than silently allowing test
    /// runtimes to fall back to cartridge graphics when a room is initialized.
    /// </summary>
    internal sealed class MapPresentationInstalledRoomAssets
    {
        /// <summary>Installed room-specific character sheets bound before room initialization.</summary>
        private readonly RoomCharacterAtlases roomCharacters;

        /// <summary>The pre-coloured block pictures room loads draw from (#1392).</summary>
        private readonly InstalledRoomArtwork roomBlockSheets;
        /// <summary>Installed room visual layouts selected at room entry.</summary>
        private readonly RoomLevelArtworks visualLayouts;
        /// <summary>Installed room level streams supplying collision and BTS at room entry.</summary>
        private readonly InstalledRoomLevelStreams levelStreams;


        /// <summary>Installed elevator platform art used by the room PLM owner.</summary>
        private readonly InstalledRoomObjectElevatorPlatformVisuals elevatorPlatform;

        /// <summary>Enemy character and sprite art bound alongside room art.</summary>
        public ImportEnemyArtwork EnemyTiles { get; }
        /// <summary>Samus body art bound before gameplay drawing begins.</summary>
        public SamusBodySources SamusBody { get; }

        /// <summary>
        /// The repository installation's room artwork set, gathered once per process (#1532): the map checks bind only map
        /// presentation, so the room catalogs they bind are the shared installation's, read where the other fixtures read them.
        /// </summary>
        internal static MapPresentationInstalledRoomAssets Shared => shared.Value;

        /// <summary>Backing loader for <see cref="Shared"/>.</summary>
        private static readonly Lazy<MapPresentationInstalledRoomAssets> shared = new(() => new MapPresentationInstalledRoomAssets());

        /// <summary>Gathers the shared installation's room artwork set for the live-map checks.</summary>
        private MapPresentationInstalledRoomAssets()
        {
            roomCharacters = InstalledFixtures.RoomCharacters;
            roomBlockSheets = InstalledFixtures.RoomAssets.BlockSheets;
            visualLayouts = InstalledFixtures.RoomAssets.VisualLayouts;
            levelStreams = InstalledFixtures.RoomAssets.LevelStreams;

            EnemyTiles = InstalledFixtures.EnemyArtwork;

            SamusBody = InstalledFixtures.SamusBody;
            elevatorPlatform = RepositoryInstallation.Installation.LoadRoomObjectElevatorPlatformVisuals();
        }

        /// <summary>Installs all fixture artwork owners before Ceres room setup can request graphics.</summary>
        public void Bind(SuperMetroidRuntime runtime)
        {
            runtime.Artwork.RoomCharacterArt = roomCharacters;
            runtime.Artwork.RoomBlockSheets = roomBlockSheets;
            runtime.Artwork.RoomVisualLayouts = visualLayouts;
            runtime.Artwork.RoomLevelStreams = levelStreams;

            runtime.BossArtwork.KraidEyes = EnemyTiles.Bosses.KraidColors?.Eyes();

            runtime.Artwork.SamusBodyArt = SamusBody.Gameplay;
            runtime.ShotBlockVisuals.RoomObjectElevatorPlatformVisuals = elevatorPlatform;
        }
    }

    /// <summary>
    /// The repository installation's map presentation on a private copy, which the catalog check corrupts and restores. The
    /// copy holds the map presentation alone (#1532); the room fixtures the checks bind are read from the shared installation.
    /// </summary>
    private sealed class MapCatalogFixture : IDisposable
    {
        /// <summary>Private copy of the installation's map presentation the check may corrupt.</summary>
        private readonly PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(installation => installation.MapDirectory);

        /// <summary>Loads the catalog from the copy, which is the shared installation's byte for byte.</summary>
        internal MapCatalogFixture()
        {
            Bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            Stock = copy.Installation.MapDirectory;
            // The shared catalog the reload is compared with is the process's shared fixture, loaded (and timed) as one
            // before the copy is reloaded, so the reload is charged its own reading alone and not the shared load's first
            // use of the reading code (#1532).
            AreaMapPresentation shared = InstalledFixtures.Maps;
            Reopened = copy.Installation.LoadMaps();
            // The copy's files are the shared installation's, so its catalog is the shared one, which the fixture sheets and
            // pictures already compose and register (#1532).
            Original = Reopened.ContentIdentity == shared.ContentIdentity ? shared
                : throw new InvalidDataException("The private copy's map presentation differs from the shared installation's.");
        }

        /// <summary>The copy's map presentation, loaded again from its files before any corruption.</summary>
        internal AreaMapPresentation Reopened { get; }

        /// <summary>Retail cartridge oracle for native comparisons.</summary>
        internal SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace Bus { get; }
        /// <summary>The copy's installed map presentation directory.</summary>
        internal string Stock { get; }
        /// <summary>Map catalog loaded before any corruption.</summary>
        internal AreaMapPresentation Original { get; }
        /// <summary>Installed room fixtures bound by real-room checks, gathered when a check first binds them.</summary>
        internal static MapPresentationInstalledRoomAssets FixtureAssets => MapPresentationInstalledRoomAssets.Shared;

        /// <summary>Deletes the private installation copy.</summary>
        public void Dispose() => copy.Dispose();
    }

    /// <summary>Checks map catalog reload against the cartridge's exploration rules, live runtime consumers, the installed resources, and corrupt-file failures.</summary>
    private static void VerifyMapPresentationCatalog()
    {
        using var fixture = new MapCatalogFixture();
        var bus = fixture.Bus;
        string stock = fixture.Stock;
        AreaMapPresentation original = fixture.Original;
        var rules = AreaIds.All.ToDictionary(area => area, area => SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, area));
        AreaMapPresentation reopened = fixture.Reopened;
        foreach (AreaId area in AreaIds.All)
        for (int y = 0; y < AreaMapSize.HeightInMapTiles; y++)
        for (int x = 0; x < AreaMapSize.WidthInMapTiles; x++)
        {
            var actual = original.Get(area);
            AssertEqual(rules[area].Cell(new(checked((ushort)x), checked((ushort)y))), actual.Cell(new(checked((ushort)x), checked((ushort)y))), "ROM-free catalog retains stock cells");
            AssertEqual(rules[area].IsDiscoverable(new(checked((ushort)x), checked((ushort)y))), actual.IsDiscoverable(new(checked((ushort)x), checked((ushort)y))), "ROM-free stock discovery parity");
            AssertEqual(rules[area].IsRevealedByMapStation(new(checked((ushort)x), checked((ushort)y))), actual.IsRevealedByMapStation(new(checked((ushort)x), checked((ushort)y))), "ROM-free station mask parity");
            AssertEqual(rules[area].RevealsCellAbove(new(checked((ushort)x), checked((ushort)y))), actual.RevealsCellAbove(new(checked((ushort)x), checked((ushort)y))), "ROM-free stock slope parity");
        }
        AssertEqual(original.ContentIdentity, reopened.ContentIdentity, "map catalog identity stable across reload");
        Suite(nameof(VerifyLiveMapCatalog), () => VerifyLiveMapCatalog(original, MapCatalogFixture.FixtureAssets));
        Suite(nameof(VerifyMapAtlasIntegration), () => VerifyMapAtlasIntegration(bus, original));
        Suite(nameof(VerifyHudAtlasIntegration), () => VerifyHudAtlasIntegration(bus, original,
            MapCatalogFixture.FixtureAssets));
        Suite(nameof(VerifyMapPaletteCycleIntegration), () => VerifyMapPaletteCycleIntegration(bus, original));
        Suite(nameof(VerifyMapStaticPaletteIntegration), () => VerifyMapStaticPaletteIntegration(bus, original));
        Suite(nameof(VerifyWorldMapLabels), () => VerifyWorldMapLabels(bus, original));
        Suite(nameof(VerifyMapSaveMarkers), () => VerifyMapSaveMarkers(bus, original));
        VerifyStockMapImporterRefusesOverwrite(bus, stock);
        Suite(nameof(VerifyBundledMapMaskValidation), () => VerifyBundledMapMaskValidation(stock));
        string name = AreaMapCatalogFormat.FileName(AreaId.Crateria);
        string mapPath = Path.Combine(stock, name);
        byte[] installedMap = File.ReadAllBytes(mapPath);
        try
        {
            File.WriteAllText(mapPath, "corrupt map");
            AssertThrows<InvalidDataException>(() => MapPresentationFiles.Load(stock), "a corrupt installed map fails where it is decoded");
        }
        finally { File.WriteAllBytes(mapPath, installedMap); }
        VerifyInstallationHoldsNoImportColours(stock, original);
        Console.WriteLine("Map catalog: deterministic reload, ROM-free exploration parity, live consumers and corruption errors pass.");
    }

    /// <summary>
    /// Checks that the installation holds no colour document only the import reads (#1544, #1637): they are import data, the
    /// installed copy holds none of them, and the game's read of it still reads the same presentation.
    /// </summary>
    private static void VerifyInstallationHoldsNoImportColours(string stock, AreaMapPresentation original)
    {
        string[] importColours =
        [
            SuperMetroid.AssetExtraction.Frontend.Map.Artwork.MapStaticPalettesFormat.FileName,
            SuperMetroid.AssetExtraction.Frontend.Title.Artwork.TitlePaletteFormat.FileName,
            MotherBrainHealthPaletteFile.FileName,
            MotherBrainRoomColorFormat.FileName,
            SuperMetroid.AssetExtraction.Fx.Artwork.RoomFxPaletteBlendDefinitions.FileName,
            SuperMetroid.AssetExtraction.Samus.SamusVisorColorFile.FileName,
            SuperMetroid.AssetExtraction.Samus.SamusHurtColorFile.FileName,
            SuperMetroid.AssetExtraction.Samus.SamusSuitColorFile.FileName,
            SuperMetroid.AssetExtraction.Samus.SamusFullBodyCycleColorFormat.FileName,
            SuperMetroid.AssetExtraction.Samus.CrystalFlashColorFile.FileName,
            SuperMetroid.AssetExtraction.Samus.SamusChargeColorFormat.FileName,
            SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork.CeresRidleyColorFormat.FileName,
            SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork.CeresRidleyFloorColorFormat.FileName,
            SuperMetroid.AssetExtraction.Samus.SamusHyperBeamColorFormat.FileName,
            MotherBrainRainbowPaletteFile.FileName,
            SuperMetroid.AssetExtraction.Fx.Artwork.RoomPaletteFxPresentationFormat.FileName,
            // The highlight cycle's colours; the installed cycle holds each frame's hold alone (#1693).
            MapPaletteCycleFile.FileName,
        ];
        string importData = SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.MapDirectoryName);
        foreach (string name in importColours)
        {
            AssertTrue(!File.Exists(Path.Combine(stock, name)), $"the installation holds no {name}: it is import data");
            AssertTrue(File.Exists(Path.Combine(importData, name)), $"the import extracts {name} into its import data");
        }
        InstalledAreaMapPresentation game = MapPresentationFiles.Load(stock);
        AssertEqual(original.ContentIdentity, game.Game.ContentIdentity, "the game's read reads no colour document only the import reads");
        AssertTrue(game is not ImportAreaMapPresentation, "the game's read holds none of the import's colours");
    }

    /// <summary>Checks that the stock map importer refuses to extract into a directory that already holds a stock catalog.</summary>
    [PrivateInstallation("Runs MapPresentationExtractor into the copy's already-populated stock map directory to check that the importer refuses to overwrite existing stock files; reading the installed maps never runs the extractor.")]
    private static void VerifyStockMapImporterRefusesOverwrite(ISnesAddressSpace bus, string stock) =>
        AssertThrows<IOException>(() => SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationExtractor.Extract(bus, stock, "test-provenance"), "stock importer refuses overwrite");

    /// <summary>Checks the installed title, room FX, Samus, Ceres Ridley and Mother Brain colours against native output.</summary>
    private static void VerifyMapPresentationColors()
    {
        var bus = RetailBus();
        string stock = RepositoryInstallation.Installation.MapDirectory;
        // The colour documents (#1637) and the room-FX pages (#1638) only the import reads are import data: their checks read them where the import does.
        string importData = SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.MapDirectoryName);
        AreaMapPresentation original = InstalledFixtures.Maps;
        Suite(nameof(VerifyTitleSpriteSources), () => VerifyTitleSpriteSources(bus));
        Suite(nameof(VerifyRoomFxLayer3TilemapsRejectMalformedDocuments), () => VerifyRoomFxLayer3TilemapsRejectMalformedDocuments(importData));
        Suite(nameof(VerifyRoomFxPaletteBlendsRejectMalformedDocuments), () => VerifyRoomFxPaletteBlendsRejectMalformedDocuments(importData));
        Suite(nameof(VerifyPowerBombGlowColorsRejectMalformedDocuments), () => VerifyPowerBombGlowColorsRejectMalformedDocuments(stock));
        Suite(nameof(VerifySamusVisorColorsRejectMalformedDocuments), () => VerifySamusVisorColorsRejectMalformedDocuments(importData));
        Suite(nameof(VerifySamusHurtColorsRejectMalformedDocuments), () => VerifySamusHurtColorsRejectMalformedDocuments(importData));
        Suite(nameof(VerifySamusSuitColors), () => VerifySamusSuitColors(importData, original, bus, MapCatalogFixture.FixtureAssets));
        Suite(nameof(VerifySamusFullBodyCycleColors), () => VerifySamusFullBodyCycleColors(importData, original, bus));
        Suite(nameof(VerifyCrystalFlashColors), () => VerifyCrystalFlashColors(importData, original, bus));
        Suite(nameof(VerifySamusChargeColors), () => VerifySamusChargeColors(importData, original, bus));
        Suite(nameof(VerifyCeresRidleyColors), () => VerifyCeresRidleyColors(importData, original, bus,
            MapCatalogFixture.FixtureAssets));
        Suite(nameof(VerifyCeresRidleyMode7Colors), () => VerifyCeresRidleyMode7Colors(importData, original, bus));
        Suite(nameof(VerifySamusHyperBeamColorsRejectMalformedDocuments), () => VerifySamusHyperBeamColorsRejectMalformedDocuments(importData));
        Suite(nameof(VerifyMotherBrainHealthPalette), () => VerifyMotherBrainHealthPalette(bus, importData, original));
        Suite(nameof(VerifyMotherBrainRainbowPalette), () => VerifyMotherBrainRainbowPalette(bus, importData, stock, original));
        Suite(nameof(VerifyMotherBrainRoomColors), () => VerifyMotherBrainRoomColors(bus, importData, original));
    }

    /// <summary>Checks the installed pause backdrops and buttons, in a run of their own.</summary>
    private static void VerifyPauseBackdropAssets() =>
        VerifyPauseBackdrops(RetailBus(), InstalledFixtures.Maps);

    /// <summary>Checks the installed pause wireframes, in a run of their own.</summary>
    private static void VerifyPauseWireframeAssets() =>
        VerifyPauseWireframes(RetailBus(), InstalledFixtures.Maps);

    /// <summary>Checks installed HUD, menu, message, typewriter, narration, ending and file-select presentation assets.</summary>
    private static void VerifyFrontendPresentationAssets()
    {
        var bus = RetailBus();
        string stock = RepositoryInstallation.Installation.MapDirectory;
        AreaMapPresentation original = InstalledFixtures.Maps;
        Suite(nameof(VerifyEscapeTimerPresentationAssets), () => VerifyEscapeTimerPresentationAssets(bus));
        Suite(nameof(VerifyGameplayHudPresentationAssets), () => VerifyGameplayHudPresentationAssets(bus, original));
        Suite(nameof(VerifyGameOverPresentationAssets), () => VerifyGameOverPresentationAssets(bus, original));
        Suite(nameof(VerifyGameOverMenuNativeIndexes), () => VerifyGameOverMenuNativeIndexes(original));
        Suite(nameof(VerifyGameOptionsPresentationAssets), () => VerifyGameOptionsPresentationAssets(bus, original));
        Suite(nameof(VerifyFileSelectPresentationAssets), () => VerifyFileSelectPresentationAssets(bus, original));
        Suite(nameof(VerifyEscapeTypewriterAssets), () => VerifyEscapeTypewriterAssets(bus, stock));
        Suite(nameof(VerifyIntroNarrationAssets), () => VerifyIntroNarrationAssets(bus, stock));
        Suite(nameof(VerifyMapLoadAnchors), () => VerifyMapLoadAnchors(bus, original));
        Suite(nameof(VerifyCompiledMapScrollControls), () => VerifyCompiledMapScrollControls(bus, original));
    }

    /// <summary>The retail cartridge oracle for native comparisons.</summary>
    private static SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace RetailBus() =>
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

    /// <summary>
    /// Checks Mother Brain's rainbow, drain and fake-death colours, which are import data, and her installed beam colours
    /// against native CGRAM output, and malformed documents of both.
    /// </summary>
    private static void VerifyMotherBrainRainbowPalette(
        ISnesAddressSpace bus,
        string importColours,
        string stock,
        AreaMapPresentation original)
    {
        string source = Path.Combine(importColours, MotherBrainRainbowPaletteFile.FileName);
        MotherBrainRainbowPaletteDocument document =
            JsonSerializer.Deserialize<MotherBrainRainbowPaletteDocument>(
                File.ReadAllBytes(source), ArtworkJson.Options)
            ?? throw new InvalidDataException("Extracted Mother Brain rainbow palette is null.");
        InstalledMotherBrainRainbowPalette rainbow = InstalledFixtures.ImportMaps.MotherBrainRainbowColours;
        Suite(nameof(VerifyMotherBrainFakeDeathPalette), () => VerifyMotherBrainFakeDeathPalette(bus, rainbow));
        for (int frame = 0; frame < MotherBrainRainbowPaletteFormat.RainbowFrameCount; frame++)
        {
            var native = new SnesCgram();
            int pointer = MotherBrainSourceDefinitions.Palettes.RainbowPointerTable + frame * sizeof(ushort);
            int palette = MotherBrainSourceDefinitions.Palettes.PaletteBank |
                LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, pointer), CpuBusRegionRead.Read(bus, pointer + 1));
            SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(native, bus, palette, MotherBrainRainbowPaletteGeometry.ColorCount,
                MotherBrainColorDestinations.BodyColor);
            SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(native, bus, palette, MotherBrainRainbowPaletteGeometry.ColorCount,
                MotherBrainColorDestinations.BrainColor);
            SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(native, bus, palette + MotherBrainRainbowPaletteGeometry.ColorCount * sizeof(ushort),
                MotherBrainRainbowPaletteGeometry.ColorCount, MotherBrainColorDestinations.RearLegColor);
            SnesCgram installed = MotherBrainLook.OfRainbow(rainbow, MotherBrainShades.Rainbow(frame));
            AssertTrue(new CgramPalettes(native).ImageEquals(new CgramPalettes(installed)),
                $"Mother Brain rainbow frame {frame} matches all native CGRAM colors");
        }

        foreach (bool draining in new[] { true, false })
        for (int frame = 0; frame < MotherBrainRainbowPaletteGeometry.GreyFrameCount; frame++)
        {
            var native = new SnesCgram();
            var seed = new SnesCgram();
            // Revival writes thirteen colors and deliberately leaves the last two from
            // the preceding phase. Seed both CGRAM images to catch an overlong copy.
            native.SetColor(MotherBrainColorDestinations.BodyColor + 13, RgbColor.FromWord(0x1234));
            native.SetColor(MotherBrainColorDestinations.BodyColor + 14, RgbColor.FromWord(0x1235));
            native.SetColor(MotherBrainColorDestinations.BrainColor + 13, RgbColor.FromWord(0x2234));
            native.SetColor(MotherBrainColorDestinations.BrainColor + 14, RgbColor.FromWord(0x2235));
            seed.SetColor(MotherBrainColorDestinations.BodyColor + 13, RgbColor.FromWord(0x1234));
            seed.SetColor(MotherBrainColorDestinations.BodyColor + 14, RgbColor.FromWord(0x1235));
            seed.SetColor(MotherBrainColorDestinations.BrainColor + 13, RgbColor.FromWord(0x2234));
            seed.SetColor(MotherBrainColorDestinations.BrainColor + 14, RgbColor.FromWord(0x2235));
            int table = draining ? MotherBrainSourceDefinitions.Palettes.DrainedToGreyPointerTable :
                MotherBrainSourceDefinitions.Palettes.RevivalFromGreyPointerTable;
            int count = draining ? MotherBrainRainbowPaletteGeometry.DrainedColors :
                MotherBrainRainbowPaletteGeometry.RevivalColors;
            int pointer = table + frame * sizeof(ushort);
            int palette = MotherBrainSourceDefinitions.Palettes.PaletteBank |
                LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, pointer), CpuBusRegionRead.Read(bus, pointer + 1));
            SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(native, bus, palette, count, MotherBrainColorDestinations.BodyColor);
            SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(native, bus, palette, count, MotherBrainColorDestinations.BrainColor);
            SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(native, bus, palette + count * sizeof(ushort),
                MotherBrainRainbowPaletteGeometry.BackLegCount,
                MotherBrainColorDestinations.DrainedRearLegColor);
            SnesCgram installed = MotherBrainLook.OfRainbow(rainbow,
                draining ? MotherBrainShades.ToGrey(frame) : MotherBrainShades.FromGrey(frame), seed);
            AssertTrue(new CgramPalettes(native).ImageEquals(new CgramPalettes(installed)),
                $"Mother Brain {(draining ? "drain" : "revival")} frame {frame} retains exact native CGRAM and preserved colors");
        }

        var nativeNormal = new SnesCgram();
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(nativeNormal, bus, MotherBrainSourceDefinitions.Palettes.NormalBrainColors,
            MotherBrainRainbowPaletteGeometry.ColorCount, MotherBrainColorDestinations.BodyColor);
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(nativeNormal, bus, MotherBrainSourceDefinitions.Palettes.NormalBrainColors,
            MotherBrainRainbowPaletteGeometry.ColorCount, MotherBrainColorDestinations.BrainColor);
        SuperMetroid.AssetExtraction.Cartridge.CartridgePaletteImporter.LoadToCgram(nativeNormal, bus, MotherBrainSourceDefinitions.Palettes.NormalRearLegColors,
            MotherBrainRainbowPaletteGeometry.ColorCount, MotherBrainColorDestinations.RearLegColor);
        SnesCgram installedNormal = MotherBrainLook.OfRainbow(rainbow, MotherBrainShades.Restored);
        AssertTrue(new CgramPalettes(nativeNormal).ImageEquals(new CgramPalettes(installedNormal)),
            "Mother Brain normal restoration matches both fixed native sources");
        AssertEqual(MotherBrainSourceDefinitions.Beam.InitialColor,
            original.MotherBrainRainbowPalette.BeamInitialColor,
            "installed beam initial fixed color matches the cartridge");
        for (int frame = 0; frame < MotherBrainRainbowPaletteGeometry.BeamCycleColorCount; frame++)
        {
            int address = MotherBrainSourceDefinitions.Beam.ColorTable + frame * MotherBrainSourceDefinitions.Beam.ColorEntryBytes;
            ushort nativeBeamColor = LittleEndianBytes.Word(CpuBusRegionRead.Read(bus, address), CpuBusRegionRead.Read(bus, address + 1));
            AssertEqual(RgbColor.FromWord(nativeBeamColor), original.MotherBrainRainbowPalette.BeamColor(frame),
                $"installed beam HDMA color {frame} matches bank-$88");
        }
        AssertEqual(MotherBrainRainbowPaletteGeometry.BeamCycleColorCount, original.MotherBrainRainbowPalette.BeamCycleLength,
            "installed beam colour cycle ends where the native loop does");
        var nativeBeamSequence = new MotherBrainRainbowBeamEffect
        {
            // Source words were compared independently against bank $88 above;
            // playback itself must use the installed catalog on both paths.
            PresentationColors = original.MotherBrainRainbowPalette,
        };
        var installedBeamSequence = new MotherBrainRainbowBeamEffect
        {
            PresentationColors = original.MotherBrainRainbowPalette,
        };
        for (int frame = 0; frame < MotherBrainRainbowPaletteGeometry.BeamCycleColorCount + 3; frame++)
        {
            nativeBeamSequence.Step(true, new(100, 95), RotationAngle.QuarterTurn, RotationAngle.FromSteps(64));
            installedBeamSequence.Step(true, new(100, 95),
                RotationAngle.QuarterTurn, RotationAngle.FromSteps(64));
            AssertTrue(nativeBeamSequence.Color == installedBeamSequence.Color &&
                nativeBeamSequence.ColourStep == installedBeamSequence.ColourStep &&
                nativeBeamSequence.Geometry == installedBeamSequence.Geometry,
                $"installed beam frame {frame} matches native color, cursor, and geometry through loop reset");
        }

        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPaletteFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Rainbow = document.Rainbow.Take(9).ToArray(),
            }, ArtworkJson.Options))), "reject truncated Mother Brain rainbow loop");
        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPaletteFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                ToGrey = document.ToGrey.Select((item, index) => index == 0 ?
                    item with { TrailingColor = null } : item).ToArray(),
            }, ArtworkJson.Options))), "reject missing Mother Brain grey trailing color");
        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPaletteFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Normal = document.Normal with { Body = document.Normal.Body.Take(14).ToArray() },
            }, ArtworkJson.Options))), "reject truncated Mother Brain normal palette");
        MotherBrainBeamColourDocument beam = JsonSerializer.Deserialize<MotherBrainBeamColourDocument>(
                File.ReadAllBytes(Path.Combine(stock, MotherBrainBeamColourFile.FileName)), ArtworkJson.Options)
            ?? throw new InvalidDataException("Installed Mother Brain beam colours are null.");
        AssertThrows<InvalidDataException>(() => MotherBrainBeamColourFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(beam with { Cycle = [] }, ArtworkJson.Options))),
            "reject a Mother Brain beam without a cycle colour");
        AssertThrows<InvalidDataException>(() => MotherBrainBeamColourFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(beam with { First = beam.First with { Blue = 32 } }, ArtworkJson.Options))),
            "reject a Mother Brain beam colour channel outside 0..31");
        AssertThrows<InvalidDataException>(() => MotherBrainBeamColourFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(beam with { Version = MotherBrainBeamColourFile.Version + 1 }, ArtworkJson.Options))),
            "reject another Mother Brain beam colour version");
        AssertThrows<InvalidDataException>(() => MotherBrainRainbowPaletteFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                FakeDeathToGrey = (document.FakeDeathToGrey ?? throw new InvalidOperationException("Verification requires document.FakeDeathToGrey.")).Take(7).ToArray(),
            }, ArtworkJson.Options))), "reject truncated Mother Brain fake-death grey fade");
        Console.WriteLine("Mother Brain palette: 10 rainbow, 38 HDMA beam, 16 drain/revival and eight fake-death grey frames; native CGRAM parity, ROM-free fades and strict validation pass.");
    }

    /// <summary>Checks the installed Mother Brain health-threshold palettes against the native copies, and schema validation.</summary>
    private static void VerifyMotherBrainHealthPalette(
        ISnesAddressSpace bus,
        string stock,
        AreaMapPresentation original)
    {
        string source = Path.Combine(stock, MotherBrainHealthPaletteFile.FileName);
        MotherBrainHealthPaletteDocument document =
            JsonSerializer.Deserialize<MotherBrainHealthPaletteDocument>(
                File.ReadAllBytes(source), ArtworkJson.Options)
            ?? throw new InvalidDataException("Extracted Mother Brain health palette is null.");
        MotherBrainHealthPalettePresentation extracted =
            MotherBrainHealthPaletteFile.Read(new MemoryStream(
                SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainHealthPaletteExtractor.Extract(bus),
                writable: false));
        foreach (var (health, state) in new VerifyMotherBrainHealthPaletteCase[]
            { new VerifyMotherBrainHealthPaletteCase(36000, 0), new VerifyMotherBrainHealthPaletteCase(9000, 0), new VerifyMotherBrainHealthPaletteCase(8999, 1), new VerifyMotherBrainHealthPaletteCase(5400, 1), new VerifyMotherBrainHealthPaletteCase(5399, 2), new VerifyMotherBrainHealthPaletteCase(1800, 2), new VerifyMotherBrainHealthPaletteCase(1799, 3), new VerifyMotherBrainHealthPaletteCase(0, 3) })
        {
            int band = MotherBrainHealthPalette.Band(health);
            SnesCgram native = MotherBrainLook.OfHealth(extracted, band);
            SnesCgram installed = MotherBrainLook.OfHealth(InstalledFixtures.ImportOf(original).MotherBrainHealthPalette, band);
            AssertTrue(new CgramPalettes(native).ImageEquals(new CgramPalettes(installed)),
                $"Mother Brain health {health} stock colors exactly match the native three-copy output");
            AssertEqual(0, installed.Colors[MotherBrainColorDestinations.BrainColor - 1],
                "Mother Brain health palette leaves transparent color untouched");
            AssertEqual(0, installed.Colors[MotherBrainColorDestinations.RearLegColor +
                MotherBrainRainbowPaletteGeometry.ColorCount],
                "Mother Brain health palette leaves adjacent colors untouched");
            AssertEqual(state, health >= MotherBrainHealthPaletteDefinitions.FirstThreshold ? 0 :
                health >= MotherBrainHealthPaletteDefinitions.SecondThreshold ? 1 :
                health >= MotherBrainHealthPaletteDefinitions.FinalThreshold ? 2 : 3,
                "native health thresholds still choose the four damage states");
        }

        AssertThrows<InvalidDataException>(() => MotherBrainHealthPaletteFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Version = MotherBrainHealthPaletteFile.Version + 1,
            }, ArtworkJson.Options))), "reject unsupported Mother Brain palette version");
        AssertThrows<InvalidDataException>(() => MotherBrainHealthPaletteFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                Body = document.Body.Take(3).ToArray(),
            }, ArtworkJson.Options))), "reject incomplete Mother Brain damage states");
        AssertThrows<InvalidDataException>(() => MotherBrainHealthPaletteFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document with
            {
                BackLegs = document.BackLegs.Select((colors, index) => index == 0 ?
                    colors.Take(14).ToArray() : colors).ToArray(),
            }, ArtworkJson.Options))), "reject truncated Mother Brain rear-leg palette");

        Console.WriteLine("Mother Brain health palette: 8 threshold boundaries, ROM-free stock copies and schema checks pass.");
    }

    /// <summary>Checks that the year card's first letter is the first spritemap its native text list names.</summary>
    private static void VerifyTitleSpriteSources(ISnesAddressSpace bus)
    {
        ushort yearPointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
            TitleSourceDefinitions.TextLists[TitleSequencePhase.YearText] + 6);
        AssertEqual(yearPointer, SuperMetroid.AssetExtraction.Frontend.Title.TitleSpriteExtractor.MapSprites(bus)[TitleSprite.YearReveal1],
            "the year's first letter is the year card's first listed spritemap");
    }

    /// <summary>
    /// Checks debugger rebinding and live Ceres HUD, pause, and file-select consumers of the bound catalog. The runtime is
    /// built without a cartridge, so it cannot read map ROM.
    /// </summary>
    private static void VerifyLiveMapCatalog(AreaMapPresentation original, MapPresentationInstalledRoomAssets fixtureAssets)
    {
        var game = new SuperMetroid.Core.Frontend.SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore());
        game.BindArtwork(game.Artwork with
        {
            MapPresentation = original,
        });
        // The installed content, the map presentation among it, is named by path through the shared index, which
        // every round trip of the process reuses, instead of indexing all loaded content again for this game (#1532).
        var restored = SuperMetroid.Verification.Harness.GraphDigestVerification.RoundTrip(game);
        AssertEqual(original.ContentIdentity, restored.MapPresentationIdentity, "restored graph names its bound map content");

        var runtime =SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(GameplayAssists.None, InstalledFixtures.BareRuntimeArtwork);
        runtime.Artwork.MapPresentation = original;
        fixtureAssets.Bind(runtime);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        var room = runtime.Room.LoadedRoom;
        // The minimap's centre shows the map cell of the room screen Samus stands in, one row below the room's map row.
        Position16<MapTiles> samusCell = room.MapPosition + new Offset16<MapTiles>(
            RoomGridConversions.PixelToScreen(runtime.Player.IntroducedSamus.Position.X.Whole),
            RoomGridConversions.PixelToScreen(runtime.Player.IntroducedSamus.Position.Y.Whole) + 1);
        AssertTrue(runtime.Hud.Minimap[SuperMetroid.Core.Hud.Artwork.GameplayHudPresentation.MinimapSize.X + 2] is SuperMetroid.Core.Hud.MinimapMapCell center &&
            center.Cell == original.Get(AreaId.Ceres).Cell(samusCell), "Ceres room-entry runtime consumes bound catalog without map ROM reads");
        var pause = new SuperMetroid.Core.Frontend.Pause.PauseMenuState(new PauseMenuEntry { Samus = runtime.Player.IntroducedSamus, System = runtime.System, Area = room.Area, RoomMapPosition = room.MapPosition, MapPresentation = original, SpriteSheets = MenuSheetFixtures.For(original) });
        Position16<MapPixels> scroll = pause.MapScroll;
        pause.BindFixtureArtwork(original);
        AssertEqual(scroll, pause.MapScroll, "pause content rebind retains map scroll");
        _ = pause.Render();
        Suite(nameof(VerifyFileSelectMapCatalog), () => VerifyFileSelectMapCatalog(original));
        Suite(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(original));
        Console.WriteLine("Live map catalog: cartridge-free Ceres room entry and pause consume the bound catalog; debugger rebind preserves scroll.");
    }

    /// <summary>Checks stock file-select map pixels, the projected map cells, and restoration of the graphics state.</summary>
    private static void VerifyFileSelectMapCatalog(AreaMapPresentation original)
    {
        var system = new SharedGameState(new SharedRandom());
        system.Progress.ExploreEveryCell();
        var stock = new SuperMetroid.Core.Frontend.FileSelect.FileSelectRoomMapGraphics(system,
            AreaId.Crateria, mapPresentation: original, sheets: MenuSheetFixtures.For(original));
        var installed = new SuperMetroid.Core.Frontend.FileSelect.FileSelectRoomMapGraphics(system, AreaId.Crateria,
            mapPresentation: original, sheets: MenuSheetFixtures.For(original));
        AssertTrue(SuperMetroid.TestSupport.FrameRendering.ScenePixels.Same(stock.BackgroundsScene(new(0, 0)), installed.BackgroundsScene(new(0, 0))),
            "file-select installed stock renders identical pixels without map ROM reads");
        AssertTrue(AreaMapProjection.Project(original.Get(AreaId.Crateria), system).Cells.SequenceEqual(installed.AreaMap.Cells),
            "file-select shows the exact projected map cells");
        // The capture names the installed content by path, as a host's store does, instead of copying it (#1532).
        var restored = SuperMetroid.Verification.Harness.GraphDigestVerification.RoundTrip(installed);
        restored.BindFixtureArtwork(original);
        AssertTrue(SuperMetroid.TestSupport.FrameRendering.ScenePixels.Same(stock.BackgroundsScene(new(0, 0)), restored.BackgroundsScene(new(0, 0))),
            "restored file-select graphics keep the installed pixels");
        Console.WriteLine("File-select map: exact stock pixels, projected map cells, and graphics-state rebind pass.");
    }

    /// <summary>Provides an unbacked bus that rejects every read and write during installed map projection.</summary>
    internal static CartridgeReadGuard NewForbiddenMapBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Unexpected map ROM read {address:X6}.")
            .RejectWrites(_ => "Unexpected map bus write.");

    /// <summary>
    /// The map and installed presentation family. Only the catalog suite writes, into a private copy of the map
    /// presentation; the full run schedules every suite independently. Each pause-screen family (map screens and sprites,
    /// the tile atlas, the equipment rules, the selectors, the reserve tank and its interface, the equipment base and
    /// labels) is a suite of its own, as is the whole installed file-select menu pass (#1680).
    /// </summary>
    internal static RegisteredSuite[] MapPresentationSuites() =>
    [
        new(nameof(VerifyMapPresentation), () => VerifyMapPresentation()),
        new(nameof(VerifyMapPresentationCatalog), () => VerifyMapPresentationCatalog()),
        new(nameof(VerifyMapPresentationColors), () => VerifyMapPresentationColors()),
        new(nameof(VerifyMapScreens), () => VerifyMapScreens(InstalledFixtures.Maps)),
        new(nameof(VerifyMapSprites), () => VerifyMapSprites(RetailBus(), InstalledFixtures.Maps)),
        new(nameof(VerifyPauseTileAtlas), () => VerifyPauseTileAtlas(RetailBus(), InstalledFixtures.Maps)),
        new(nameof(VerifyCompiledPauseEquipmentRules), () => VerifyCompiledPauseEquipmentRules(RetailBus(), InstalledFixtures.Maps)),
        new(nameof(VerifyPauseSelectors), () => VerifyPauseSelectors(RetailBus(), InstalledFixtures.Maps)),
        new(nameof(VerifyPauseReserveTankAssets), () => VerifyPauseReserveTankAssets(RetailBus(), InstalledFixtures.Maps)),
        new(nameof(VerifyPauseReserveUiAssets), () => VerifyPauseReserveUiAssets(RetailBus(), InstalledFixtures.Maps)),
        new(nameof(VerifyPauseEquipmentBaseAssets), () => VerifyPauseEquipmentBaseAssets(RetailBus(), InstalledFixtures.Maps)),
        new(nameof(VerifyPauseEquipmentLabelAssets), () => VerifyPauseEquipmentLabelAssets(InstalledFixtures.Maps)),
        // Stronger than composing individual range guards: no bus read or write
        // is permitted anywhere in this complete installed saved-map lifecycle.
        new(nameof(VerifyInstalledFileSelectMenu), () => VerifyInstalledFileSelectMenu(InstalledFixtures.Maps, verifyCapturedRendering: true)),
        new(nameof(VerifyPauseBackdropAssets), () => VerifyPauseBackdropAssets()),
        new(nameof(VerifyPauseWireframeAssets), () => VerifyPauseWireframeAssets()),
        new(nameof(VerifyFrontendPresentationAssets), () => VerifyFrontendPresentationAssets()),
    ];

}
