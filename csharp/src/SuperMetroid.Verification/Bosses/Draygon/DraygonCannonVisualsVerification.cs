using static SuperMetroid.Verification.Bosses.Draygon.LookupDraygonCannonStockVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>Checks Draygon cannon visual extraction, installation, and stock integrity.</summary>
internal static class DraygonCannonVisualsVerification
{
    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyDraygonCannonVisualInstallation(
        CartridgeImportAddressSpace rom)
    {
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectDraygonCannonVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomObjectDraygonCannonVisualFilesConstants.ValidateStock(
                installation.RoomObjectDraygonCannonVisualDirectory);
            VerifyDraygonCannonStockMapping(rom, installation.LoadRoomObjectDraygonCannonVisuals());

            string stockPath = Path.Combine(
                installation.RoomObjectDraygonCannonVisualDirectory,
                RoomObjectDraygonCannonVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomObjectDraygonCannonVisualFilesConstants.ValidateStock(
                    installation.RoomObjectDraygonCannonVisualDirectory),
                "tampered cannon stock fails loudly where it is read");
        }
    }

}
