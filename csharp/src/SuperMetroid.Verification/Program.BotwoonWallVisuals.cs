using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyBotwoonWallVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
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
        AssertTrue(plms.TilemapUpdates.Any(update =>
                update.BlockIndex == blockIndex &&
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
