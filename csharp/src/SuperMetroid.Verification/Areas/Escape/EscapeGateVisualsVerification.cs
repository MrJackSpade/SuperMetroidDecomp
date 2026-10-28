using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Areas.Escape.LookupEscapeGateStockVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmPopulationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Checks Mother Brain escape gate visual overrides against PLM collision and install contracts.</summary>
internal static class EscapeGateVisualsVerification
{
    /// <summary>Checks cloned closed and half-closed frames, rejected physical bits, and live and installed edits.</summary>
    internal static void VerifyEscapeGateVisuals(SuperMetroidAddressSpace rom)
    {
        RoomPlmVisualEntry[] entries =
            MotherBrainEscapeGatePlmDrawDefinitions.All
                .OrderBy(draw => PlmDrawAddressCodec.Of(draw.Draw))
                .Select(draw => new RoomPlmVisualEntry(
                    MotherBrainEscapeGatePlmDrawDefinitions.VisualId(draw.Draw),
                    draw.Runs.Span[0].LevelWords.Span
                        .SelectToArray(word => new RoomLevelWord(word).VisualWord)))
                .ToArray();
        RoomPlmVisualEntry closed = entries.Single(entry =>
            entry.Id == "closed");
        RoomPlmVisualEntry halfClosed = entries.Single(entry =>
            entry.Id == "half-closed");
        ushort stockClosed = closed.Blocks[0];
        ushort stockHalfClosed = halfClosed.Blocks[0];
        closed.Blocks[0] = 0x0053;
        halfClosed.Blocks[0] = 0x0054;
        var edited = new RoomPlmEscapeGateVisualCatalog(entries);
        closed.Blocks[0] = 0x0055;
        halfClosed.Blocks[0] = 0x0056;
        AssertEqual((ushort)0x0053,
            edited.GetWord(PlmDraw.Of(MotherBrainEscapeGateDraw.Closed), 0),
            "escape-gate catalog copies closed-frame author data");
        AssertEqual((ushort)0x0054,
            edited.GetWord(PlmDraw.Of(MotherBrainEscapeGateDraw.HalfClosed), 0),
            "escape-gate catalog copies transition-frame author data");
        AssertEqual(stockClosed,
            RoomPlmEscapeGateVisualCatalog.Stock().GetWord(
                PlmDraw.Of(MotherBrainEscapeGateDraw.Closed), 0),
            "stock escape-gate catalog retains cartridge appearance");
        closed.Blocks[0] = stockClosed;
        halfClosed.Blocks[0] = stockHalfClosed;

        AssertThrows<InvalidDataException>(
            () => new RoomPlmEscapeGateVisualCatalog(entries.Skip(1)),
            "escape-gate catalog rejects a missing frame");
        closed.Blocks[0] = 0xf053;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmEscapeGateVisualCatalog(entries),
            "escape-gate catalog rejects collision bits in a visual word");
        closed.Blocks[0] = stockClosed;
        Suite(nameof(VerifyEscapeGateLiveVisual), () => VerifyEscapeGateLiveVisual(edited));
        Suite(nameof(VerifyEscapeGateVisualInstallation), () => VerifyEscapeGateVisualInstallation(rom));
        Console.WriteLine(
            "Escape-gate visuals: closed/closing edits preserve physical collision, installed overrides survive refresh, and invalid content fails loudly.");
    }

    /// <summary>Runs resident and door-triggered gates to prove edited tiles leave collision unchanged.</summary>
    private static void VerifyEscapeGateLiveVisual(
        RoomPlmEscapeGateVisualCatalog edited)
    {
        const int width = 16;
        const int gateX = 7;
        const int gateY = 2;
        const ushort population = 0x9000;
        int origin = gateY * width + gateX;
        var bus = new TestAddressSpace();
        // Header dispatch remains a cartridge read; only the gate's program and
        // draw payloads are compiled and therefore guarded below.
        SeedPlmProgramWords(bus, 0xc8ca, 0xb3c1,
            RoomPlmInstructionListsTooling.MotherBrainEscapeRoomGateClosed,
            RoomPlmInstructionListsConstants.MotherBrainEscapeRoomGateClosing);
        SeedRoomPlmPopulation(bus, population,
        [
            0xca, 0xc8, gateX, gateY, 0x00, 0x80,
            0x00, 0x00,
        ]);
        var guarded = NewMotherBrainEscapeGateGuard(bus);
        RoomLevelData CreateLevel() => new RoomLevelData(new(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]));
        var system = new SharedGameState();

        RoomLevelData closedLevel = CreateLevel();
        var closed = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmEscapeGateVisuals = edited });
        AssertEqual(1, PlmFixtures.LoadPopulation(ref closed, new PlmRoom(closedLevel, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Tourian), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "edited escape gate loads its real resident header");
        closed.Step(closedLevel);
        AssertEqual((ushort)0x830f,
            closedLevel.GetCollisionBlockByIndex(origin).PackedWord.Raw,
            "edited closed gate preserves native physical collision");
        AssertEqual((ushort)0x0053,
            closedLevel.DisplayedVisualWord(origin),
            "edited closed gate survives later camera streaming");

        RoomLevelData closingLevel = CreateLevel();
        var closing = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmEscapeGateVisuals = edited });
        AssertEqual(1, PlmFixtures.LoadPopulation(ref closing, new PlmRoom(closingLevel, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Tourian), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "edited escape closure loads resident gate");
        var enteringDoor = new DoorDefinition(
            Name: nameof(TourianDoorDefinitions.MotherBrain1),
            DestinationRoom: RoomHeaderDefinitions.TourianEscape1,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.EscapeGateCloses),
            PlmBlock: new(gateX, gateY),
            DestinationScreen: new(0, 0),
            SamusSpeed: DoorSamusSpeed.AxisDefault,
            SetupCode: DoorSetupCode.None);
        AssertTrue(closing.DoorPlms.TrySpawnDoorClosingPlm(closingLevel,
                enteringDoor, system),
            "edited escape closure selects the real secondary instruction list");
        for (int frame = 0; frame < 3; frame++)
            closing.Step(closingLevel);
        AssertEqual((ushort)0x830f,
            closingLevel.GetCollisionBlockByIndex(origin).PackedWord.Raw,
            "edited half-closed gate preserves native physical collision");
        AssertEqual((ushort)0x0054, closingLevel.DisplayedVisualWord(origin),
            "edited half-closed gate is shown");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "escape-gate visuals never reread compiled program or draw payloads");
    }

    /// <summary>Checks extraction, override reload after stock refresh, and tampered stock rejection.</summary>
    private static void VerifyEscapeGateVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("escape-gate-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmEscapeGateVisualFiles.Extract(rom,
                installation.RoomPlmEscapeGateVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmEscapeGateVisualFiles.ValidateStock(
                installation.RoomPlmEscapeGateVisualDirectory);
            VerifyEscapeGateStockMapping(rom, installation.LoadRoomPlmEscapeGateVisuals());
            string stockPath = Path.Combine(
                installation.RoomPlmEscapeGateVisualDirectory,
                RoomPlmEscapeGateVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted escape-gate JSON is empty.");
            JsonNode closed = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "closed");
            closed.Required("blocks")[0] = 0x0053;
            Directory.CreateDirectory(
                installation.RoomPlmEscapeGateVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmEscapeGateVisualOverrideDirectory,
                RoomPlmEscapeGateVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmEscapeGateVisuals().GetWord(
                    PlmDraw.Of(MotherBrainEscapeGateDraw.Closed), 0),
                "installed escape-gate override changes selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmEscapeGateVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmEscapeGateVisualFiles.Load(refreshed,
                    installation.RoomPlmEscapeGateVisualOverrideDirectory).GetWord(
                    PlmDraw.Of(MotherBrainEscapeGateDraw.Closed), 0),
                "escape-gate override survives stock replacement");
            closed.Required("blocks")[0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmEscapeGateVisuals(),
                "invalid escape-gate override fails loudly");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmEscapeGateVisualFiles.ValidateStock(
                    installation.RoomPlmEscapeGateVisualDirectory),
                "tampered escape-gate stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }
}
