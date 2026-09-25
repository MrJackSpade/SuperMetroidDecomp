using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifySporeSpawnCeilingVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "spore-ceiling-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Spore Spawn ceiling visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmSporeSpawnCeilingVisualFiles.Extract(rom,
                installation.RoomPlmSporeSpawnCeilingVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmSporeSpawnCeilingVisualFiles.ValidateStock(
                installation.RoomPlmSporeSpawnCeilingVisualDirectory);
            RoomPlmSporeSpawnCeilingVisualCatalog stock =
                installation.LoadRoomPlmSporeSpawnCeilingVisuals();
            AssertEqual((ushort)0x0053,
                stock.GetWord(SporeSpawnCeilingPlmDrawDefinitions.CrumbleFirstPointer,
                    0, 0),
                "stock Spore Spawn first crumble block matches ROM");
            AssertEqual((ushort)0x00ff,
                stock.GetWord(SporeSpawnCeilingPlmDrawDefinitions.ClearPointer,
                    1, 1),
                "stock Spore Spawn bottom-right clear block matches ROM");
            AssertThrows<InvalidDataException>(
                () => new RoomPlmSporeSpawnCeilingVisualCatalog(
                    [new RoomPlmSporeSpawnCeilingVisualEntry(
                        "crumble-frame-0", new ushort[4])]),
                "Spore Spawn visual catalog rejects missing frames");

            string stockPath = Path.Combine(
                installation.RoomPlmSporeSpawnCeilingVisualDirectory,
                RoomPlmSporeSpawnCeilingVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Spore Spawn ceiling JSON is empty.");
            JsonNode first = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "crumble-frame-0")!;
            JsonNode clear = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "clear-ceiling")!;
            first["blocks"]![0] = 0x0058;
            clear["blocks"]![3] = 0x0059;
            Directory.CreateDirectory(
                installation.RoomPlmSporeSpawnCeilingVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmSporeSpawnCeilingVisualOverrideDirectory,
                RoomPlmSporeSpawnCeilingVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmSporeSpawnCeilingVisualCatalog edited =
                installation.LoadRoomPlmSporeSpawnCeilingVisuals();
            AssertEqual((ushort)0x0058,
                edited.GetWord(SporeSpawnCeilingPlmDrawDefinitions.CrumbleFirstPointer,
                    0, 0),
                "Spore Spawn override edits the first crumble block");
            AssertEqual((ushort)0x0059,
                edited.GetWord(SporeSpawnCeilingPlmDrawDefinitions.ClearPointer,
                    1, 1),
                "Spore Spawn override edits the bottom-right clear block");

            VerifySporeSpawnCeilingVisualSeparation(edited, clear: false);
            VerifySporeSpawnCeilingVisualSeparation(edited, clear: true);

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmSporeSpawnCeilingVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0059,
                RoomPlmSporeSpawnCeilingVisualFiles.Load(refreshed,
                    installation.RoomPlmSporeSpawnCeilingVisualOverrideDirectory)
                    .GetWord(SporeSpawnCeilingPlmDrawDefinitions.ClearPointer, 1, 1),
                "Spore Spawn override survives stock replacement");

            first["blocks"]![0] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmSporeSpawnCeilingVisuals(),
                "Spore Spawn override cannot change physical collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmSporeSpawnCeilingVisualFiles.ValidateStock(
                    installation.RoomPlmSporeSpawnCeilingVisualDirectory),
                "tampered Spore Spawn ceiling stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Spore Spawn ceiling visuals: four native frames, live 2x2 edits, physical/timing isolation, stock repair and strict failures pass.");
    }

    private static void VerifySporeSpawnCeilingVisualSeparation(
        RoomPlmSporeSpawnCeilingVisualCatalog edited, bool clear)
    {
        const int width = 16;
        const int height = 32;
        ushort[] words = new ushort[width * height];
        for (int y = 30; y <= 31; y++)
        for (int x = 7; x <= 8; x++)
            words[y * width + x] = 0x0123;
        RoomLevelData level = CreateRoom(width, height, words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        level.SetBlockDefinitionWord(0x0058 * 4, 0x0058);
        level.SetBlockDefinitionWord(0x0059 * 4, 0x0059);
        var plms = new RoomPlmSystem { SporeSpawnCeilingVisuals = edited };
        AssertTrue(plms.TrySpawnSporeSpawnCeiling(level,
                clear ? RoomPlmHeaders.ClearSporeSpawnCeiling :
                    RoomPlmHeaders.CrumbleSporeSpawnCeiling),
            $"Spore Spawn {(clear ? "clear" : "crumble")} PLM allocates");
        var guard = new SporeSpawnCeilingSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(guard, level, streamer, 0, 16 * 16, 0);
        int blockIndex = clear ? 31 * width + 8 : 30 * width + 7;
        ushort expectedPhysical = clear ? (ushort)0x00ff : (ushort)0x0053;
        ushort expectedVisual = clear ? (ushort)0x0059 : (ushort)0x0058;
        AssertEqual(expectedPhysical,
            level.GetCollisionBlockByIndex(blockIndex).LevelWord,
            $"Spore Spawn {(clear ? "clear" : "crumble")} edited art retains physical word");
        AssertTrue(plms.TilemapUpdates.Any(update =>
                update.BlockIndex == blockIndex &&
                update.TopRow[0] == expectedVisual),
            $"Spore Spawn {(clear ? "clear" : "crumble")} edit reaches immediate redraw");
        AssertEqual(expectedVisual,
            level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                blockIndex, 0).TopRow[0],
            $"Spore Spawn {(clear ? "clear" : "crumble")} edit survives streaming");
        int deletionFrame = -1;
        for (int frame = 1; frame < 24 && plms.ActiveCount != 0; frame++)
        {
            plms.Step(guard, level, streamer, 0, 16 * 16, 0);
            if (plms.ActiveCount == 0)
                deletionFrame = frame;
        }
        AssertEqual(clear ? 4 : 16, deletionFrame,
            $"Spore Spawn {(clear ? "clear" : "crumble")} edited art preserves deletion timing");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Spore Spawn {(clear ? "clear" : "crumble")} edited draw reads no source bytes");
    }
}
