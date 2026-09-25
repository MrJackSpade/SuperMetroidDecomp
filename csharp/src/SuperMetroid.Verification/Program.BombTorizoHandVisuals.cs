using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyBombTorizoHandVisualInstallation()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        string testRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "bomb-torizo-hand-visual-" + Guid.NewGuid().ToString("N")));
        string allowedRoot = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
            Path.DirectorySeparatorChar;
        if (!testRoot.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Bomb Torizo hand test root escaped test-temp.");
        try
        {
            var installation = new GameInstallation(testRoot);
            // Extraction compares every native direction, word, and signed offset
            // with the compiled lists before it writes a stock file.
            RoomPlmBombTorizoHandVisualFiles.Extract(rom,
                installation.RoomPlmBombTorizoHandVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmBombTorizoHandVisualFiles.ValidateStock(
                installation.RoomPlmBombTorizoHandVisualDirectory);
            AssertEqual((ushort)0x0065,
                installation.LoadRoomPlmBombTorizoHandVisuals().GetWord(
                    BombTorizoHandPlmDrawDefinitions.Intact, 0, 0),
                "stock hand visual matches the native first block");
            AssertThrows<InvalidDataException>(
                () => new RoomPlmBombTorizoHandVisualCatalog(
                    [new RoomPlmBombTorizoHandVisualEntry("intact", new ushort[8])]),
                "hand catalog rejects a missing cleared frame");

            string stockPath = Path.Combine(
                installation.RoomPlmBombTorizoHandVisualDirectory,
                RoomPlmBombTorizoHandVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted hand JSON is empty.");
            JsonNode intact = document["entries"]!.AsArray().Single(entry =>
                entry!["id"]!.GetValue<string>() == "intact")!;
            intact["blocks"]![0] = 0x0053;
            Directory.CreateDirectory(
                installation.RoomPlmBombTorizoHandVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmBombTorizoHandVisualOverrideDirectory,
                RoomPlmBombTorizoHandVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmBombTorizoHandVisuals().GetWord(
                    BombTorizoHandPlmDrawDefinitions.Intact, 0, 0),
                "installed hand override selects its changed block");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmBombTorizoHandVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmBombTorizoHandVisualFiles.Load(refreshed,
                    installation.RoomPlmBombTorizoHandVisualOverrideDirectory)
                    .GetWord(BombTorizoHandPlmDrawDefinitions.Intact, 0, 0),
                "hand override survives stock replacement");

            intact["blocks"]![0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmBombTorizoHandVisuals(),
                "hand override rejects collision bits in visual words");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmBombTorizoHandVisualFiles.ValidateStock(
                    installation.RoomPlmBombTorizoHandVisualDirectory),
                "tampered hand stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }
}
