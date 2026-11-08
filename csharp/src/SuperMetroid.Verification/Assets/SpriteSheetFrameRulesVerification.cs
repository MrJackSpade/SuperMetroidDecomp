using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Holds every installed Samus, projectile and frontend sprite sheet's frames to the sheet pixel rule (#1513): no frame
/// is empty, as <see cref="SheetInkRules"/> defines it. Each sheet's frame set is the
/// one its family's draw tables name, which loading the installation already requires; whether a frame is only a piece of
/// a body the actor publishes with others is <see cref="ActorPartDrawsVerification"/>'s rule.
/// </summary>
internal static class SpriteSheetFrameRulesVerification
{
    /// <summary>Checks every installed sprite sheet's frames against the sheet pixel rules.</summary>
    internal static void VerifySpriteSheetFrameRules() =>
        SheetInkRules.Assert("sprite", RepositoryInstallation.Installation.SpriteSheetDirectory,
            [.. SpriteSheetDefinitions.Sheets.Select(sheet => new LabelledSheet(sheet.ToString(), InstalledFixtures.SpriteSheets.Get(sheet)))]);

    /// <summary>Runs the installed sprite sheets' pixel rules.</summary>
    [VerificationCommand("--sprite-sheet-frame-rules")]
    internal static int SpriteSheetFrameRulesCommand()
    {
        Suite(nameof(VerifySpriteSheetFrameRules), VerifySpriteSheetFrameRules);
        return 0;
    }
}
