using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresDestructionArtworkVerification;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresFlightArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingFlyawayArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingMode7ArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.EndingObjectArtworkVerification;
using static SuperMetroid.Verification.Frontend.Ending.Artwork.EndingPaletteArtworkVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed intro and ending artwork after the frontend parity setup phase.</summary>
internal static class IntroArtworkPostSlicesVerification
{
    /// <summary>Runs the Ceres cinematic and ending artwork checks.</summary>
    internal static void VerifyIntroArtworkPostSlices(string sourceRom)
    {
        VerifyCeresCinematicArtwork(sourceRom);
        VerifyEndingArtwork();
    }

    /// <summary>
    /// Runs the Ceres flight and destruction artwork checks, which only read the shared installation; ROM import itself
    /// is covered by the asset-import verification.
    /// </summary>
    internal static void VerifyCeresCinematicArtwork(string sourceRom)
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        CartridgeImportAddressSpace bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        Suite(nameof(VerifyCeresFlightArtwork), () => VerifyCeresFlightArtwork(installation, bus));
        Suite(nameof(VerifyCeresDestructionArtwork), () => VerifyCeresDestructionArtwork(installation, bus));
    }

    /// <summary>Runs the ending flyaway, Mode 7, object and palette artwork checks, which only read the shared installation.</summary>
    internal static void VerifyEndingArtwork()
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        Suite(nameof(VerifyEndingFlyawayArtwork), () => VerifyEndingFlyawayArtwork());
        Suite(nameof(VerifyEndingMode7Artwork), () => VerifyEndingMode7Artwork());
        Suite(nameof(VerifyEndingObjectArtwork), () => VerifyEndingObjectArtwork());
        Suite(nameof(VerifyEndingPaletteArtwork), () => VerifyEndingPaletteArtwork(installation));
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
