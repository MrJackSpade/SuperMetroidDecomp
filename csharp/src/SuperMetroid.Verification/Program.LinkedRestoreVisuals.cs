using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyLinkedRestoreVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "linked-restore-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Linked restore test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            // Extraction checks all six compiled row shapes, full physical words,
            // and signed offsets against this pinned retail cartridge.
            RoomPlmLinkedRestoreVisualFiles.Extract(rom,
                installation.RoomPlmLinkedRestoreVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmLinkedRestoreVisualFiles.ValidateStock(
                installation.RoomPlmLinkedRestoreVisualDirectory);
            AssertEqual((ushort)0x0058,
                installation.LoadRoomPlmLinkedRestoreVisuals().GetWord(
                    RoomPlmBombBlockRestoreDrawDefinitions.Horizontal, 0, 0),
                "stock bomb restore selects cartridge block 058");
            AssertEqual((ushort)0x00bc,
                installation.LoadRoomPlmLinkedRestoreVisuals().GetWord(
                    RoomPlmContactCrumbleRestoreDrawDefinitions.Horizontal, 0, 0),
                "stock crumble restore selects cartridge block 0BC");
            AssertThrows<InvalidDataException>(
                () => new RoomPlmLinkedRestoreVisualCatalog(
                    [new RoomPlmLinkedRestoreVisualEntry("bomb-horizontal",
                        new ushort[2])]),
                "restore catalog rejects missing linked layouts");

            string stockPath = Path.Combine(
                installation.RoomPlmLinkedRestoreVisualDirectory,
                RoomPlmLinkedRestoreVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted restore JSON is empty.");
            JsonNode bomb = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "bomb-horizontal")!;
            bomb["blocks"]![0] = 0x0053;
            Directory.CreateDirectory(
                installation.RoomPlmLinkedRestoreVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmLinkedRestoreVisualOverrideDirectory,
                RoomPlmLinkedRestoreVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmLinkedRestoreVisuals().GetWord(
                    RoomPlmBombBlockRestoreDrawDefinitions.Horizontal, 0, 0),
                "installed restore override selects edited block");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmLinkedRestoreVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmLinkedRestoreVisualFiles.Load(refreshed,
                    installation.RoomPlmLinkedRestoreVisualOverrideDirectory)
                    .GetWord(RoomPlmBombBlockRestoreDrawDefinitions.Horizontal, 0, 0),
                "restore override survives stock replacement");
            bomb["blocks"]![0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmLinkedRestoreVisuals(),
                "restore override rejects physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmLinkedRestoreVisualFiles.ValidateStock(
                    installation.RoomPlmLinkedRestoreVisualDirectory),
                "tampered restore stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    private static void VerifyLinkedRestoreVisualSeparation(
        SuperMetroidAddressSpace rom, HashSet<int> forbidden, bool bomb)
    {
        RoomPlmLinkedRestoreVisualEntry[] entries =
            RoomPlmLinkedRestoreDrawDefinitions.All.Select(draw =>
                new RoomPlmLinkedRestoreVisualEntry(
                    RoomPlmLinkedRestoreDrawDefinitions.VisualId(draw.Pointer),
                    draw.Runs.Span.ToArray().SelectMany(run =>
                        run.LevelWords.Span.ToArray().Select(word =>
                            new RoomLevelWord(word).VisualWord)).ToArray()))
                .ToArray();
        string editedId = bomb ? "bomb-horizontal" : "crumble-horizontal";
        ushort editedWord = bomb ? (ushort)0x0053 : (ushort)0x0054;
        entries.Single(entry => entry.Id == editedId).Blocks[0] = editedWord;
        var visuals = new RoomPlmLinkedRestoreVisualCatalog(entries);

        const int width = 8;
        const int blockIndex = 27;
        var words = new ushort[width * width];
        words[blockIndex] = bomb ? (ushort)0xf321 : (ushort)0xb321;
        var definitions = new byte[0x400 * 8];
        definitions[editedWord * 8] = (byte)editedWord;
        var level = new RoomLevelData(width, width, words,
            new byte[words.Length], new ushort[words.Length], definitions);
        var plms = new RoomPlmSystem { LinkedRestoreVisuals = visuals };
        bool spawned = bomb
            ? plms.TrySpawnCollisionBombBlock(level, blockIndex, 1)
            : plms.TrySpawnSamusContactCrumbleBlock(level, blockIndex,
                new RoomBlockBehavior(1));
        AssertTrue(spawned, $"{editedId} installs its real linked PLM");

        var streamer = level.CreateBackgroundStreamer();
        var guarded = new ShotBlockProgramReadGuard(rom, forbidden);
        bool sawImmediate = false;
        for (int frame = 0; frame < (bomb ? 420 : 100); frame++)
        {
            plms.Step(guarded, level, streamer, 0, 0, 0);
            sawImmediate |= plms.TilemapUpdates.Any(update =>
                update.BlockIndex == blockIndex &&
                update.TopRow[0] == editedWord);
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"{editedId} does not reread compiled program or draw bytes");
        AssertTrue(sawImmediate,
            $"{editedId} reaches an immediate tilemap update");
        AssertEqual(bomb ? (ushort)0xf058 : (ushort)0xb0bc,
            level.GetCollisionBlockByIndex(blockIndex).LevelWord,
            $"{editedId} preserves its native physical parent word");
        AssertEqual(editedWord,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                blockIndex, 0).TopRow[0],
            $"{editedId} survives later room streaming");
    }
}
