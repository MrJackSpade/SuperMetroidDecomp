using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Rooms.RoomObjects.InstalledFramePictureAssertions;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Areas.Tourian;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>Checks Tourian entrance crumble and clear art independently of its physical PLM blocks.</summary>
internal static class TourianAccessVisualsVerification
{
    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    internal static void VerifyTourianAccessVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "Tourian stock native oracle revision");
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectTourianAccessVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            RoomObjectTourianAccessVisualFilesConstants.ValidateStock(
                installation.RoomObjectTourianAccessVisualDirectory);
            VerifyTourianAccessStockMapping(rom, installation.LoadRoomObjectTourianAccessVisuals());

            string stockPath = Path.Combine(
                installation.RoomObjectTourianAccessVisualDirectory,
                RoomObjectTourianAccessVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomObjectTourianAccessVisualFilesConstants.ValidateStock(
                    installation.RoomObjectTourianAccessVisualDirectory),
                "tampered Tourian stock fails loudly where it is read");
        }
        Console.WriteLine(
            "Tourian access visuals: five native layouts and strict failures pass.");
    }

    /// <summary>Compares the five access-floor frames' pictures with the cartridge, rejects other draws, then checks the import and its rejections (#1557).</summary>
    private static void VerifyTourianAccessStockMapping(CartridgeImportAddressSpace rom,
        RoomObjectTourianAccessVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9297,"crumble-empty-row"), new LookupEscapeGateStockFrame(0x92a3,"crumble-frame-0"),
            new LookupEscapeGateStockFrame(0x92af,"crumble-frame-1"), new LookupEscapeGateStockFrame(0x92bb,"crumble-frame-2"), new LookupEscapeGateStockFrame(0x92c7,"clear-six-rows")];
        foreach (RoomObjectTourianAccessVisuals pictures in new[] { RoomObjectTourianAccessVisuals.Stock(), installed })
        {
            AssertPicturedAsNative(rom, pictures.Images, RoomObjectTourianAccessVisualFiles.Family, frames, WrittenCells(RoomObjectTourianAccessVisualFiles.Family));
        }
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
            if (frames.All(frame => frame.Pointer != raw))
                AssertThrows<InvalidDataException>(() => installed.Image(NativeObjectDraws.At<TourianAccessDraw>((ushort)raw)), "Tourian access unknown pointer");
        AssertImportAndRejections(rom, installed.Images, RoomObjectTourianAccessVisualFiles.Family, RepositoryInstallation.Installation.RoomObjectTourianAccessVisualDirectory);
    }

    /// <summary>Runs Tourian access visual extraction and PLM isolation checks.</summary>
    [VerificationCommand("--tourian-access-visuals")]
    internal static int TourianAccessVisualsCommand()
    {
        Suite(nameof(VerifyTourianAccessVisuals), () => VerifyTourianAccessVisuals());
        return 0;
    }
}
