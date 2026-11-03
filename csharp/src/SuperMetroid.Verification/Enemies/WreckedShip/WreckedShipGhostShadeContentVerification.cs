using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.WreckedShip;

/// <summary>
/// Checks the Wrecked Ship ghost's fade (#1512): the compatibility value's brightening carries a channel passing full
/// into the next, losing blue's carry, as the cartridge's addition of one level to all three channels of a packed colour
/// does; and the installed Coven sheet's shade for every step the ghost passes through shows that step's colours in the
/// ghost's row.
/// </summary>
internal static class WreckedShipGhostShadeContentVerification
{
    /// <summary>Checks the brightening's carry, then every installed fade shade against the value's colours.</summary>
    internal static void VerifyWreckedShipGhostShadeContent()
    {
        // Colour zero of the ghost's own colours is (0, 0, 14): materialized, then brightened 17 times, its blue is full;
        // the next brightening carries blue out of the colour, leaving it at zero, as the word addition does.
        var fade = new WreckedShipGhostFade();
        fade.Darken();
        fade.BeginMaterializing();
        for (int moved = 1; moved != 0;)
            moved = fade.StepTowardTarget();
        AssertEqual(WreckedShipGhostAppearance.PaletteColor(0), fade.Shown[0], "the materialized ghost shows its own colour zero");
        for (int step = 0; step < 17; step++)
            fade.Brighten();
        AssertEqual(new RgbColor(17, 17, 31), fade.Shown[0], "seventeen brightenings fill colour zero's blue");
        fade.Brighten();
        AssertEqual(new RgbColor(18, 18, 0), fade.Shown[0], "the next brightening carries blue out of the colour");
        AssertEqual(new RgbColor(31, 0, 0), fade.Shown[10], "a colour whose red is full is not brightened");

        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetLists.All.First(candidate =>
            candidate.Records.ToArray().Any(record => record.DefinitionId == EnemyDefinitionId.Coven));
        int first = new GraphicsSetRows(set).RowOf(EnemyDefinitionId.Coven) * LoadedSetColours.RowColours;
        SpriteImage image = InstalledFixtures.EnemySheets.For(set, EnemyDefinitionId.Coven).Image;
        IReadOnlyList<(PictureShade Shade, RgbColor[] Colours)> steps = EnemySheetShades.GhostFadeSteps();
        AssertTrue(steps.Count > 3 * 31, "the ghost's path passes through its brightening and both fades");
        foreach ((PictureShade shade, RgbColor[] colours) in steps)
        {
            RgbColor[] inks = PictureShading.Shaded(shade).InkColours(image);
            for (int color = 0; color < WreckedShipGhostFade.ColourCount; color++)
                AssertEqual(colours[color], inks[first + color], $"ghost shade {shade.Key} ink {color} shows the fade's colour");
        }
        Console.WriteLine($"Wrecked Ship ghost shade content: the brightening's carry and {steps.Count} installed fade shades in {set.Name} match the fade.");
    }

    /// <summary>Runs the focused Wrecked Ship ghost shade-content comparison.</summary>
    [VerificationCommand("--wrecked-ship-ghost-shade-content")]
    internal static int WreckedShipGhostShadeContentCommand()
    {
        Suite(nameof(VerifyWreckedShipGhostShadeContent), () => VerifyWreckedShipGhostShadeContent());
        return 0;
    }
}
