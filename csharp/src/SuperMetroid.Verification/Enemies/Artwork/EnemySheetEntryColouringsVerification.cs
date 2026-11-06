using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Holds the sheets of every enemy whose initialization leaves it drawn in its encounter's entry colouring to that
/// colouring (#1513): such an enemy is never drawn in its sheet's standard colours, so its sheet's pictures, the ones a
/// reviewer sees and an editor changes, show it in the entry colouring, and drawing it in that colouring changes no pixel.
/// The Shitroid and the Sidehopper it drains are those enemies: their initializers draw both in the Shitroid encounter's
/// target colours from the room's start, while the room's graphics set loads all three of its records into one row, so
/// the Shitroid's own draw row and its victim's hold colours neither is ever drawn in.
/// </summary>
internal static class EnemySheetEntryColouringsVerification
{
    /// <summary>Each enemy whose initialization leaves it drawn in an entry colouring, with that colouring.</summary>
    private static readonly Dictionary<EnemyDefinitionId, PictureShading> EntryColourings = new()
    {
        [EnemyDefinitionId.BabyMetroid] = ShitroidShades.InTargetColours,
        [EnemyDefinitionId.CorpseSidehopper] = ShitroidShades.InTargetColours,
    };

    /// <summary>Fails, naming each sheet colour that differs, when such an enemy's sheet shows a colour its entry colouring replaces.</summary>
    internal static void VerifyEnemySheetEntryColourings()
    {
        string directory = RepositoryInstallation.Installation.EnemySheetDirectory;
        var failures = new List<string>();
        int sheets = 0;
        foreach ((EnemySheetKey key, SpriteSheet sheet) in InstalledFixtures.EnemySheets.sheets
            .Where(entry => EntryColourings.ContainsKey(entry.Key.Enemy))
            .OrderBy(entry => entry.Key.Set, StringComparer.Ordinal).ThenBy(entry => entry.Key.Enemy))
        {
            sheets++;
            SpriteImage image = sheet.Image;
            HashSet<int> inked = InkedColours(SheetInkRules.InkPlane(directory, image), image, sheet);
            foreach (PictureShade shade in EntryColourings[key.Enemy].Shades)
            {
                ShadeInks shown = image.Shade(shade);
                for (int index = 0; index < shown.Inks.Length; index++)
                {
                    int ink = shown.Inks[index];
                    if (inked.Contains(ink) && image.StandardColours[ink] != shown.Colours[index])
                        failures.Add($"{key.Set} {key.Enemy} ({image.Name}) shows colour {ink} as {image.StandardColours[ink]}, "
                            + $"which its entry colouring {shade.Key} draws as {shown.Colours[index]}");
                }
            }
        }
        AssertTrue(sheets > 0, "the installation holds a sheet of an enemy drawn in its entry colouring");
        AssertTrue(failures.Count == 0,
            $"{failures.Count} colours of installed enemy sheets are colours their enemies are never drawn in:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Enemy sheet entry colourings: {sheets} installed sheets of enemies drawn in their entry colouring show it.");
    }

    /// <summary>Every colour the frames of <paramref name="sheet"/> ink on <paramref name="image"/>, from its ink plane <paramref name="inks"/>.</summary>
    private static HashSet<int> InkedColours(byte[] inks, SpriteImage image, SpriteSheet sheet)
    {
        var inked = new HashSet<int>();
        foreach (SpriteSheetFrame frame in sheet.frames.Values)
        {
            for (int row = 0; row < frame.Size.Y; row++)
            {
                for (int column = 0; column < frame.Size.X; column++)
                {
                    int ink = inks[(frame.Corner.Y + row) * image.Size.X + frame.Corner.X + column];
                    if (!image.IsColourZero(new SpriteImageInk(ink, frontTile: false)))
                        inked.Add(ink);
                }
            }
        }
        return inked;
    }

    /// <summary>Runs the entry-colouring rule of the installed enemy sheets.</summary>
    [VerificationCommand("--enemy-sheet-entry-colourings")]
    internal static int EnemySheetEntryColouringsCommand()
    {
        Suite(nameof(VerifyEnemySheetEntryColourings), VerifyEnemySheetEntryColourings);
        return 0;
    }
}
