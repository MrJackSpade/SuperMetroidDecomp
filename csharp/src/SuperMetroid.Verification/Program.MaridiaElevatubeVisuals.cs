using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyMaridiaElevatubeVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "maridia-elevatube-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Maridia elevatube visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmMaridiaElevatubeVisualFiles.Extract(rom,
                installation.RoomPlmMaridiaElevatubeVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmMaridiaElevatubeVisualFiles.ValidateStock(
                installation.RoomPlmMaridiaElevatubeVisualDirectory);
            AssertEqual((ushort)0x0180,
                installation.LoadRoomPlmMaridiaElevatubeVisuals()
                    .GetWord(MaridiaElevatubePlmDefinitions.DrawPointer, 0, 0),
                "stock elevatube tile matches the cartridge");

            string stockPath = Path.Combine(
                installation.RoomPlmMaridiaElevatubeVisualDirectory,
                RoomPlmMaridiaElevatubeVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted elevatube JSON is empty.");
            document["entries"]![0]!["blocks"]![0] = 0x0058;
            Directory.CreateDirectory(
                installation.RoomPlmMaridiaElevatubeVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmMaridiaElevatubeVisualOverrideDirectory,
                RoomPlmMaridiaElevatubeVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmMaridiaElevatubeVisualCatalog edited =
                installation.LoadRoomPlmMaridiaElevatubeVisuals();
            AssertEqual((ushort)0x0058,
                edited.GetWord(MaridiaElevatubePlmDefinitions.DrawPointer, 0, 0),
                "elevatube override replaces only the visible block");

            RoomLevelData level = CreateRoom(4, 4, new ushort[16], new byte[16],
                blockDefinitions: new byte[0x400 * 8]);
            level.SetBlockDefinitionWord(0x0058 * 4, 0x0058);
            var plms = new RoomPlmSystem { MaridiaElevatubeVisuals = edited };
            AssertTrue(plms.TrySpawnMaridiaElevatube(level),
                "production door setup spawns the elevatube PLM");
            var guard = new MaridiaElevatubeSourceGuard(new TestAddressSpace());
            BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
            int blockIndex = level.GetBlockIndex(
                MaridiaElevatubePlmRomData.BlockX,
                MaridiaElevatubePlmRomData.BlockY);
            plms.Step(guard, level, streamer, 0, 0, 0);
            AssertEqual((ushort)0x8180,
                level.GetCollisionBlockByIndex(blockIndex).LevelWord,
                "edited elevatube tile retains native physical block");
            AssertTrue(plms.TilemapUpdates.Any(update =>
                    update.BlockIndex == blockIndex && update.TopRow[0] == 0x0058),
                "edited elevatube reaches immediate tilemap update");
            AssertEqual((ushort)0x0058,
                level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                    blockIndex, 0).TopRow[0],
                "edited elevatube survives later camera streaming");
            int heardAtFrame = -1;
            for (int frame = 1; frame < 24 && plms.ActiveCount != 0; frame++)
            {
                plms.Step(guard, level, streamer, 0, 0, 0);
                if (plms.SoundRequests.Any(request =>
                        request.SoundEffect == SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            MaridiaElevatubePlmDefinitions.SoundId)))
                    heardAtFrame = frame;
            }
            AssertEqual(16, heardAtFrame,
                "edited art leaves native sixteen-frame sound timing intact");
            AssertEqual(0, plms.ActiveCount,
                "edited art leaves native PLM deletion intact");
            AssertEqual(0, guard.ForbiddenReadAttempts,
                "edited elevatube reads no compiled source bytes");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmMaridiaElevatubeVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0058,
                RoomPlmMaridiaElevatubeVisualFiles.Load(refreshed,
                    installation.RoomPlmMaridiaElevatubeVisualOverrideDirectory)
                    .GetWord(MaridiaElevatubePlmDefinitions.DrawPointer, 0, 0),
                "elevatube override survives stock replacement");
            document["entries"]![0]!["blocks"]![0] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmMaridiaElevatubeVisuals(),
                "elevatube override cannot edit collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmMaridiaElevatubeVisualFiles.ValidateStock(
                    installation.RoomPlmMaridiaElevatubeVisualDirectory),
                "tampered elevatube stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Maridia elevatube visuals: native tile, live edit, physical/sound isolation, stock repair and strict failures pass.");
    }
}
