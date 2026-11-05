using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Holds every installed enemy sheet to the sheet rules (#1513): no frame is empty or inks an empty colour row, as
/// <see cref="SheetInkRules"/> defines them. Each sheet's frame set is the one its enemy's frame table names, every frame
/// one whole picture, which loading the installation already requires.
/// </summary>
internal static class EnemySheetFrameRulesVerification
{
    /// <summary>Every installed enemy sheet, each with its graphics set and enemy, in a stable order.</summary>
    private static IEnumerable<KeyValuePair<EnemySheetKey, SpriteSheet>> Sheets =>
        InstalledFixtures.EnemySheets.sheets.OrderBy(entry => entry.Key.Set, StringComparer.Ordinal).ThenBy(entry => entry.Key.Enemy);

    /// <summary>Checks every installed enemy sheet's frames against the sheet pixel rules.</summary>
    internal static void VerifyEnemySheetInks() =>
        SheetInkRules.Assert("enemy", RepositoryInstallation.Installation.EnemySheetDirectory,
            [.. Sheets.Select(entry => new LabelledSheet($"{entry.Key.Set} {entry.Key.Enemy}", entry.Value))]);

    /// <summary>Runs the installed enemy sheets' pixel rules.</summary>
    [VerificationCommand("--enemy-sheet-frame-rules")]
    internal static int EnemySheetFrameRulesCommand()
    {
        Suite(nameof(VerifyEnemySheetInks), VerifyEnemySheetInks);
        return 0;
    }
}
