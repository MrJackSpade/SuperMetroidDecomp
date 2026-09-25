using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyTourianAccessVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
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
            RoomPlmTourianAccessVisualCatalog stock =
                installation.LoadRoomPlmTourianAccessVisuals();
            AssertEqual((ushort)0x0053,
                stock.GetWord(TourianAccessPlmDrawDefinitions.CrumbleFirstPointer,
                    0, 0),
                "stock Tourian first crumble frame selects native visual block");
            AssertEqual((ushort)0x00ff,
                stock.GetWord(TourianAccessPlmDrawDefinitions.ClearPointer, 5, 0),
                "stock Tourian clear includes the bottom physical row");
            AssertThrows<InvalidDataException>(
                () => new RoomPlmTourianAccessVisualCatalog(
                    [new RoomPlmTourianAccessVisualEntry(
                        "crumble-frame-0", new ushort[4])]),
                "Tourian visual catalog rejects missing frames");

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
