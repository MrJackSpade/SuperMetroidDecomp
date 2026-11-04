using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Bosses.Draygon.LookupDraygonCannonStockVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks Draygon cannon visual extraction, installation, overrides, and stock integrity.</summary>
internal static class DraygonCannonVisualsVerification
{
    /// <summary>Exercises all stock cannon layouts and verifies visual-only overrides survive stock replacement.</summary>
    internal static void VerifyDraygonCannonVisualInstallation(
        SuperMetroidAddressSpace rom)
    {
        // A private installation copy, whose override is written and whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmDraygonCannonVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomPlmDraygonCannonVisualFilesConstants.ValidateStock(
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

            string refreshed = Path.Combine(copy.Root, "refreshed-stock");
            AssertEqual(RoomBlockAppearance.Of(0x53),
                ReExtractDraygonCannonStock(rom, refreshed,
                    installation.RoomPlmDraygonCannonVisualOverrideDirectory)
                    .GetAppearance(PlmDraw.Of(DraygonCannonDraw.RightShieldA), 0, 0),
                "cannon override survives stock replacement");

            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmDraygonCannonVisualFilesConstants.ValidateStock(
                    installation.RoomPlmDraygonCannonVisualDirectory),
                "tampered cannon stock fails manifest validation");
        }
    }

    /// <summary>Extracts stock Draygon cannon visuals from the cartridge into <paramref name="refreshed"/> and loads them under <paramref name="overrides"/>.</summary>
    [PrivateInstallation("Runs RoomPlmDraygonCannonVisualFiles.Extract into a fresh stock directory beside the copy's Draygon cannon override, to check that a stock replacement keeps the edited right-shield-a block (appearance 53) selected; the copied stock is the installer's output and never re-runs the extractor.")]
    private static RoomPlmDraygonCannonVisualCatalog ReExtractDraygonCannonStock(ISnesAddressSpace rom, string refreshed, string overrides)
    {
        RoomPlmDraygonCannonVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
        return RoomPlmDraygonCannonVisualFiles.Load(refreshed, overrides);
    }
}
