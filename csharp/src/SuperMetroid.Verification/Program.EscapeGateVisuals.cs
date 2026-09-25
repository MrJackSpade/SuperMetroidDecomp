using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEscapeGateVisuals(SuperMetroidAddressSpace rom)
    {
        RoomPlmEscapeGateVisualEntry[] entries =
            MotherBrainEscapeGatePlmDrawDefinitions.All
                .OrderBy(draw => draw.Pointer)
                .Select(draw => new RoomPlmEscapeGateVisualEntry(
                    MotherBrainEscapeGatePlmDrawDefinitions.VisualId(draw.Pointer),
                    draw.Runs.Span[0].LevelWords.Span.ToArray()
                        .Select(word => new RoomLevelWord(word).VisualWord).ToArray()))
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
            edited.GetWord(MotherBrainEscapeGatePlmDrawDefinitions.Closed, 0),
            "escape-gate catalog copies closed-frame author data");
        AssertEqual((ushort)0x0054,
            edited.GetWord(MotherBrainEscapeGatePlmDrawDefinitions.HalfClosed, 0),
            "escape-gate catalog copies transition-frame author data");
        AssertEqual(stockClosed,
            RoomPlmEscapeGateVisualCatalog.Stock().GetWord(
                MotherBrainEscapeGatePlmDrawDefinitions.Closed, 0),
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
        VerifyEscapeGateLiveVisual(edited);
        VerifyEscapeGateVisualInstallation(rom);
        Console.WriteLine(
            "Escape-gate visuals: closed/closing edits preserve physical collision, installed overrides survive refresh, and invalid content fails loudly.");
    }

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
        WriteWord(bus, 0x84c8ca, 0xb3c1);
        WriteWord(bus, 0x84c8cc,
            RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosed);
        WriteWord(bus, 0x84c8ce,
            RoomPlmInstructionLists.MotherBrainEscapeRoomGateClosing);
        bus.WriteBytes(0x8f0000 | population,
        [
            0xca, 0xc8, gateX, gateY, 0x00, 0x80,
            0x00, 0x00,
        ]);
        var guarded = new MotherBrainEscapeGateReadGuard(bus);
        byte[] blockDefinitions = new byte[0x400 * 8];
        blockDefinitions[0x53 * 8] = 0x53;
        blockDefinitions[0x54 * 8] = 0x54;
        RoomLevelData CreateLevel() => new(width, width,
            new ushort[width * width], new byte[width * width],
            new ushort[width * width], blockDefinitions);
        var system = new Bank80SystemState();

        RoomLevelData closedLevel = CreateLevel();
        BackgroundTilemapStreamer closedStreamer = closedLevel.CreateBackgroundStreamer();
        var closed = new RoomPlmSystem { EscapeGateVisuals = edited };
        AssertEqual(1, closed.LoadRoomPopulation(guarded, closedLevel,
                closedStreamer, new SnesVram(), population, system, AreaId.Tourian,
                () => new SamusState(), () => false),
            "edited escape gate loads its real resident header");
        closed.Step(guarded, closedLevel, closedStreamer, 0, 0, 0);
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
        var closing = new RoomPlmSystem { EscapeGateVisuals = edited };
        AssertEqual(1, closing.LoadRoomPopulation(guarded, closingLevel,
                closingStreamer, new SnesVram(), population, system, AreaId.Tourian,
                () => new SamusState(), () => false),
            "edited escape closure loads resident gate");
        var enteringDoor = new CartridgeDoorHeader(
            Pointer: 0xaa8c,
            DestinationRoomPointer: 0xde4d,
            BitFlags: 0,
            Orientation: 9,
            PlmX: gateX,
            PlmY: gateY,
            DestinationScreenX: 0,
            DestinationScreenY: 0,
            SamusDistance: 0x8000,
            SetupCodePointer: 0);
        AssertTrue(closing.TrySpawnDoorClosingPlm(guarded, closingLevel,
                enteringDoor, system),
            "edited escape closure selects the real secondary instruction list");
        for (int frame = 0; frame < 3; frame++)
            closing.Step(guarded, closingLevel, closingStreamer, 0, 0, 0);
        AssertEqual((ushort)0x830f,
            closingLevel.GetCollisionBlockByIndex(origin).LevelWord,
            "edited half-closed gate preserves native physical collision");
        AssertEqual((ushort)0x0054, closing.TilemapUpdates[0].TopRow[0],
            "edited half-closed gate reaches immediate visible tilemap update");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "escape-gate visuals never reread compiled program or draw payloads");
    }

    private static void VerifyEscapeGateVisualInstallation(SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "escape-gate-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Escape-gate test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmEscapeGateVisualFiles.Extract(rom,
                installation.RoomPlmEscapeGateVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmEscapeGateVisualFiles.ValidateStock(
                installation.RoomPlmEscapeGateVisualDirectory);
            string stockPath = Path.Combine(
                installation.RoomPlmEscapeGateVisualDirectory,
                RoomPlmEscapeGateVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted escape-gate JSON is empty.");
            JsonNode closed = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "closed")!;
            closed["blocks"]![0] = 0x0053;
            Directory.CreateDirectory(
                installation.RoomPlmEscapeGateVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmEscapeGateVisualOverrideDirectory,
                RoomPlmEscapeGateVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmEscapeGateVisuals().GetWord(
                    MotherBrainEscapeGatePlmDrawDefinitions.Closed, 0),
                "installed escape-gate override changes selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmEscapeGateVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmEscapeGateVisualFiles.Load(refreshed,
                    installation.RoomPlmEscapeGateVisualOverrideDirectory).GetWord(
                    MotherBrainEscapeGatePlmDrawDefinitions.Closed, 0),
                "escape-gate override survives stock replacement");
            closed["blocks"]![0] = 0xf053;
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
