using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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
        var failures = new List<string>();
        int sheets = 0;
        foreach ((EnemyDefinitionId enemy, EnemyArtworkVariant<SpriteSheet> variant) in InstalledFixtures.EnemySheets.EnemyVariants
            .Where(entry => EntryColourings.ContainsKey(entry.Key)))
        {
            sheets++;
            SpriteSheet sheet = variant.Sheet;
            SpriteImage image = sheet.Image;
            HashSet<ColourKey> painted = PaintedKeys(image, sheet);
            foreach (PictureShade shade in EntryColourings[enemy].Shades)
            {
                foreach (KeyedSubstitution substitution in image.Shade(shade).Substitutions)
                {
                    if (painted.Contains(substitution.Replaced) && substitution.Replaced.Colour != substitution.Shown)
                        failures.Add($"{variant.Name} ({image.Name}) shows colour key {substitution.Replaced}, "
                            + $"which its entry colouring {shade.Key} draws as {substitution.Shown}");
                }
            }
        }
        AssertTrue(sheets > 0, "the installation holds a sheet of an enemy drawn in its entry colouring");
        AssertTrue(failures.Count == 0,
            $"{failures.Count} colours of installed enemy sheets are colours their enemies are never drawn in:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Enemy sheet entry colourings: {sheets} installed sheets of enemies drawn in their entry colouring show it.");
    }

    /// <summary>Every colour key the opaque pixels of the frames of <paramref name="sheet"/> show on <paramref name="image"/>.</summary>
    private static HashSet<ColourKey> PaintedKeys(SpriteImage image, SpriteSheet sheet)
    {
        ReadOnlySpan<RgbaPixel> pixels = image.Pixels;
        var painted = new HashSet<ColourKey>();
        foreach (SpriteSheetFrame frame in sheet.frames.Values)
        {
            for (int row = 0; row < frame.Size.Y; row++)
            {
                for (int column = 0; column < frame.Size.X; column++)
                {
                    int pixel = (frame.Corner.Y + row) * image.Size.X + frame.Corner.X + column;
                    if (pixels[pixel].A != 0)
                        painted.Add(image.KeyAt(pixel));
                }
            }
        }
        return painted;
    }

    /// <summary>Runs the entry-colouring rule of the installed enemy sheets.</summary>
    [VerificationCommand("--enemy-sheet-entry-colourings")]
    internal static int EnemySheetEntryColouringsCommand()
    {
        Suite(nameof(VerifyEnemySheetEntryColourings), VerifyEnemySheetEntryColourings);
        return 0;
    }
}
