using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyElevatorPlatformVisuals()
    {
        RoomPlmElevatorPlatformVisualCatalog stock =
            RoomPlmElevatorPlatformVisualCatalog.Stock();
        RoomPlmElevatorPlatformVisualEntry[] entries =
            ElevatorPlatformPlmDefinitions.DrawLists.Select(list =>
                new RoomPlmElevatorPlatformVisualEntry(
                    ElevatorPlatformPlmDefinitions.VisualId(list.Pointer),
                    list.Runs.Span.ToArray().Select(run =>
                        run.LevelWords.Span.ToArray().Select(word =>
                            new RoomLevelWord(word).VisualWord).ToArray()).ToArray()))
                .ToArray();
        RoomPlmElevatorPlatformVisualEntry first = entries.Single(entry =>
            entry.Id == "first-frame");
        first.Runs[0][0] = 0x0053;
        var edited = new RoomPlmElevatorPlatformVisualCatalog(entries);
        first.Runs[0][0] = 0x0054;
        AssertEqual((ushort)0x0053,
            edited.GetWord(ElevatorPlatformPlmDefinitions.FirstDraw, 0, 0),
            "elevator visual catalog copies author data");
        AssertEqual((ushort)0x0085,
            stock.GetWord(ElevatorPlatformPlmDefinitions.FirstDraw, 0, 0),
            "stock elevator visual retains native block reference");

        (ushort physical, ushort rendered) Render(RoomPlmElevatorPlatformVisualCatalog visuals)
        {
            var bus = new TestAddressSpace();
            const ushort population = 0x9400;
            bus.WriteBytes(0x8f0000 | population,
                [0x0b, 0xb7, 0x0c, 0x08, 0x00, 0x00, 0x00, 0x00]);
            WriteWord(bus, 0x84b70d, ElevatorPlatformPlmDefinitions.InstructionLoop);
            var definitions = new byte[0x400 * 8];
            for (int tile = 0; tile < 4; tile++)
            {
                definitions[0x085 * 8 + tile * 2] = 0x15;
                definitions[0x053 * 8 + tile * 2] = 0x53;
            }
            RoomLevelData level = CreateRoom(32, 16, new ushort[32 * 16],
                new byte[32 * 16], blockDefinitions: definitions);
            BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
            var plms = new RoomPlmSystem { ElevatorPlatformVisuals = visuals };
            AssertEqual(1, plms.LoadRoomPopulation(bus, level, streamer,
                new SnesVram(), RoomPlmPopulationImporter.Read(bus, population), new Bank80SystemState(), AreaId.Crateria,
                getSamus: () => null, isAreaTorizoDefeated: () => false),
                "elevator visual fixture loads one real PLM");
            plms.Step(bus, level, streamer, 0, 0, 0);
            int index = Enumerable.Range(0, 32 * 16).Single(block =>
                level.GetCollisionBlockByIndex(block).LevelWord == 0x8085);
            return (level.GetCollisionBlockByIndex(index).LevelWord,
                level.CreateBackgroundStreamer()
                    .BuildPlmLevelBlockUpdate(index, 0).TopRow[0]);
        }

        var native = Render(stock);
        var changed = Render(edited);
        AssertEqual((ushort)0x8085, native.physical,
            "stock elevator keeps cartridge physical block");
        AssertEqual(native.physical, changed.physical,
            "edited elevator art cannot change platform collision");
        AssertEqual((ushort)0x0015, native.rendered,
            "stock elevator streams its native tile");
        AssertEqual((ushort)0x0053, changed.rendered,
            "edited elevator art survives camera streaming");

        AssertThrows<InvalidDataException>(
            () => new RoomPlmElevatorPlatformVisualCatalog(entries.Skip(1)),
            "elevator visual catalog rejects missing frames");
        first.Runs[0][0] = 0xf053;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmElevatorPlatformVisualCatalog(entries),
            "elevator visual catalog rejects collision bits");
        first.Runs[0][0] = 0x0053;
        VerifyElevatorPlatformVisualInstallation();
    }

    private static void VerifyElevatorPlatformVisualInstallation()
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "elevator-platform-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Elevator visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
                Path.GetFullPath("Super Metroid.smc"));
            RoomPlmElevatorPlatformVisualFiles.Extract(rom,
                installation.RoomPlmElevatorPlatformVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmElevatorPlatformVisualFiles.ValidateStock(
                installation.RoomPlmElevatorPlatformVisualDirectory);
            AssertEqual((ushort)0x0085,
                installation.LoadRoomPlmElevatorPlatformVisuals().GetWord(
                    ElevatorPlatformPlmDefinitions.FirstDraw, 0, 0),
                "installed elevator stock matches the cartridge frame");

            string stockPath = Path.Combine(
                installation.RoomPlmElevatorPlatformVisualDirectory,
                RoomPlmElevatorPlatformVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted elevator JSON is empty.");
            JsonNode first = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "first-frame")!;
            first["runs"]![0]![0] = 0x0053;
            Directory.CreateDirectory(
                installation.RoomPlmElevatorPlatformVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmElevatorPlatformVisualOverrideDirectory,
                RoomPlmElevatorPlatformVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmElevatorPlatformVisuals().GetWord(
                    ElevatorPlatformPlmDefinitions.FirstDraw, 0, 0),
                "elevator override selects edited frame art");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmElevatorPlatformVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmElevatorPlatformVisualFiles.Load(refreshed,
                    installation.RoomPlmElevatorPlatformVisualOverrideDirectory).GetWord(
                        ElevatorPlatformPlmDefinitions.FirstDraw, 0, 0),
                "elevator override survives stock replacement");

            first["runs"]![0]![0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmElevatorPlatformVisuals(),
                "elevator override rejects collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmElevatorPlatformVisuals(),
                "elevator stock manifest rejects corruption");
        }
        finally
        {
            if (Directory.Exists(testRoot)) Directory.Delete(testRoot, recursive: true);
        }
    }
}
