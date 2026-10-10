using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Checks stock Mother Brain fake-death mappings, editable row and tube tiles, collision isolation, and strict asset validation.</summary>
    private static void VerifyMotherBrainFakeDeathVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Mother Brain stock oracle revision");
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "mother-brain-fake-death-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Mother Brain fake-death visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmMotherBrainFakeDeathVisualFiles.Extract(rom,
                installation.RoomPlmMotherBrainFakeDeathVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmMotherBrainFakeDeathVisualFiles.ValidateStock(
                installation.RoomPlmMotherBrainFakeDeathVisualDirectory);
            RoomPlmMotherBrainFakeDeathVisualCatalog stock =
                installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
            VerifyMotherBrainStockMapping(rom, stock);

            string stockPath = Path.Combine(
                installation.RoomPlmMotherBrainFakeDeathVisualDirectory,
                RoomPlmMotherBrainFakeDeathVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException(
                    "Extracted Mother Brain fake-death JSON is empty.");
            Entry("background-row-d")["blocks"]![12] = 0x0058;
            Entry("clear-bottom-right-tube")["blocks"]![5] = 0x0059;
            Directory.CreateDirectory(
                installation.RoomPlmMotherBrainFakeDeathVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmMotherBrainFakeDeathVisualOverrideDirectory,
                RoomPlmMotherBrainFakeDeathVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmMotherBrainFakeDeathVisualCatalog edited =
                installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
            AssertEqual((ushort)0x0058,
                edited.GetWord(MotherBrainFakeDeathPlmDrawDefinitions.BackgroundRowD,
                    0, 12),
                "Mother Brain override edits far background tile");
            AssertEqual((ushort)0x0059,
                edited.GetWord(
                    MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomRightTube,
                    1, 0),
                "Mother Brain override edits second-run tube tile");
            VerifyMotherBrainVisualDraw(RoomPlmHeaders.MotherBrainsBackgroundRowD,
                12, 0, 0x1249, 0x0058, edited, cameraX: 64);
            VerifyMotherBrainVisualDraw(RoomPlmHeaders.ClearMotherBrainBottomRightTube,
                -1, 0, 0x00ff, 0x0059, edited, cameraX: 0);
            VerifyMotherBrainVisualDraw(RoomPlmHeaders.MotherBrainsBackgroundRowD,
                12, 0, 0x1249, new RoomLevelWord(0x1249).VisualWord,
                null, cameraX: 64);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmMotherBrainFakeDeathVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0059,
                RoomPlmMotherBrainFakeDeathVisualFiles.Load(refreshed,
                    installation.RoomPlmMotherBrainFakeDeathVisualOverrideDirectory)
                    .GetWord(
                        MotherBrainFakeDeathPlmDrawDefinitions.ClearBottomRightTube,
                        1, 0),
                "Mother Brain override survives stock replacement");

            Entry("background-row-d")["blocks"]![12] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmMotherBrainFakeDeathVisuals(),
                "Mother Brain override cannot change collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmMotherBrainFakeDeathVisualFiles.ValidateStock(
                    installation.RoomPlmMotherBrainFakeDeathVisualDirectory),
                "tampered Mother Brain fake-death stock fails manifest validation");

            JsonNode Entry(string id) => document["entries"]!.AsArray()
                .Single(entry => entry!["id"]!.GetValue<string>() == id)!;
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Mother Brain fake-death visuals: 22 stock draws, live row/tube edits, collision isolation, override persistence and strict failures pass.");
    }

    /// <summary>Verifies a fake-death PLM redraw uses the selected visual tile while preserving the underlying collision word.</summary>
    /// <param name="header">PLM header selecting the fake-death background or tube mutation.</param>
    /// <param name="xOffset">Horizontal block offset from the spawned PLM origin.</param>
    /// <param name="yOffset">Vertical block offset from the spawned PLM origin.</param>
    /// <param name="physicalWord">Expected level word retained by the collision map.</param>
    /// <param name="visualWord">Expected tile word submitted to the streamed background map.</param>
    /// <param name="visuals">Optional replacement catalog; <see langword="null"/> exercises the stock visual mapping.</param>
    /// <param name="cameraX">Camera position used while the room PLM updates its streamed tilemap destination.</param>
    private static void VerifyMotherBrainVisualDraw(ushort header,
        int xOffset, int yOffset, ushort physicalWord, ushort visualWord,
        RoomPlmMotherBrainFakeDeathVisualCatalog? visuals, ushort cameraX)
    {
        const int width = 32;
        ushort[] words = Enumerable.Repeat((ushort)0x8123, width * 16).ToArray();
        RoomLevelData level = CreateRoom(width, 16, words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        level.SetBlockDefinitionWord(0x0058 * 4, 0x0058);
        level.SetBlockDefinitionWord(0x0059 * 4, 0x0059);
        level.SetBlockDefinitionWord(0x0249 * 4, 0x0249);
        var plms = new RoomPlmSystem { MotherBrainFakeDeathVisuals = visuals };
        AssertTrue(plms.TrySpawnMotherBrainMutation(level, 5, 3, header),
            $"Mother Brain visual header ${header:X4} allocates");
        var guard = new MotherBrainFakeDeathSourceGuard(new TestAddressSpace());
        plms.Step(guard, level, level.CreateBackgroundStreamer(), cameraX, 0, 0);
        int blockIndex = (3 + yOffset) * width + 5 + xOffset;
        AssertEqual(physicalWord,
            level.GetCollisionBlockByIndex(blockIndex).LevelWord,
            $"Mother Brain visual header ${header:X4} retains physical block");
        // The redraw identifies its block by the block's DrawPLM BG1 ring destination.
        ushort blockDestination = level.CreateBackgroundStreamer()
            .BuildPlmLevelBlockUpdate(blockIndex, 0).TopRowDestination;
        AssertTrue(plms.TilemapUpdates.Any(update =>
                update.TopRowDestination == blockDestination &&
                update.TopRow[0] == visualWord),
            $"Mother Brain visual header ${header:X4} presents edited tile");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Mother Brain visual header ${header:X4} reads no compiled source");
    }
}
