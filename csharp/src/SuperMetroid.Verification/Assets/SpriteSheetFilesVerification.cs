using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>Confirms the installed sprite sheets (#1392): the installed timer sheet holds exactly the timer's frames.</summary>
internal static class SpriteSheetFilesVerification
{
    /// <summary>Checks that the installed timer sheet holds exactly the timer's frames.</summary>
    internal static void VerifySpriteSheetFiles()
    {
        InstalledSpriteSheets installed = SpriteSheetFiles.Load(RepositoryInstallation.Installation.SpriteSheetDirectory);
        SpriteSheet timer = installed.Get(SpriteSheetId.EscapeTimer);
        AssertTrue(timer.HoldsExactly(EscapeTimerLayoutDefinitions.Glyphs.Select(EscapeTimerComposition.FrameName)),
            "the installed timer sheet holds exactly the timer's frames");
        Console.WriteLine("Sprite sheet files: the installed timer sheet holds exactly the timer's frames.");
    }

    /// <summary>Runs the sprite sheet installation checks.</summary>
    [VerificationCommand("--sprite-sheet-files")]
    internal static int SpriteSheetFilesCommand()
    {
        Suite(nameof(VerifySpriteSheetFiles), VerifySpriteSheetFiles);
        return 0;
    }
}
