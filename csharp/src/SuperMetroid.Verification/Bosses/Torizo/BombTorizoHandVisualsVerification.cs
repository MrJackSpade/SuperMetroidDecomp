using static SuperMetroid.Verification.Bosses.Torizo.LookupBombTorizoHandStockVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>Checks Bomb Torizo hand draw-list extraction and the installed stock.</summary>
internal static class BombTorizoHandVisualsVerification
{
    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyBombTorizoHandVisualInstallation()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectBombTorizoHandVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomObjectBombTorizoHandVisualFilesConstants.ValidateStock(
                installation.RoomObjectBombTorizoHandVisualDirectory);
            AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
                Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Hand stock oracle revision");
            VerifyBombTorizoHandStockMapping(rom, installation.LoadRoomObjectBombTorizoHandVisuals());

            string stockPath = Path.Combine(
                installation.RoomObjectBombTorizoHandVisualDirectory,
                RoomObjectBombTorizoHandVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomObjectBombTorizoHandVisualFilesConstants.ValidateStock(
                    installation.RoomObjectBombTorizoHandVisualDirectory),
                "tampered hand stock fails loudly where it is read");
        }
    }

}
