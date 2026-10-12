using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Runs the artwork checks after intro frontend parity without replaying its thousands
    /// of setup frames. This keeps typed-DMA guard regressions quick to isolate.
    /// </summary>
    private static void VerifyIntroArtworkPostSlices(string sourceRom)
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
}
