using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyKraidRoomVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "kraid-room-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Kraid room visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmKraidVisualFiles.Extract(rom,
                installation.RoomPlmKraidVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmKraidVisualFiles.ValidateStock(
                installation.RoomPlmKraidVisualDirectory);
            RoomPlmKraidVisualCatalog stock =
                installation.LoadRoomPlmKraidVisuals();
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in
                     KraidRoomPlmDrawDefinitions.All)
            {
                for (int block = 0; block < draw.Runs.Span[0].LevelWords.Length;
                     block++)
                    AssertEqual(new RoomLevelWord(
                            draw.Runs.Span[0].LevelWords.Span[block]).VisualWord,
                        stock.GetWord(draw.Pointer, 0, block),
                        $"Kraid stock draw ${draw.Pointer:X4}, block {block}");
            }
            AssertThrows<InvalidDataException>(
                () => new RoomPlmKraidVisualCatalog([]),
                "Kraid visual catalog rejects missing frames");

            string stockPath = Path.Combine(
                installation.RoomPlmKraidVisualDirectory,
                RoomPlmKraidVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Kraid room JSON is empty.");
            JsonNode first = Entry("crumble-first");
            JsonNode ceiling = Entry("clear-ceiling");
            JsonNode spikes = Entry("clear-spikes");
            first["blocks"]![0] = 0x0058;
            ceiling["blocks"]![14] = 0x0059;
            spikes["blocks"]![21] = 0x005a;
            Directory.CreateDirectory(
                installation.RoomPlmKraidVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmKraidVisualOverrideDirectory,
                RoomPlmKraidVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmKraidVisualCatalog edited =
                installation.LoadRoomPlmKraidVisuals();
            AssertEqual((ushort)0x0058,
                edited.GetWord(KraidRoomPlmDrawDefinitions.CrumbleFirst, 0, 0),
                "Kraid override edits first crumble frame");
            AssertEqual((ushort)0x0059,
                edited.GetWord(KraidRoomPlmDrawDefinitions.ClearCeiling, 0, 14),
                "Kraid override edits far-right ceiling clear block");
            AssertEqual((ushort)0x005a,
                edited.GetWord(KraidRoomPlmDrawDefinitions.ClearSpikes, 0, 21),
                "Kraid override edits far-right spike clear block");
            VerifyKraidVisualOwnerIsolation(edited);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmKraidVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x005a,
                RoomPlmKraidVisualFiles.Load(refreshed,
                    installation.RoomPlmKraidVisualOverrideDirectory)
                    .GetWord(KraidRoomPlmDrawDefinitions.ClearSpikes, 0, 21),
                "Kraid override survives stock replacement");

            first["blocks"]![0] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmKraidVisuals(),
                "Kraid override cannot change physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmKraidVisualFiles.ValidateStock(
                    installation.RoomPlmKraidVisualDirectory),
                "tampered Kraid stock fails manifest validation");

            JsonNode Entry(string id) => document["entries"]!.AsArray()
                .Single(entry => entry!["id"]!.GetValue<string>() == id)!;
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Kraid room visuals: ten native draws, live ceiling/spike edits, elevatube owner isolation, physical/timing separation, stock repair and strict failures pass.");
    }

    private static void VerifyKraidVisualOwnerIsolation(
        RoomPlmKraidVisualCatalog edited)
    {
        var tube = new RoomPlmMaridiaElevatubeVisualCatalog(
            [new RoomPlmMaridiaElevatubeVisualEntry(
                MaridiaElevatubePlmDefinitions.VisualId, [0x005b])]);
        VerifyKraidDraw(RoomPlmHeaders.CrumbleKraidCeilingIntoBackground1,
            KraidRoomPlmDrawDefinitions.CrumbleFirst, 0,
            0x8180, 0x0058, edited, tube, layer1X: 0);
        VerifyKraidDraw(RoomPlmHeaders.ClearKraidCeiling,
            KraidRoomPlmDrawDefinitions.ClearCeiling, 14,
            0x0130, 0x0059, edited, tube, layer1X: 64);
        VerifyKraidDraw(RoomPlmHeaders.ClearKraidSpikes,
            KraidRoomPlmDrawDefinitions.ClearSpikes, 21,
            0x0110, 0x005a, edited, tube, layer1X: 192);
        VerifyKraidDraw(RoomPlmHeaders.CrumbleKraidCeilingIntoBackground1,
            KraidRoomPlmDrawDefinitions.CrumbleFirst, 0,
            0x8180, 0x0180, null, tube, layer1X: 0);

        RoomLevelData tubeLevel = CreateRoom(4, 4, new ushort[16], new byte[16],
            blockDefinitions: new byte[0x400 * 8]);
        tubeLevel.SetBlockDefinitionWord(0x005b * 4, 0x005b);
        var tubePlms = new RoomPlmSystem
        {
            KraidVisuals = edited,
            MaridiaElevatubeVisuals = tube,
        };
        AssertTrue(tubePlms.TrySpawnMaridiaElevatube(tubeLevel),
            "elevatube allocates alongside Kraid appearance resource");
        var tubeGuard = new KraidRoomSourceGuard(new TestAddressSpace());
        tubePlms.Step(tubeGuard, tubeLevel,
            tubeLevel.CreateBackgroundStreamer(), 0, 0, 0);
        int tubeBlock = tubeLevel.GetBlockIndex(
            MaridiaElevatubePlmRomData.BlockX,
            MaridiaElevatubePlmRomData.BlockY);
        AssertEqual((ushort)0x8180,
            tubeLevel.GetCollisionBlockByIndex(tubeBlock).LevelWord,
            "elevatube retains shared native physical word");
        AssertTrue(tubePlms.TilemapUpdates.Any(update =>
                update.BlockIndex == tubeBlock && update.TopRow[0] == 0x005b),
            "elevatube retains its own edited appearance, not Kraid's first frame");
        AssertEqual(0, tubeGuard.ForbiddenReadAttempts,
            "elevatube does not read migrated Kraid source data");
    }

    private static void VerifyKraidDraw(
        ushort header, ushort drawPointer, int blockOffset,
        ushort physicalWord, ushort visualWord,
        RoomPlmKraidVisualCatalog? kraid,
        RoomPlmMaridiaElevatubeVisualCatalog tube,
        ushort layer1X)
    {
        const int width = 32;
        ushort[] words = new ushort[width * 16];
        for (int x = 5; x <= 26; x++)
            words[5 * width + x] = 0x8123;
        RoomLevelData level = CreateRoom(width, 16, words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        foreach (ushort tile in new ushort[] { 0x0058, 0x0059, 0x005a })
            level.SetBlockDefinitionWord(tile * 4, tile);
        level.SetBlockDefinitionWord(0x0180 * 4, 0x0180);
        var plms = new RoomPlmSystem
        {
            KraidVisuals = kraid,
            MaridiaElevatubeVisuals = tube,
        };
        AssertTrue(plms.TrySpawnKraidRoomMutation(level, 5, 5, header),
            $"Kraid visual owner ${header:X4} allocates");
        var guard = new KraidRoomSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(guard, level, streamer, layer1X, 0, 0);
        int blockIndex = 5 * width + 5 + blockOffset;
        AssertEqual(physicalWord,
            level.GetCollisionBlockByIndex(blockIndex).LevelWord,
            $"Kraid draw ${drawPointer:X4} retains physical block {blockOffset}");
        AssertEqual(visualWord,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                blockIndex, 0).TopRow[0],
            $"Kraid draw ${drawPointer:X4} edit survives streaming");
        AssertTrue(plms.TilemapUpdates.Any(update =>
                update.BlockIndex == blockIndex &&
                update.TopRow[0] == visualWord),
            $"Kraid draw ${drawPointer:X4} uses owner-specific visible block {blockOffset}");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Kraid draw ${drawPointer:X4} reads no migrated source bytes");
    }
}
