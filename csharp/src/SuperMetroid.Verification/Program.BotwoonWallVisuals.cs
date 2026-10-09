using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Checks that extracted, loaded, and custom Botwoon wall-clear art preserve the stock word mapping and content identity rules.</summary>
    /// <param name="rom">Retail address space used to read the native nine-word clear-wall spritemap.</param>
    private static void VerifyBotwoonWallStockMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] native = new ushort[9];
        for (int index = 0; index < native.Length; index++)
            native[index] = (ushort)(ReadBotwoonInstructionWord(rom, 0x849311 + 2 * index) & 0x0fff);
        var stock = RoomPlmBotwoonWallVisualCatalog.Stock();
        var loaded = new RoomPlmBotwoonWallVisualCatalog([new("clear-wall", native)]);
        string expectedIdentity = SuperMetroid.Core.Assets.SelectedPresentationHash.Create(
            nameof(RoomPlmBotwoonWallVisualCatalog), content => content.AppendWords("blocks", native));
        AssertEqual(expectedIdentity, stock.ContentIdentity, "Botwoon calculated stock preserves prior content identity");
        AssertEqual(expectedIdentity, loaded.ContentIdentity, "Botwoon loaded stock identity");
        ushort[] customWords = [0x10,0x20,0x30,0x58,0x50,0x60,0x70,0x80,0x90];
        var custom = new RoomPlmBotwoonWallVisualCatalog([new("clear-wall", customWords)]);
        string customIdentity = custom.ContentIdentity;
        for (int index = 0; index < native.Length; index++)
        {
            AssertEqual(native[index], stock.GetWord(0x930f, 0, index), "Botwoon stock appearance matches native");
            AssertEqual(native[index], loaded.GetWord(0x930f, 0, index), "Botwoon imported stock appearance matches native");
            AssertEqual(customWords[index], custom.GetWord(0x930f, 0, index), "Botwoon custom appearance preserved");
        }
        Array.Fill(customWords, (ushort)0);
        AssertEqual(customIdentity, custom.ContentIdentity, "Botwoon custom payload remains cloned");
        AssertTrue(customIdentity != expectedIdentity, "Botwoon custom content retains distinct identity");
        foreach (var catalog in new[] {stock, loaded, custom})
        {
            foreach (int index in new[] {int.MinValue,-1,9,int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetWord(0x930f, 0, index), "Botwoon visual block domain");
            foreach (int run in new[] {int.MinValue,-1,1,int.MaxValue})
                AssertThrows<InvalidDataException>(() => catalog.GetWord(0x930f, run, 0), "Botwoon visual run domain");
            foreach (ushort pointer in new ushort[] {0,0x930e,0x9310,0xffff})
                AssertThrows<InvalidDataException>(() => catalog.GetWord(pointer, 0, 0), "Botwoon visual pointer domain");
        }
        Suite(nameof(VerifyBotwoonWallVisualSeparation), () => VerifyBotwoonWallVisualSeparation(custom));
    }

    /// <summary>Confirms the clear-wall draw pointer is the only pointer with a stable exported visual identity.</summary>
    private static void VerifyBotwoonWallVisualIdMapping()
    {
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (pointer == 0x930f)
                AssertEqual("clear-wall", BotwoonWallPlmDrawDefinitions.VisualId((ushort)pointer), "Botwoon stable export identity");
            else
                AssertThrows<InvalidDataException>(() => BotwoonWallPlmDrawDefinitions.VisualId((ushort)pointer), "Botwoon visual ID accepts only the clear draw");
    }

    /// <summary>Exercises stock extraction, installation loading, art-only overrides, refresh persistence, and rejection of physical-bit edits.</summary>
    private static void VerifyBotwoonWallVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "botwoon-wall-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Botwoon wall visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmBotwoonWallVisualFiles.Extract(rom,
                installation.RoomPlmBotwoonWallVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmBotwoonWallVisualFiles.ValidateStock(
                installation.RoomPlmBotwoonWallVisualDirectory);
            RoomPlmBotwoonWallVisualCatalog stock =
                installation.LoadRoomPlmBotwoonWallVisuals();
            for (int block = 0; block < 9; block++)
                AssertEqual((ushort)0x00ff,
                    stock.GetWord(BotwoonWallPlmDrawDefinitions.ClearPointer,
                        0, block),
                    $"stock Botwoon wall clear block {block} matches ROM");
            AssertThrows<InvalidDataException>(
                () => new RoomPlmBotwoonWallVisualCatalog([]),
                "Botwoon wall visual catalog rejects missing clear frame");

            string stockPath = Path.Combine(
                installation.RoomPlmBotwoonWallVisualDirectory,
                RoomPlmBotwoonWallVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Botwoon wall JSON is empty.");
            JsonNode clear = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "clear-wall")!;
            clear["blocks"]![3] = 0x0058;
            Directory.CreateDirectory(
                installation.RoomPlmBotwoonWallVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmBotwoonWallVisualOverrideDirectory,
                RoomPlmBotwoonWallVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmBotwoonWallVisualCatalog edited =
                installation.LoadRoomPlmBotwoonWallVisuals();
            AssertEqual((ushort)0x0058,
                edited.GetWord(BotwoonWallPlmDrawDefinitions.ClearPointer, 0, 3),
                "Botwoon wall override edits the fourth clear block");
            VerifyBotwoonWallVisualSeparation(edited);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmBotwoonWallVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0058,
                RoomPlmBotwoonWallVisualFiles.Load(refreshed,
                    installation.RoomPlmBotwoonWallVisualOverrideDirectory)
                    .GetWord(BotwoonWallPlmDrawDefinitions.ClearPointer, 0, 3),
                "Botwoon wall override survives stock replacement");

            clear["blocks"]![3] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmBotwoonWallVisuals(),
                "Botwoon wall override cannot change physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmBotwoonWallVisualFiles.ValidateStock(
                    installation.RoomPlmBotwoonWallVisualDirectory),
                "tampered Botwoon wall stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Botwoon wall visuals: native nine-tile clear, live art edit, physical/timing isolation, stock repair and strict failures pass.");
    }

    /// <summary>Verifies an edited clear-wall tile reaches redraw and streaming while collision words and native deletion timing remain unchanged.</summary>
    /// <param name="edited">Loaded catalog supplying the custom clear-wall artwork.</param>
    private static void VerifyBotwoonWallVisualSeparation(
        RoomPlmBotwoonWallVisualCatalog edited)
    {
        const int width = 32;
        const int height = 16;
        ushort[] words = new ushort[width * height];
        for (int y = 4; y <= 12; y++)
            words[y * width + 15] = 0x0123;
        RoomLevelData level = CreateRoom(width, height, words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        level.SetBlockDefinitionWord(0x0058 * 4, 0x0058);
        var plms = new RoomPlmSystem { BotwoonWallVisuals = edited };
        AssertTrue(plms.TrySpawnBotwoonWall(level,
                RoomPlmHeaders.ClearBotwoonWall),
            "edited Botwoon wall-clear PLM allocates");
        var guard = new BotwoonWallSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(guard, level, streamer, 0, 0, 0);
        int blockIndex = 7 * width + 15;
        AssertEqual((ushort)0x00ff,
            level.GetCollisionBlockByIndex(blockIndex).LevelWord,
            "edited Botwoon wall art retains the physical clear word");
        // A PLM redraw identifies its block by the BG1 ring destination it targets.
        ushort blockDestination = streamer.BuildPlmLevelBlockUpdate(blockIndex, 0).TopRowDestination;
        AssertTrue(plms.TilemapUpdates.Any(update =>
                update.TopRowDestination == blockDestination &&
                update.TopRow[0] == 0x0058),
            "Botwoon wall edit reaches immediate redraw");
        AssertEqual((ushort)0x0058,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                blockIndex, 0).TopRow[0],
            "Botwoon wall edit survives background streaming");
        plms.Step(guard, level, streamer, 0, 0, 0);
        AssertEqual(0, plms.ActiveCount,
            "edited Botwoon wall preserves native deletion timing");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "edited Botwoon wall draw reads no source bytes");
    }
}
