using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static Program;
using static RepositoryInputsVerification;
using static SuiteTimingVerification;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using static CeresDestructionArtworkVerification;
using static CeresFlightArtworkVerification;
using static EndingFlyawayArtworkVerification;
using static EndingMode7ArtworkVerification;
using static EndingObjectArtworkVerification;
using static EndingPaletteArtworkVerification;

/// <summary>Represents intro artwork post slices verification.</summary>
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

    /// <summary>intro artwork post slices command.</summary>
    [VerificationCommand("--intro-artwork-post-slices")]
    internal static int IntroArtworkPostSlicesCommand()
    {
        string postSliceRom = RepositoryRomPath;
        Suite(nameof(VerifyIntroArtworkPostSlices), () => VerifyIntroArtworkPostSlices(postSliceRom));
        return 0;
    }
}
