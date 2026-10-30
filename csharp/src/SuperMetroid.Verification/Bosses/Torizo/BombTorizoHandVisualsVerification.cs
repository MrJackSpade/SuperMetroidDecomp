using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Bosses.Torizo.LookupBombTorizoHandStockVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>Checks Bomb Torizo hand draw-list extraction, installed overrides,
/// and stock refresh.</summary>
internal static class BombTorizoHandVisualsVerification
{
    /// <summary>Extracts the native hand art into a scratch installation and
    /// confirms one edited block survives loading and stock replacement.</summary>
    internal static void VerifyBombTorizoHandVisualInstallation()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        using var testRootScratch = new TestScratchDirectory("bomb-torizo-hand-visual");
        string testRoot = testRootScratch.Root;
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
            AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
                Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hand stock oracle revision");
            VerifyBombTorizoHandStockMapping(rom, installation.LoadRoomPlmBombTorizoHandVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmBombTorizoHandVisualDirectory,
                RoomPlmBombTorizoHandVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted hand JSON is empty.");
            JsonNode intact = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "intact");
            intact.Required("blocks")[0] = InstalledAppearanceJson.Cell(0x53);
            Directory.CreateDirectory(
                installation.RoomPlmBombTorizoHandVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmBombTorizoHandVisualOverrideDirectory,
                RoomPlmBombTorizoHandVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual(RoomBlockAppearance.Of(0x53),
                installation.LoadRoomPlmBombTorizoHandVisuals().GetAppearance(
                    PlmDraw.Of(BombTorizoHandDraw.Intact), 0, 0),
                "installed hand override selects its changed block");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmBombTorizoHandVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x53),
                RoomPlmBombTorizoHandVisualFiles.Load(refreshed,
                    installation.RoomPlmBombTorizoHandVisualOverrideDirectory)
                    .GetAppearance(PlmDraw.Of(BombTorizoHandDraw.Intact), 0, 0),
                "hand override survives stock replacement");

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
