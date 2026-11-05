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
/// <see cref="SheetInkRules"/> defines them, and no frame is a part image, a piece of an extended frame the draw path
/// places in more than one piece (<see cref="ExtendedFrameParts"/>). Each sheet's frame set is the one its enemy's frame
/// table names, which loading the installation already requires.
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

    /// <summary>
    /// Checks that no installed enemy sheet holds a part image: a frame that pictures one piece of an extended frame the
    /// draw path places in more than one piece, or a picture of such a piece at an artwork step or a corpse's rot.
    /// </summary>
    internal static void VerifyEnemySheetPartImages()
    {
        EnemyExtendedFramePlacementCatalog placements = InstalledFixtures.EnemyTiles.ExtendedFramePlacements
            ?? throw new InvalidOperationException("Verification requires the installed extended frame placements.");
        IReadOnlyDictionary<string, ExtendedFramePart> parts = ExtendedFrameParts.Of(placements);
        var checkedSheets = new HashSet<SpriteSheet>(ReferenceEqualityComparer.Instance);
        var failures = new List<string>();
        int frames = 0;
        foreach ((EnemySheetKey key, SpriteSheet sheet) in Sheets)
        {
            if (!checkedSheets.Add(sheet))
                continue;
            frames += sheet.frames.Count;
            string[] partImages = [.. sheet.frames.Keys.Where(name => parts.ContainsKey(ExtendedFrameParts.PicturedFrame(name))).Order(StringComparer.Ordinal)];
            if (partImages.Length > 0)
                failures.Add($"{key.Set} {key.Enemy} ({sheet.Image.Name}) holds {partImages.Length} part images: {string.Join(", ", partImages)}");
        }
        AssertTrue(checkedSheets.Count > 0, "the installation holds enemy sheets");
        AssertTrue(failures.Count == 0,
            $"{failures.Count} installed enemy sheets hold pieces of frames drawn in several pieces:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Enemy sheet part images: {frames} frames of {checkedSheets.Count} installed enemy sheets each picture a whole frame.");
    }

    /// <summary>Runs the installed enemy sheets' pixel and part-image rules.</summary>
    [VerificationCommand("--enemy-sheet-frame-rules")]
    internal static int EnemySheetFrameRulesCommand()
    {
        Suite(nameof(VerifyEnemySheetInks), VerifyEnemySheetInks);
        Suite(nameof(VerifyEnemySheetPartImages), VerifyEnemySheetPartImages);
        return 0;
    }
}
