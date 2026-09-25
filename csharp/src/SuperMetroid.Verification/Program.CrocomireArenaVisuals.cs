using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyCrocomireArenaVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "crocomire-arena-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Crocomire visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmCrocomireVisualFiles.Extract(rom,
                installation.RoomPlmCrocomireVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmCrocomireVisualFiles.ValidateStock(
                installation.RoomPlmCrocomireVisualDirectory);
            RoomPlmCrocomireVisualCatalog stock =
                installation.LoadRoomPlmCrocomireVisuals();
            foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in
                     CrocomireArenaPlmDrawDefinitions.All)
            for (int run = 0; run < draw.Runs.Length; run++)
            for (int block = 0; block < draw.Runs.Span[run].LevelWords.Length; block++)
                AssertEqual(new RoomLevelWord(
                        draw.Runs.Span[run].LevelWords.Span[block]).VisualWord,
                    stock.GetWord(draw.Pointer, run, block),
                    $"Crocomire stock draw ${draw.Pointer:X4} run {run} block {block}");
            AssertThrows<InvalidDataException>(
                () => new RoomPlmCrocomireVisualCatalog([]),
                "Crocomire visual catalog rejects missing draws");

            string stockPath = Path.Combine(
                installation.RoomPlmCrocomireVisualDirectory,
                RoomPlmCrocomireVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Crocomire JSON is empty.");
            Entry("clear-bridge")["blocks"]![9] = 0x0058;
            Entry("create-invisible-wall")["blocks"]![23] = 0x0059;
            Directory.CreateDirectory(
                installation.RoomPlmCrocomireVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmCrocomireVisualOverrideDirectory,
                RoomPlmCrocomireVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmCrocomireVisualCatalog edited =
                installation.LoadRoomPlmCrocomireVisuals();
            AssertEqual((ushort)0x0058,
                edited.GetWord(CrocomireArenaPlmDrawDefinitions.ClearBridge, 0, 9),
                "Crocomire override edits far bridge block");
            AssertEqual((ushort)0x0059,
                edited.GetWord(CrocomireArenaPlmDrawDefinitions.CreateInvisibleWall, 2, 7),
                "Crocomire override edits final wall column");
            VerifyCrocomireVisualDraw(RoomPlmHeaders.ClearCrocomireBridge,
                9, 0, 0x0080, 0x0058, edited);
            VerifyCrocomireVisualDraw(RoomPlmHeaders.CreateCrocomireInvisibleWall,
                2, 7, 0x8080, 0x0059, edited);
            VerifyCrocomireVisualDraw(RoomPlmHeaders.ClearCrocomireBridge,
                9, 0, 0x0080, 0x0080, null);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmCrocomireVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0059,
                RoomPlmCrocomireVisualFiles.Load(refreshed,
                    installation.RoomPlmCrocomireVisualOverrideDirectory)
                    .GetWord(CrocomireArenaPlmDrawDefinitions.CreateInvisibleWall,
                        2, 7),
                "Crocomire override survives stock replacement");

            Entry("clear-bridge")["blocks"]![9] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmCrocomireVisuals(),
                "Crocomire override cannot alter physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmCrocomireVisualFiles.ValidateStock(
                    installation.RoomPlmCrocomireVisualDirectory),
                "tampered Crocomire stock fails manifest validation");

            JsonNode Entry(string id) => document["entries"]!.AsArray()
                .Single(entry => entry!["id"]!.GetValue<string>() == id)!;
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine("Crocomire arena visuals: five stock draws, bridge/wall edits, physical isolation, override persistence and strict failures pass.");
    }

    private static void VerifyCrocomireVisualDraw(ushort header,
        int xOffset, int yOffset, ushort physicalWord, ushort visualWord,
        RoomPlmCrocomireVisualCatalog? visuals)
    {
        const int width = 32;
        ushort[] words = Enumerable.Repeat((ushort)0x8123, width * 16).ToArray();
        RoomLevelData level = CreateRoom(width, 16, words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        level.SetBlockDefinitionWord(0x0058 * 4, 0x0058);
        level.SetBlockDefinitionWord(0x0059 * 4, 0x0059);
        level.SetBlockDefinitionWord(0x0080 * 4, 0x0080);
        var plms = new RoomPlmSystem { CrocomireVisuals = visuals };
        AssertTrue(plms.TrySpawnCrocomireArenaMutation(level, 5, 3, header),
            $"Crocomire visual header ${header:X4} allocates");
        var guard = new CrocomireSourceGuard(new TestAddressSpace());
        plms.Step(guard, level, level.CreateBackgroundStreamer(), 0, 0, 0);
        int blockIndex = (3 + yOffset) * width + 5 + xOffset;
        AssertEqual(physicalWord,
            level.GetCollisionBlockByIndex(blockIndex).LevelWord,
            $"Crocomire visual header ${header:X4} retains physical block");
        AssertTrue(plms.TilemapUpdates.Any(update =>
                update.BlockIndex == blockIndex &&
                update.TopRow[0] == visualWord),
            $"Crocomire visual header ${header:X4} presents edited tile");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Crocomire visual header ${header:X4} reads no compiled source");
    }
}
