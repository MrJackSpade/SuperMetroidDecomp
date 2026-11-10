using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Holds every installed spawned-object sheet (enemy projectiles, room sprite objects and the frames encounters draw
/// themselves) to the sheet pixel rule (#1513): no frame is empty, as <see cref="SheetInkRules"/> defines it. Each sheet's frame set is the one its spawners' tables name, which loading
/// the installation already requires.
/// </summary>
internal static class SpawnedObjectSheetFrameRulesVerification
{
    /// <summary>Checks every installed spawned-object sheet's frames against the sheet pixel rules.</summary>
    internal static void VerifySpawnedObjectSheetInks() =>
        SheetInkRules.Assert("spawned-object", RepositoryInstallation.Installation.EnemySheetDirectory,
            [.. InstalledFixtures.EnemySheets.spawnedVariants.OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .SelectMany(entry => entry.Value.Sheet.Sheets.Select(sheet => new LabelledSheet($"spawned {entry.Key}", sheet)))]);

    /// <summary>Runs the installed spawned-object sheets' pixel rules.</summary>
    [VerificationCommand("--spawned-object-sheet-frame-rules")]
    internal static int SpawnedObjectSheetFrameRulesCommand()
    {
        Suite(nameof(VerifySpawnedObjectSheetInks), VerifySpawnedObjectSheetInks);
        return 0;
    }
}
