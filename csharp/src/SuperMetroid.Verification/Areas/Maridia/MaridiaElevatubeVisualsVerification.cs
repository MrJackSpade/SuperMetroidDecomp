using SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Areas.Maridia.LookupElevatubeStockVerification;
using static SuperMetroid.Verification.Areas.Maridia.MaridiaElevatubePlmVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Maridia;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks installed Maridia elevatube tile art through stock extraction, installation, and live PLM drawing.</summary>
internal static class MaridiaElevatubeVisualsVerification
{
    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyMaridiaElevatubeVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Elevatube stock oracle revision");
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmMaridiaElevatubeVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomPlmMaridiaElevatubeVisualFilesConstants.ValidateStock(
                installation.RoomPlmMaridiaElevatubeVisualDirectory);
            VerifyElevatubeStockMapping(rom, installation.LoadRoomPlmMaridiaElevatubeVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmMaridiaElevatubeVisualDirectory,
                RoomPlmMaridiaElevatubeVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmMaridiaElevatubeVisualFilesConstants.ValidateStock(
                    installation.RoomPlmMaridiaElevatubeVisualDirectory),
                "tampered elevatube stock fails loudly where it is read");
        }
        Console.WriteLine(
            "Maridia elevatube visuals: native tile and strict failures pass.");
    }

    /// <summary>Runs the focused elevatube artwork and PLM behavior checks.</summary>
    [VerificationCommand("--maridia-elevatube-visuals")]
    internal static int MaridiaElevatubeVisualsCommand()
    {
        Suite(nameof(VerifyMaridiaElevatubeVisuals), () => VerifyMaridiaElevatubeVisuals());
        return 0;
    }

}
