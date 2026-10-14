using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Areas.Ceres.CeresDestructionArtworkVerification;
using static SuperMetroid.Verification.Areas.Ceres.CeresFlightArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingFlyawayArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingMode7ArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingObjectArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.Artwork.EndingPaletteArtworkVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed intro and ending artwork after the frontend parity setup phase.</summary>
internal static class IntroArtworkPostSlicesVerification
{
    /// <summary>
    /// Runs the artwork checks after intro frontend parity without replaying its thousands
    /// of setup frames. This keeps typed-DMA guard regressions quick to isolate.
    /// </summary>
    internal static void VerifyIntroArtworkPostSlices(string sourceRom)
    {
        using var rootScratch = new TestScratchDirectory("intro-post-slices");
        string root = rootScratch.Root;
        try
        {
            GameInstallation installation = GameAssetInstaller.Install(sourceRom, root);
            CartridgeImportAddressSpace bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
            VerifyCeresFlightArtwork(installation, bus);
            VerifyCeresDestructionArtwork(installation, bus);
            VerifyEndingFlyawayArtwork(installation);
            VerifyEndingMode7Artwork(installation);
            VerifyEndingObjectArtwork(installation);
            VerifyEndingPaletteArtwork(installation);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    /// <summary>Runs focused flight, destruction, flyaway, Mode 7, object, and palette artwork checks.</summary>
    [VerificationCommand("--intro-artwork-post-slices")]
    internal static int IntroArtworkPostSlicesCommand()
    {
        string postSliceRom = RepositoryRomPath;
        Suite(nameof(VerifyIntroArtworkPostSlices), () => VerifyIntroArtworkPostSlices(postSliceRom));
        return 0;
    }
}
