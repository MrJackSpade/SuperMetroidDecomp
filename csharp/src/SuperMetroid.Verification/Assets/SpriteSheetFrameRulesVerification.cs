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
/// Holds every installed Samus, projectile and frontend sprite sheet's frames to the sheet pixel rules (#1513): no frame
/// is empty and none inks an empty colour row, as <see cref="SheetInkRules"/> defines them. Each sheet's frame set is the
/// one its family's draw tables name, which loading the installation already requires; whether a frame is only a piece of
/// a body the actor publishes with others is <see cref="ActorPartDrawsVerification"/>'s rule.
/// </summary>
internal static class SpriteSheetFrameRulesVerification
{
    /// <summary>Checks every installed sprite sheet's frames against the sheet pixel rules.</summary>
    internal static void VerifySpriteSheetFrameRules() =>
        SheetInkRules.Assert("sprite", RepositoryInstallation.Installation.SpriteSheetDirectory,
            [.. SpriteSheetDefinitions.Sheets.Select(sheet => new LabelledSheet(sheet.ToString(), InstalledFixtures.SpriteSheets.Get(sheet))
            {
                DefinedSets = ProjectileSheets.Contains(sheet) ? [BeamSet] : [],
            })]);

    /// <summary>
    /// The projectile sheets: the intro's and each beam combination's the game shows, retail and glitched. Each is coloured
    /// with its combination's beam palette in the beams' colour set, which that palette always defines.
    /// </summary>
    private static readonly HashSet<SpriteSheetId> ProjectileSheets =
        [SpriteSheetId.IntroProjectiles, .. SamusBeamColours.Shown.Select(ProjectileSheetFrames.For)];

    /// <summary>The colour set Samus's beam palette fills on the sheets coloured with it.</summary>
    private static readonly int BeamSet = SamusSheetInks.Beams / ImageColourGroup.Colours;

    /// <summary>Runs the installed sprite sheets' pixel rules.</summary>
    [VerificationCommand("--sprite-sheet-frame-rules")]
    internal static int SpriteSheetFrameRulesCommand()
    {
        Suite(nameof(VerifySpriteSheetFrameRules), VerifySpriteSheetFrameRules);
        return 0;
    }
}
