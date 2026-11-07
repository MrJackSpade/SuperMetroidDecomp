using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks that no installed enemy or spawned-object sheet shows a frame in a colour row its colouring leaves empty (#1513):
/// every colour group every frame was coloured from has a row in the sheet's manifest colours holding a colour other
/// than black beyond its transparent colour zero. It reads the manifests only and decodes no image. A sheet coloured in
/// the zero colours of an enemy whose encounter sets its colours in code, as the Baby's and Ridley's were, shows only
/// black and fails.
/// </summary>
internal static class EnemySheetShownRowsVerification
{
    /// <summary>Colours in one sprite palette row.</summary>
    private const int RowColours = 16;

    /// <summary>Checks every installed enemy and spawned-object sheet's frames against their manifest colour rows.</summary>
    internal static void VerifyEnemySheetShownRows()
    {
        InstalledEnemySheets installed = InstalledFixtures.EnemySheets;
        var checkedSheets = new HashSet<SpriteSheet>(ReferenceEqualityComparer.Instance);
        var failures = new List<string>();
        int frames = 0;
        foreach ((EnemySheetKey key, SpriteSheet sheet) in installed.sheets)
        {
            if (checkedSheets.Add(sheet))
                frames += CheckSheet(sheet, $"{key.Set} {key.Enemy}", failures);
        }
        foreach ((SpawnedObjectSheetKey key, SpawnedObjectSheet spawned) in installed.spawned)
        {
            // A set's spawned-object pictures are on its own sheet and the common sheets (#1513), each checked once.
            foreach (SpriteSheet sheet in spawned.Sheets.Where(checkedSheets.Add))
                frames += CheckSheet(sheet, $"{key.Set} spawned {key.Owner}", failures);
        }
        AssertTrue(checkedSheets.Count > 0, "the installation holds enemy sheets");
        AssertTrue(failures.Count == 0,
            $"{failures.Count} installed sheet frames ink colours whose manifest row is empty:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Enemy sheet shown rows: {frames} frames of {checkedSheets.Count} installed enemy and spawned-object sheets ink only filled colour rows.");
    }

    /// <summary>
    /// Records in <paramref name="failures"/> each frame of <paramref name="sheet"/> coloured from a colour group whose row
    /// of the sheet's manifest colours is black beyond colour zero, and returns how many frames it checked. It reads only
    /// the manifest: each frame names the groups its parts were coloured in, the rows its parts' own rows with the sheet's
    /// palette bits ORed in name, and the manifest's colours for such a row are the colours that row draws in, so no
    /// pixel is decoded.
    /// </summary>
    private static int CheckSheet(SpriteSheet sheet, string label, List<string> failures)
    {
        SpriteImage image = sheet.Image;
        ReadOnlySpan<RgbColor> colours = image.StandardColours;
        var emptyGroups = new List<ImageColourGroup>();
        foreach (ImageColourGroup group in ImageColourGroup.Every)
        {
            bool empty = true;
            for (int colour = 1; colour < RowColours; colour++)
                empty &= colours[group.Number * RowColours + colour] == RgbColor.Black;
            if (empty)
                emptyGroups.Add(group);
        }
        foreach ((string name, SpriteSheetFrame frame) in sheet.frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            int[] rows = [.. emptyGroups.Where(frame.ColourGroups.Contains).Select(group => group.Number)];
            if (rows.Length > 0)
                failures.Add($"{label} ({image.Name}) frame {name} inks empty rows {string.Join(", ", rows)}");
        }
        return sheet.frames.Count;
    }

    /// <summary>Runs the installed enemy sheets' shown-row check.</summary>
    [VerificationCommand("--enemy-sheet-shown-rows")]
    internal static int EnemySheetShownRowsCommand()
    {
        Suite(nameof(VerifyEnemySheetShownRows), VerifyEnemySheetShownRows);
        return 0;
    }
}
