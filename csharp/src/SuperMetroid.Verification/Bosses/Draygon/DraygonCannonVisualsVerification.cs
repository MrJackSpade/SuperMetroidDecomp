using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Bosses.Draygon.LookupDraygonCannonStockVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Verification.Harness;
namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks Draygon cannon visual extraction, installation, overrides, and stock integrity.</summary>
internal static class DraygonCannonVisualsVerification
{
    /// <summary>Exercises all stock cannon layouts and verifies visual-only overrides survive stock replacement.</summary>
    internal static void VerifyDraygonCannonVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("draygon-cannon-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            // Extraction compares all twelve reachable native layouts, including
            // every row word and signed offset, with the compiled definitions.
            RoomPlmDraygonCannonVisualFiles.Extract(rom,
                installation.RoomPlmDraygonCannonVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmDraygonCannonVisualFiles.ValidateStock(
                installation.RoomPlmDraygonCannonVisualDirectory);
            VerifyDraygonCannonStockMapping(rom, installation.LoadRoomPlmDraygonCannonVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmDraygonCannonVisualDirectory,
                RoomPlmDraygonCannonVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted cannon JSON is empty.");
            JsonNode rightShield = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "right-shield-a");
            rightShield.Required("blocks")[0] = InstalledAppearanceJson.Cell(0x53);
            Directory.CreateDirectory(
                installation.RoomPlmDraygonCannonVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmDraygonCannonVisualOverrideDirectory,
                RoomPlmDraygonCannonVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual(RoomBlockAppearance.Of(0x53),
                installation.LoadRoomPlmDraygonCannonVisuals().GetAppearance(
                    PlmDraw.Of(DraygonCannonDraw.RightShieldA), 0, 0),
                "installed cannon override selects its edited block");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmDraygonCannonVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x53),
                RoomPlmDraygonCannonVisualFiles.Load(refreshed,
                    installation.RoomPlmDraygonCannonVisualOverrideDirectory)
                    .GetAppearance(PlmDraw.Of(DraygonCannonDraw.RightShieldA), 0, 0),
                "cannon override survives stock replacement");

            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmDraygonCannonVisualFiles.ValidateStock(
                    installation.RoomPlmDraygonCannonVisualDirectory),
                "tampered cannon stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }
}
