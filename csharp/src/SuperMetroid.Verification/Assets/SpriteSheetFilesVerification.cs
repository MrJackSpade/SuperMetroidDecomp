using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms the installed sprite sheets (#1392): the installed timer sheet loads holding exactly the composed pixels and
/// frames. That composing every sheet again reproduces the installed files is <see cref="SheetFilesRecompositionVerification"/>'s
/// check, as the sheets' manifests and the picture store are written for every sheet family at once (#1635).
/// </summary>
internal static class SpriteSheetFilesVerification
{
    /// <summary>Checks that the installed timer sheet loads as composed.</summary>
    internal static void VerifySpriteSheetFiles(ISnesAddressSpace bus)
    {
        InstalledSpriteSheets installed = SpriteSheetFiles.Load(RepositoryInstallation.Installation.SpriteSheetDirectory);

        ComposedSpriteSheet composed = EscapeTimerSheetComposer.Compose(bus);
        SpriteSheet timer = installed.Get(SpriteSheetId.EscapeTimer);
        AssertEqual(composed.Size, timer.Image.Size, "the installed timer image has the composed size");
        AssertTrue(AssembledSheetImages.HoldsPixels(timer.Image, AssembledSheetImages.Assemble(InstalledSheetConversion.Install(
                composed.Manifest, composed.Size, composed.Pixels, composed.PaletteIndices))),
            "the installed timer image holds the composed pixels as the import installs them");
        AssertTrue(timer.HoldsExactly(EscapeTimerLayoutDefinitions.Glyphs.Select(EscapeTimerComposition.FrameName)),
            "the installed timer sheet holds exactly the timer's frames");
        Console.WriteLine("Sprite sheet files: the composed timer sheet loads exactly.");
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
