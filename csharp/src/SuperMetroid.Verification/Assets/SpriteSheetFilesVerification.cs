using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms the installed sprite sheets (#1392): extraction writes each sheet's composed image and frames, and the
/// installed timer sheet loads holding exactly the composed pixels and frames.
/// </summary>
internal static class SpriteSheetFilesVerification
{
    /// <summary>Checks that composing the sheets again reproduces the installed files, then that the installed timer sheet loads as composed.</summary>
    internal static void VerifySpriteSheetFiles(ISnesAddressSpace bus)
    {
        VerifySpriteSheetRecomposition(bus);
        InstalledSpriteSheets installed = SpriteSheetFiles.Load(RepositoryInstallation.Installation.SpriteSheetDirectory);

        ComposedSpriteSheet composed = EscapeTimerSheetComposer.Compose(bus);
        SpriteSheet timer = installed.Get(SpriteSheetId.EscapeTimer);
        AssertEqual(composed.Size, timer.Image.Size, "the installed timer image has the composed size");
        AssertTrue(timer.Image.Pixels.SequenceEqual(composed.Pixels), "the installed timer image holds the composed pixels");
        AssertTrue(timer.HoldsExactly(EscapeTimerLayoutDefinitions.Glyphs.Select(EscapeTimerComposition.FrameName)),
            "the installed timer sheet holds exactly the timer's frames");
        Console.WriteLine("Sprite sheet files: every sheet recomposes byte for byte; the composed timer sheet loads exactly.");
    }

    /// <summary>Composes the sheets again into a scratch directory and compares them with the installed files byte for byte.</summary>
    [PrivateInstallation("Checks SpriteSheetFiles.Extract itself: composing every sprite sheet again from the cartridge must reproduce the installed sheet images and frame manifests byte for byte, which only a fresh extraction can show.")]
    private static void VerifySpriteSheetRecomposition(ISnesAddressSpace bus)
    {
        DirectoryInfo scratch = Directory.CreateTempSubdirectory("sm-sprite-sheets-");
        try
        {
            string stock = Path.Combine(scratch.FullName, "stock");
            // The body sheets are named from the body tables installed beside them, the pause sheet from the menus'
            // selector presentation, the intro sheet from the intro's compositions and the ending sheets from the ending's,
            // which extraction reads from the shared installation's content directory.
            SpriteSheetFiles.Extract(bus, RepositoryInstallation.Installation.ContentDirectory, stock, SupportedCartridge.Sha256);
            // Composing the sheets again reproduces the installed files byte for byte.
            string installedDirectory = Path.Combine(RepositoryInstallation.Installation.ContentDirectory, GameInstallationLayout.SpriteSheetDirectoryName);
            string[] installedFiles = [.. Directory.GetFiles(installedDirectory).Select(Path.GetFileName).OfType<string>().Order(StringComparer.Ordinal)];
            AssertTrue(installedFiles.SequenceEqual(Directory.GetFiles(stock).Select(Path.GetFileName).OfType<string>().Order(StringComparer.Ordinal)),
                "the recomposed sprite sheets are the installed files");
            foreach (string name in installedFiles)
                AssertTrue(File.ReadAllBytes(Path.Combine(installedDirectory, name)).AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(stock, name))),
                    $"sprite sheet file {name} recomposes byte for byte");
        }
        finally
        {
            scratch.Delete(recursive: true);
        }
    }

    /// <summary>Runs the sprite sheet installation checks against the repository cartridge.</summary>
    [VerificationCommand("--sprite-sheet-files")]
    internal static int SpriteSheetFilesCommand()
    {
        Suite(nameof(VerifySpriteSheetFiles), () => VerifySpriteSheetFiles(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath(RepositoryRomPath))));
        return 0;
    }
}
