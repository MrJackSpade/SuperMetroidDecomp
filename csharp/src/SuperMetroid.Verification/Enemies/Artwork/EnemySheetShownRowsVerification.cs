using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Checks that no installed enemy or spawned-object sheet shows a frame in a colour row its colouring leaves empty (#1513):
/// every pixel of every frame inks a colour whose row in the sheet's manifest colours holds a colour other than black
/// beyond its transparent colour zero. A sheet coloured in the zero colours of an enemy whose encounter sets its colours
/// in code, as the Baby's and Ridley's were, shows only black and fails.
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
            if (checkedSheets.Add(spawned.Sheet))
                frames += CheckSheet(spawned.Sheet, $"{key.Set} spawned {key.Owner}", failures);
        }
        AssertTrue(checkedSheets.Count > 0, "the installation holds enemy sheets");
        AssertTrue(failures.Count == 0,
            $"{failures.Count} installed sheet frames ink colours whose manifest row is empty:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Enemy sheet shown rows: {frames} frames of {checkedSheets.Count} installed enemy and spawned-object sheets ink only filled colour rows.");
    }

    /// <summary>
    /// Records in <paramref name="failures"/> each frame of <paramref name="sheet"/> that inks a colour whose row of the
    /// sheet's manifest colours is black beyond colour zero, and returns how many frames it checked.
    /// </summary>
    private static int CheckSheet(SpriteSheet sheet, string label, List<string> failures)
    {
        SpriteImage image = sheet.Image;
        ReadOnlySpan<RgbColor> colours = image.StandardColours;
        bool[] emptyRows = new bool[colours.Length / RowColours];
        for (int row = 0; row < emptyRows.Length; row++)
        {
            emptyRows[row] = true;
            for (int colour = 1; colour < RowColours; colour++)
                emptyRows[row] &= colours[row * RowColours + colour] == RgbColor.Black;
        }
        ReadOnlySpan<SpriteImageInk> inks = image.Inks;
        int width = image.Size.X;
        foreach ((string name, SpriteSheetFrame frame) in sheet.frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            var rows = new SortedSet<int>();
            for (int y = 0; y < frame.Size.Y; y++)
            {
                for (int x = 0; x < frame.Size.X; x++)
                {
                    int ink = inks[(frame.Corner.Y + y) * width + frame.Corner.X + x].PaletteIndex;
                    if (ink % RowColours != 0 && emptyRows[ink / RowColours])
                        rows.Add(ink / RowColours);
                }
            }
            if (rows.Count > 0)
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
