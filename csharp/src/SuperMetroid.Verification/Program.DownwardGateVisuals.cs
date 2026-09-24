using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyDownwardGateVisuals()
    {
        RoomPlmDownwardGateVisualCatalog stock = RoomPlmDownwardGateVisualCatalog.Stock();
        RoomPlmDownwardGateVisualEntry[] entries = DownwardGatePlmDrawDefinitions.All
            .Select(list => new RoomPlmDownwardGateVisualEntry(
                DownwardGatePlmDrawDefinitions.VisualId(list.Pointer),
                list.Runs.Span.ToArray()
                    .Select(run => run.LevelWords.Span.ToArray()
                        .Select(word => new RoomLevelWord(word).VisualWord).ToArray())
                    .ToArray()))
            .ToArray();
        // The synthetic population's header begins at the opening list, which
        // selects the sixth resident column frame before its gate pre-instruction.
        RoomPlmDownwardGateVisualEntry column = entries.Single(entry =>
            entry.Id == "column-frame-5");
        column.Runs[0][1] = 0x0053;
        RoomPlmDownwardGateVisualEntry trigger = entries.Single(entry =>
            entry.Id == "blue-left-trigger");
        trigger.Runs[1][0] = 0x0053;
        var edited = new RoomPlmDownwardGateVisualCatalog(entries);
        column.Runs[0][1] = 0x0054;
        AssertEqual((ushort)0x0053, edited.GetWord(0xa55d, 0, 1),
            "gate visual catalog copies author data");
        AssertEqual((ushort)0x00ff, stock.GetWord(0xa55d, 0, 1),
            "stock gate visual retains the physical column art");

        (ushort physical, ushort streamed, ushort triggerPhysical, ushort triggerStreamed)
            Render(RoomPlmDownwardGateVisualCatalog visuals)
        {
            (TestAddressSpace bus, RoomLevelData level, BackgroundTilemapStreamer streamer,
                RoomPlmSystem plms, int gateBlockIndex) = CreateDownwardGateFixture(
                    DownwardGateTriggerBehavior.BlueLeft, visuals);
            StepDownwardGatePlm(plms, bus, level, streamer);
            AssertEqual(visuals.GetWord(0xa55d, 0, 1) == 0x0053
                    ? (ushort)0x0053 : (ushort)0x000f,
                streamer.BuildPlmLevelBlockUpdate(
                    gateBlockIndex + level.WidthInBlocks, 0).TopRow[0],
                "first gate PLM step draws its selected second-column visual word");
            StepDownwardGatePlm(plms, bus, level, streamer);
            int columnIndex = gateBlockIndex + level.WidthInBlocks;
            int triggerIndex = gateBlockIndex - 1;
            ushort streamed = level.CreateBackgroundStreamer()
                .BuildPlmLevelBlockUpdate(columnIndex, 0).TopRow[0];
            ushort triggerStreamed = level.CreateBackgroundStreamer()
                .BuildPlmLevelBlockUpdate(triggerIndex, 0).TopRow[0];
            return (level.GetCollisionBlockByIndex(columnIndex).LevelWord,
                streamed,
                level.GetCollisionBlockByIndex(triggerIndex).LevelWord,
                triggerStreamed);
        }

        var native = Render(stock);
        var changed = Render(edited);
        AssertEqual((ushort)0xc0ff, native.physical,
            "stock gate draw retains its physical second-column word");
        AssertEqual(native.physical, changed.physical,
            "gate block art cannot change column collision");
        AssertEqual(native.triggerPhysical, changed.triggerPhysical,
            "gate trigger art cannot change shot-block collision");
        AssertEqual((ushort)0x000f, native.streamed,
            "stock gate block streams its native visual tile");
        AssertEqual((ushort)0x0053, changed.streamed,
            "edited gate block art survives camera streaming");
        AssertEqual((ushort)0x0053, changed.triggerStreamed,
            "edited trigger art survives camera streaming");

        AssertThrows<InvalidDataException>(
            () => new RoomPlmDownwardGateVisualCatalog(entries.Skip(1)),
            "gate visual catalog rejects missing frames");
        column.Runs[0][1] = 0xf053;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmDownwardGateVisualCatalog(entries),
            "gate visual catalog rejects collision bits");
        column.Runs[0][1] = 0x0053;
        AssertThrows<InvalidDataException>(
            () => new RoomPlmDownwardGateVisualCatalog(entries.Select(entry =>
                entry.Id == "column-frame-5" ? entry with { Id = "unknown-frame" } : entry)),
            "gate visual catalog rejects unknown frame IDs");

        VerifyDownwardGateVisualInstallation();
    }

    private static void VerifyDownwardGateVisualInstallation()
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "downward-gate-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Gate visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
                Path.GetFullPath("Super Metroid.smc"));
            RoomPlmDownwardGateVisualFiles.Extract(rom,
                installation.RoomPlmDownwardGateVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmDownwardGateVisualFiles.ValidateStock(
                installation.RoomPlmDownwardGateVisualDirectory);
            AssertEqual((ushort)0x00ff,
                installation.LoadRoomPlmDownwardGateVisuals().GetWord(0xa55d, 0, 1),
                "installed gate stock matches the cartridge column art");

            string stockPath = Path.Combine(installation.RoomPlmDownwardGateVisualDirectory,
                RoomPlmDownwardGateVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted gate JSON is empty.");
            JsonNode column = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "column-frame-5")!;
            column["runs"]![0]![1] = 0x0053;
            Directory.CreateDirectory(installation.RoomPlmDownwardGateVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmDownwardGateVisualOverrideDirectory,
                RoomPlmDownwardGateVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmDownwardGateVisuals().GetWord(0xa55d, 0, 1),
                "gate visual override selects edited column art");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmDownwardGateVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmDownwardGateVisualFiles.Load(refreshed,
                    installation.RoomPlmDownwardGateVisualOverrideDirectory).GetWord(0xa55d, 0, 1),
                "gate visual override survives stock replacement");

            column["runs"]![0]![1] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmDownwardGateVisuals(),
                "gate override rejects collision bits instead of silently restoring stock");

            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmDownwardGateVisuals(),
                "gate stock manifest hash rejects corruption");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }
}
