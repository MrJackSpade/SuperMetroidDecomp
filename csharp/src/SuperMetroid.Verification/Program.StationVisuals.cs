using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyStationVisuals(SuperMetroidAddressSpace rom)
    {
        RoomPlmStationVisualCatalog stock = RoomPlmStationVisualCatalog.Stock();
        RoomPlmStationVisualEntry[] entries = RoomPlmStationDrawDefinitions.All
            .Select(list => new RoomPlmStationVisualEntry(
                RoomPlmStationDrawDefinitions.VisualId(list.Pointer),
                list.Runs.Span.ToArray()
                    .Select(run => run.LevelWords.Span.ToArray()
                        .Select(word => new RoomLevelWord(word).VisualWord).ToArray())
                    .ToArray()))
            .ToArray();
        RoomPlmStationVisualEntry first = entries.Single(entry => entry.Id == "map-frame-0");
        first.Runs[0][0] = 0x0053;
        var edited = new RoomPlmStationVisualCatalog(entries);
        first.Runs[0][0] = 0x0054;
        ushort pointer = StationAnimationProgramDefinitions.Resolve(
            StationAnimationProgramDefinitions.MapIdle, 0).DrawPointer;
        AssertEqual((ushort)0x0053, edited.GetWord(pointer, 0, 0),
            "station visual catalog copies author data");
        AssertEqual((ushort)0x010c, stock.GetWord(pointer, 0, 0),
            "stock station visual retains map frame zero");

        (ushort physical, ushort immediate, ushort streamed) Render(
            RoomPlmStationVisualCatalog visuals)
        {
            var bus = new TestAddressSpace();
            SeedRoomPlmPopulationRom(bus);
            const ushort population = 0x9500;
            bus.WriteBytes(0x8f0000 | population,
                [0xd3, 0xb6, 0x06, 0x06, 0x00, 0x00, 0x00, 0x00]);
            const int width = 16;
            const int block = 6 + 6 * width;
            var definitions = new byte[0x400 * 8];
            for (int tile = 0; tile < 4; tile++)
            {
                definitions[0x10c * 8 + tile * 2] = 0x0c;
                definitions[0x053 * 8 + tile * 2] = 0x53;
            }

            var level = new RoomLevelData(width, width,
                new ushort[width * width], new byte[width * width],
                new ushort[width * width], definitions);
            BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
            var plms = new RoomPlmSystem { StationVisuals = visuals };
            plms.LoadRoomPopulation(bus, level, streamer, new SnesVram(), population,
                new Bank80SystemState(), AreaId.Crateria,
                () => new SamusState(), () => false);
            plms.Step(bus, level, streamer, 0, 0, 0);
            AssertTrue(plms.TilemapUpdates.Count > 0,
                "station frame publishes an immediate PLM redraw");
            return (level.GetCollisionBlockByIndex(block).LevelWord,
                plms.TilemapUpdates[0].TopRow[0],
                level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(block, 0).TopRow[0]);
        }

        var native = Render(stock);
        var changed = Render(edited);
        AssertEqual((ushort)0x810c, native.physical,
            "stock map-station frame keeps its native physical level word");
        AssertEqual(native.physical, changed.physical,
            "station art cannot change collision or physical level data");
        AssertEqual((ushort)0x000c, native.immediate,
            "stock station visual reaches the immediate redraw");
        AssertEqual((ushort)0x0053, changed.immediate,
            "edited station visual reaches the immediate redraw");
        AssertEqual((ushort)0x0053, changed.streamed,
            "edited station visual survives later camera streaming");

        AssertThrows<InvalidDataException>(
            () => new RoomPlmStationVisualCatalog(entries.Skip(1)),
            "station visual catalog rejects missing frames");
        first.Runs[0][0] = 0xf053;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmStationVisualCatalog(entries),
            "station visual catalog rejects collision bits");
        first.Runs[0][0] = 0x0053;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmStationVisualCatalog(entries.Select(entry =>
                entry.Id == "map-frame-0" ? entry with { Id = "missing-frame" } : entry)),
            "station visual catalog rejects unknown frame IDs");

        var gameplayBus = new TestAddressSpace();
        SeedRoomPlmPopulationRom(gameplayBus);
        VerifyOtherStationFamilies(gameplayBus, edited);
        VerifyStationVisualInstallation(rom);
    }

    private static void VerifyStationVisualInstallation(SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "station-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Station visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmStationVisualFiles.Extract(rom,
                installation.RoomPlmStationVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmStationVisualFiles.ValidateStock(
                installation.RoomPlmStationVisualDirectory);
            ushort pointer = StationAnimationProgramDefinitions.Resolve(
                StationAnimationProgramDefinitions.MapIdle, 0).DrawPointer;
            AssertEqual((ushort)0x010c,
                installation.LoadRoomPlmStationVisuals().GetWord(pointer, 0, 0),
                "installed station stock matches native map art");

            string stockPath = Path.Combine(installation.RoomPlmStationVisualDirectory,
                RoomPlmStationVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted station JSON is empty.");
            JsonNode frame = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "map-frame-0")!;
            frame["runs"]![0]![0] = 0x0053;
            Directory.CreateDirectory(installation.RoomPlmStationVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmStationVisualOverrideDirectory,
                RoomPlmStationVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmStationVisuals().GetWord(pointer, 0, 0),
                "station visual override selects edited map frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmStationVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmStationVisualFiles.Load(refreshed,
                    installation.RoomPlmStationVisualOverrideDirectory).GetWord(pointer, 0, 0),
                "station visual override survives stock replacement");

            frame["runs"]![0]![0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmStationVisuals(),
                "station override rejects collision bits instead of silently restoring stock");

            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmStationVisuals(),
                "station stock manifest hash rejects corruption");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }
}
