using static SuperMetroid.Verification.Areas.Escape.LookupEscapeGateStockVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Escape;

/// <summary>Checks Mother Brain escape gate visuals against PLM collision and install contracts.</summary>
internal static class EscapeGateVisualsVerification
{
    /// <summary>Checks the installed pictures of the gate's frames and that invalid installed content is rejected.</summary>
    internal static void VerifyEscapeGateVisuals(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyEscapeGateVisualInstallation), () => VerifyEscapeGateVisualInstallation(rom));
        Console.WriteLine(
            "Escape-gate visuals: the installed pictures and invalid content checks pass.");
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyEscapeGateVisualInstallation(CartridgeImportAddressSpace rom)
    {
        // A private installation copy, whose stock file is tampered below.
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectEscapeGateVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectEscapeGateVisualFilesConstants.ValidateStock(
            installation.RoomObjectEscapeGateVisualDirectory);
        VerifyEscapeGateStockMapping(rom, installation.LoadRoomObjectEscapeGateVisuals());
        string stockPath = Path.Combine(
            installation.RoomObjectEscapeGateVisualDirectory,
            RoomObjectEscapeGateVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => RoomObjectEscapeGateVisualFilesConstants.ValidateStock(
                installation.RoomObjectEscapeGateVisualDirectory),
            "tampered escape-gate stock fails loudly where it is read");
    }

}
