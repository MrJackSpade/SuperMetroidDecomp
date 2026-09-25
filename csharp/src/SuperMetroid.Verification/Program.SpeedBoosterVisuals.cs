using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifySpeedBoosterVisuals()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "speed-booster-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Speed Booster visual test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmSpeedBoosterVisualFiles.Extract(rom,
                installation.RoomPlmSpeedBoosterVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmSpeedBoosterVisualFiles.ValidateStock(
                installation.RoomPlmSpeedBoosterVisualDirectory);
            ushort drawPointer = SpeedBoosterBlockPlmDrawDefinitions.BombReveal.Pointer;
            AssertEqual((ushort)0x00b6,
                installation.LoadRoomPlmSpeedBoosterVisuals()
                    .GetWord(drawPointer, 0, 0),
                "stock bomb-reveal visual matches the cartridge");

            string stockPath = Path.Combine(
                installation.RoomPlmSpeedBoosterVisualDirectory,
                RoomPlmSpeedBoosterVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Speed Booster JSON is empty.");
            document["entries"]![0]!["blocks"]![0] = 0x0058;
            Directory.CreateDirectory(
                installation.RoomPlmSpeedBoosterVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmSpeedBoosterVisualOverrideDirectory,
                RoomPlmSpeedBoosterVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            RoomPlmSpeedBoosterVisualCatalog edited =
                installation.LoadRoomPlmSpeedBoosterVisuals();
            AssertEqual((ushort)0x0058, edited.GetWord(drawPointer, 0, 0),
                "Speed Booster visual override is loaded");

            (TestAddressSpace bus, RoomLevelData level, RoomPlmSystem plms,
                int blockIndex) = CreateSpeedBoosterBlockFixture(
                    new RoomBlockBehavior(0x82), AreaId.Brinstar);
            level.SetBlockDefinitionWord(0x0058 * 4, 0x0058);
            plms.SpeedBoosterVisuals = edited;
            var guard = new SpeedBoosterPlmReadGuard(bus);
            AssertTrue(plms.TrySpawnBombedSpecialBlock(level, blockIndex,
                    new RoomBlockBehavior(0x82), AreaId.Brinstar, 0x0500),
                "bomb triggers the production Speed Booster reveal PLM");
            plms.Step(guard, level, level.CreateBackgroundStreamer(), 0, 0, 0);
            AssertEqual((ushort)0xb0b6,
                level.GetCollisionBlockByIndex(blockIndex).LevelWord,
                "edited reveal retains compiled type-B physical level word");
            AssertTrue(plms.TilemapUpdates.Any(update =>
                    update.BlockIndex == blockIndex && update.TopRow[0] == 0x0058),
                "edited reveal reaches the immediate tilemap update");
            AssertEqual((ushort)0x0058,
                level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                    blockIndex, 0).TopRow[0],
                "edited reveal survives later camera streaming");
            plms.Step(guard, level, level.CreateBackgroundStreamer(), 0, 0, 0);
            AssertEqual(0, plms.ActiveCount,
                "edited reveal retains native one-frame deletion");
            AssertEqual(0, guard.ForbiddenReadAttempts,
                "edited Speed Booster reveal reads no compiled ROM source bytes");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmSpeedBoosterVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0058,
                RoomPlmSpeedBoosterVisualFiles.Load(refreshed,
                    installation.RoomPlmSpeedBoosterVisualOverrideDirectory)
                    .GetWord(drawPointer, 0, 0),
                "override survives stock replacement");
            document["entries"]![0]!["blocks"]![0] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmSpeedBoosterVisuals(),
                "override cannot change collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmSpeedBoosterVisualFiles.ValidateStock(
                    installation.RoomPlmSpeedBoosterVisualDirectory),
                "stock tampering fails validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine(
            "Speed Booster visuals: native reveal parity, live edit, physical isolation, stock repair and strict failures pass.");
    }
}
