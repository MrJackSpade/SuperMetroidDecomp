using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyTourianAccessVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tourian stock native oracle revision");
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "tourian-access-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Tourian access test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmTourianAccessVisualFiles.Extract(rom,
                installation.RoomPlmTourianAccessVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmTourianAccessVisualFiles.ValidateStock(
                installation.RoomPlmTourianAccessVisualDirectory);
            VerifyTourianAccessStockMapping(rom, installation.LoadRoomPlmTourianAccessVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmTourianAccessVisualDirectory,
                RoomPlmTourianAccessVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Tourian visual JSON is empty.");
            JsonNode firstFrame = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "crumble-frame-0")!;
            JsonNode clear = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "clear-six-rows")!;
            firstFrame["blocks"]![0] = 0x0058;
            clear["blocks"]![20] = 0x0058;
            Directory.CreateDirectory(
                installation.RoomPlmTourianAccessVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmTourianAccessVisualOverrideDirectory,
                RoomPlmTourianAccessVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmTourianAccessVisualCatalog edited =
                installation.LoadRoomPlmTourianAccessVisuals();
            AssertEqual((ushort)0x0058,
                edited.GetWord(TourianAccessPlmDrawDefinitions.CrumbleFirstPointer,
                    0, 0),
                "Tourian override edits the first crumble frame");
            AssertEqual((ushort)0x0058,
                edited.GetWord(TourianAccessPlmDrawDefinitions.ClearPointer, 5, 0),
                "Tourian override edits the last clear row independently");

            VerifyTourianAccessVisualSeparation(rom, edited, clear: false);
            VerifyTourianAccessVisualSeparation(rom, edited, clear: true);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmTourianAccessVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0058,
                RoomPlmTourianAccessVisualFiles.Load(refreshed,
                    installation.RoomPlmTourianAccessVisualOverrideDirectory)
                    .GetWord(TourianAccessPlmDrawDefinitions.ClearPointer, 5, 0),
                "Tourian override survives stock replacement");

            firstFrame["blocks"]![0] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmTourianAccessVisuals(),
                "Tourian override rejects physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmTourianAccessVisualFiles.ValidateStock(
                    installation.RoomPlmTourianAccessVisualDirectory),
                "tampered Tourian stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Tourian access visuals: five native layouts, live crumble/clear edits, physical isolation, stock repair and strict failures pass.");
    }

    private static void VerifyTourianAccessStockMapping(SuperMetroidAddressSpace rom,
        RoomPlmTourianAccessVisualCatalog installed)
    {
        (ushort Pointer, string Id)[] frames = [(0x9297,"crumble-empty-row"), (0x92a3,"crumble-frame-0"),
            (0x92af,"crumble-frame-1"), (0x92bb,"crumble-frame-2"), (0x92c7,"clear-six-rows")];
        var native = new Dictionary<ushort, ushort[]>();
        foreach (var frame in frames)
        {
            var words = new ushort[frame.Pointer == 0x92c7 ? 24 : 4];
            for (int index = 0; index < words.Length; index++)
                words[index] = (ushort)(ReadSamusEaterPlmWord(rom,
                    0x840000 | (frame.Pointer + index / 4 * 12 + index % 4 * 2 + 2)) & 0xfff);
            native.Add(frame.Pointer, words);
        }
        var entries = frames.Select(frame => new RoomPlmTourianAccessVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = RoomPlmTourianAccessVisualCatalog.Stock();
        var imported = new RoomPlmTourianAccessVisualCatalog(entries.Reverse());
        string Hash(Dictionary<ushort, ushort[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromWordFrames(
            nameof(RoomPlmTourianAccessVisualCatalog), words);
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "Tourian access original flattened hash");
            foreach (var frame in frames)
            {
                for (int index = 0; index < native[frame.Pointer].Length; index++)
                    AssertEqual(native[frame.Pointer][index], catalog.GetWord(frame.Pointer,index / 4,index % 4), "Tourian access native stock word");
                foreach (int bad in new[] {int.MinValue,-1,native[frame.Pointer].Length / 4,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,bad,0), "Tourian access run bounds");
                foreach (int bad in new[] {int.MinValue,-1,4,int.MaxValue})
                for (int run = 0; run < native[frame.Pointer].Length / 4; run++)
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(frame.Pointer,run,bad), "Tourian access block bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetWord((ushort)pointer,0,0), "Tourian access unknown pointer");
        entries[4].Blocks[23] = 0x0c58;
        entries[2].Blocks[0] = 0x0059;
        var mixed = new RoomPlmTourianAccessVisualCatalog(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "Tourian access mixed custom stock hash");
        entries[4].Blocks[23] = 0x005a;
        entries[1].Blocks[0] = 0x005b;
        foreach (var frame in frames)
        for (int index = 0; index < native[frame.Pointer].Length; index++)
            AssertEqual(expected[frame.Pointer][index], mixed.GetWord(frame.Pointer,index / 4,index % 4), "Tourian access clone isolation and stock fallback");
        AssertThrows<InvalidDataException>(() => new RoomPlmTourianAccessVisualCatalog(entries[..4]), "Tourian access missing frame");
        AssertThrows<InvalidDataException>(() => new RoomPlmTourianAccessVisualCatalog(entries.Append(entries[0])), "Tourian access duplicate frame");
        var invalid = entries.ToArray();
        invalid[0] = new("CRUMBLE-EMPTY-ROW", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new RoomPlmTourianAccessVisualCatalog(invalid), "Tourian access ordinal identity");
        invalid[0] = new(entries[0].Id, new ushort[3]);
        AssertThrows<InvalidDataException>(() => new RoomPlmTourianAccessVisualCatalog(invalid), "Tourian access exact frame shape");
        entries[0].Blocks[0] = 0xf058;
        AssertThrows<InvalidDataException>(() => new RoomPlmTourianAccessVisualCatalog(entries), "Tourian access visual bits only");
    }

    private static void VerifyTourianAccessVisualSeparation(
        SuperMetroidAddressSpace rom,
        RoomPlmTourianAccessVisualCatalog edited,
        bool clear)
    {
        const int width = 16;
        const int height = 20;
        int blockIndex = (clear ? 17 : 12) * width + 6;
        ushort[] words = new ushort[width * height];
        words[blockIndex] = 0x0123;
        byte[] definitions = new byte[0x400 * 8];
        definitions[0x0058 * 8] = 0x58;
        RoomLevelData level = CreateRoom(width, height, words,
            new byte[words.Length], blockDefinitions: definitions);
        var plms = new RoomPlmSystem { TourianAccessVisuals = edited };
        AssertTrue(plms.TrySpawnTourianAccess(level, clear),
            $"Tourian {(clear ? "clear" : "crumble")} real PLM allocates");
        var bus = new TourianAccessSourceGuard(rom);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        // The edited bottom clear row must be inside the camera to assert its
        // immediate VRAM update; the physical level mutation is world-relative.
        plms.Step(bus, level, streamer, 0, 12 * 16, 0);
        AssertEqual(clear ? (ushort)0x00ff : (ushort)0x0053,
            level.GetCollisionBlockByIndex(blockIndex).LevelWord,
            $"Tourian {(clear ? "clear" : "crumble")} edited art retains native physical word");
        AssertTrue(plms.TilemapUpdates.Any(update =>
                update.BlockIndex == blockIndex && update.TopRow[0] == 0x0058),
            $"Tourian {(clear ? "clear" : "crumble")} edited block reaches immediate redraw");
        AssertEqual((ushort)0x0058,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                blockIndex, 0).TopRow[0],
            $"Tourian {(clear ? "clear" : "crumble")} edit survives later streaming");
        AssertEqual(0, bus.ForbiddenReadAttempts,
            $"Tourian {(clear ? "clear" : "crumble")} edited draw reads no source bytes");
    }
}
