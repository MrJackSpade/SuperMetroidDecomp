using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Areas.Escape.LookupEscapeGateStockVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmPopulationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mother Brain escape gate visual overrides against PLM collision and install contracts.</summary>
internal static class EscapeGateVisualsVerification
{
    /// <summary>Checks cloned closed and half-closed frames, rejected physical bits, and live and installed edits.</summary>
    internal static void VerifyEscapeGateVisuals(SuperMetroidAddressSpace rom)
    {
        RoomPlmEscapeGateVisualEntry[] entries =
            MotherBrainEscapeGatePlmDrawDefinitions.All
                .OrderBy(draw => draw.Pointer)
                .Select(draw => new RoomPlmEscapeGateVisualEntry(
                    MotherBrainEscapeGatePlmDrawDefinitions.VisualId(draw.Pointer),
                    draw.Runs.Span[0].LevelWords.Span
                        .SelectToArray(word => new RoomLevelWord(word).VisualWord)))
                .ToArray();
        RoomPlmEscapeGateVisualEntry closed = entries.Single(entry =>
            entry.Id == "closed");
        RoomPlmEscapeGateVisualEntry halfClosed = entries.Single(entry =>
            entry.Id == "half-closed");
        ushort stockClosed = closed.Blocks[0];
        ushort stockHalfClosed = halfClosed.Blocks[0];
        closed.Blocks[0] = 0x0053;
        halfClosed.Blocks[0] = 0x0054;
        var edited = new RoomPlmEscapeGateVisualCatalog(entries);
        closed.Blocks[0] = 0x0055;
        halfClosed.Blocks[0] = 0x0056;
        AssertEqual((ushort)0x0053,
            edited.GetWord((ushort)MotherBrainEscapeGateDraw.Closed, 0),
            "escape-gate catalog copies closed-frame author data");
        AssertEqual((ushort)0x0054,
            edited.GetWord((ushort)MotherBrainEscapeGateDraw.HalfClosed, 0),
            "escape-gate catalog copies transition-frame author data");
        AssertEqual(stockClosed,
            RoomPlmEscapeGateVisualCatalog.Stock().GetWord(
                (ushort)MotherBrainEscapeGateDraw.Closed, 0),
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
            RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing);
        SeedRoomPlmPopulation(bus, population,
        [
            0xca, 0xc8, gateX, gateY, 0x00, 0x80,
            0x00, 0x00,
        ]);
        var guarded = NewMotherBrainEscapeGateGuard(bus);
        byte[] blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x53 * 8] = 0x53;
        blockDefinitions[0x54 * 8] = 0x54;
        RoomLevelData CreateLevel() => new RoomLevelData(new(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), blockDefinitions);
        var system = new Bank80SystemState();

        RoomLevelData closedLevel = CreateLevel();
        BackgroundTilemapStreamer closedStreamer = closedLevel.CreateBackgroundStreamer();
        var closed = new RoomPlmSystem { Drawing = { EscapeGateVisuals = edited } };
        AssertEqual(1, closed.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, closedLevel, closedStreamer, new SnesVram(), system, AreaId.Tourian), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "edited escape gate loads its real resident header");
        closed.Step(guarded, closedLevel, closedStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual((ushort)0x830f,
            closedLevel.GetCollisionBlockByIndex(origin).LevelWord,
            "edited closed gate preserves native physical collision");
        AssertEqual((ushort)0x0053, closed.TilemapUpdates[0].TopRow[0],
            "edited closed gate reaches immediate visible tilemap update");
        AssertEqual((ushort)0x0053,
            closedLevel.CreateBackgroundStreamer()
                .BuildPlmLevelBlockUpdate(origin, 0).TopRow[0],
            "edited closed gate survives later camera streaming");

        RoomLevelData closingLevel = CreateLevel();
        BackgroundTilemapStreamer closingStreamer = closingLevel.CreateBackgroundStreamer();
        var closing = new RoomPlmSystem { Drawing = { EscapeGateVisuals = edited } };
        AssertEqual(1, closing.PopulationLoader.LoadRoomPopulation(new PlmRoom(guarded, closingLevel, closingStreamer, new SnesVram(), system, AreaId.Tourian), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "edited escape closure loads resident gate");
        var enteringDoor = new CartridgeDoorHeader(
            Pointer: 0xaa8c,
            DestinationRoomPointer: 0xde4d,
            Orientation: new(DoorDirection.Left, DoorClosingBehavior.EscapeGateCloses),
            PlmBlock: new(gateX, gateY),
            DestinationScreen: new(0, 0),
            SamusDistance: 0x8000,
            SetupCodePointer: DoorSetupCode.None);
        AssertTrue(closing.DoorPlms.TrySpawnDoorClosingPlm(guarded, closingLevel,
                enteringDoor, system),
            "edited escape closure selects the real secondary instruction list");
        for (int frame = 0; frame < 3; frame++)
            closing.Step(guarded, closingLevel, closingStreamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
        AssertEqual((ushort)0x830f,
            closingLevel.GetCollisionBlockByIndex(origin).LevelWord,
            "edited half-closed gate preserves native physical collision");
        AssertEqual((ushort)0x0054, closing.TilemapUpdates[0].TopRow[0],
            "edited half-closed gate reaches immediate visible tilemap update");
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
                    (ushort)MotherBrainEscapeGateDraw.Closed, 0),
                "installed escape-gate override changes selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmEscapeGateVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmEscapeGateVisualFiles.Load(refreshed,
                    installation.RoomPlmEscapeGateVisualOverrideDirectory).GetWord(
                    (ushort)MotherBrainEscapeGateDraw.Closed, 0),
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
