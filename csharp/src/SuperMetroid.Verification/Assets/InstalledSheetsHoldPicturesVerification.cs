using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Holds every installed sheet to the rule that a sheet exists only to show pictures (#1513): no installed sprite, enemy,
/// spawned-object or boss body sheet holds no picture at all. A sheet whose owner draws nothing has nothing to install; the
/// frame rules of <see cref="SheetPixelRules"/> only look at the frames a sheet holds, so an empty sheet passes them.
/// </summary>
internal static class InstalledSheetsHoldPicturesVerification
{
    /// <summary>Fails, naming every installed sheet that holds no picture.</summary>
    internal static void VerifyInstalledSheetsHoldPictures()
    {
        var sheets = new List<LabelledSheet>();
        sheets.AddRange(SpriteSheetDefinitions.Sheets.Select(sheet => new LabelledSheet($"sprite sheet {sheet}", InstalledFixtures.SpriteSheets.Get(sheet))));
        sheets.AddRange(InstalledFixtures.EnemySheets.variants.OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new LabelledSheet($"enemy variant {entry.Key}", entry.Value.Sheet)));
        sheets.AddRange(InstalledFixtures.EnemySheets.spawnedVariants.OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .SelectMany(entry => entry.Value.Sheet.Sheets.Select(sheet => new LabelledSheet($"spawned-object variant {entry.Key}", sheet))));
        sheets.AddRange(Enum.GetValues<BossBodyOwner>()
            .Select(owner => new LabelledSheet($"boss body sheet {owner}", InstalledFixtures.BossBodySheets.Of(owner).Sheet)));
        string[] empty = [.. sheets.Where(sheet => sheet.Sheet.frames.Count == 0)
            .Select(sheet => $"{sheet.Label} ({sheet.Sheet.Image.Name}) holds no picture")];
        AssertTrue(empty.Length == 0,
            $"{empty.Length} installed sheets hold no picture:{Environment.NewLine}{string.Join(Environment.NewLine, empty)}");
        Console.WriteLine($"  Installed sheets: {sheets.Count} sheets each hold a picture.");
    }

    /// <summary>Runs the installed sheets' empty-sheet rule.</summary>
    [VerificationCommand("--installed-sheets-hold-pictures")]
    internal static int InstalledSheetsHoldPicturesCommand()
    {
        Suite(nameof(VerifyInstalledSheetsHoldPictures), VerifyInstalledSheetsHoldPictures);
        return 0;
    }
}
