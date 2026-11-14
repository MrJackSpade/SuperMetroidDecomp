using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>
/// Checks the Speed Booster cycle on a step Screw Attack leaves past its four-step list (#1254, #1736): the step shows the
/// suit's own last Speed Booster shade, and the step, timer and four-frame hold are the original's.
/// </summary>
internal static class SpeedBoosterInheritedStepVerification
{
    /// <summary>Leaves Screw Attack on steps four and five in every suit and checks the Speed Booster copy that follows.</summary>
    internal static void VerifySpeedBoosterInheritedStep()
    {
        SamusEquipmentFlags[] suits = [0, SamusEquipmentFlags.VariaSuit, SamusEquipmentFlags.GravitySuit];
        foreach (SamusEquipmentFlags suitItems in suits)
        {
            SamusEquipmentFlags equipped = suitItems | SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.SpeedBooster;
            SamusSuit suit = equipped.ShownSuit();
            var lastPalette = new SamusFullBodyCyclePalette(SamusFullBodyCycleFamily.SpeedBooster, suit,
                SamusPaletteDefinitions.FullBodyCycles.SpeedBoosterLastStep);
            PictureShade lastShade = SamusShades.FullBodyCycle(lastPalette);
            foreach (int inherited in new[] { 4, 5 })
            {
                string context = $"{suit} step {inherited}";
                var speed = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus().WaterSpeedsInAir) { SpeedBoostCounter = new SamusSpeedBoostCounter(4, 0), SpecialPaletteTimer = 1 };
                var colours = new SamusColours();
                for (int call = 0; call < inherited; call++)
                    speed.UpdateSpeedBoosterPalette(colours, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.SpinJumping, AnimationFrame = 1, EquippedItems = equipped });
                AssertEqual(inherited, speed.CyclePaletteStep, $"{context}: Screw Attack leaves its shared step for the next movement handler");
                AssertTrue(speed.UpdateSpeedBoosterPalette(colours, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.NormalJumping, AnimationFrame = 0, EquippedItems = equipped }),
                    $"{context}: leaving Screw Attack while boosted copies a Speed Booster palette");
                AssertEqual(lastShade, colours.Suit, $"{context}: the inherited step shows the suit's own last Speed Booster shade");
                RgbColor[] shown = colours.ShownSuit(InstalledFixtures.SamusShadeSources);
                for (int colour = 0; colour < SamusColourSets.ColoursPerSet; colour++)
                    AssertEqual(InstalledFixtures.SamusShadeSources.FullBodyCycles.Resolve(lastPalette, colour), shown[colour],
                        $"{context}: suit colour {colour} is the suit's own last Speed Booster colour");
                AssertEqual(SamusPaletteDefinitions.FullBodyCycles.SpeedBoosterLastStep, speed.CyclePaletteStep, $"{context}: the step is clamped only after the copy");
                AssertEqual(4, speed.SpecialPaletteTimer, $"{context}: the four-frame timer is reloaded");
                for (int call = 0; call < 3; call++)
                {
                    AssertTrue(!speed.UpdateSpeedBoosterPalette(colours, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.NormalJumping, AnimationFrame = 0, EquippedItems = equipped }),
                        $"{context}: the shade holds until the timer expires");
                    AssertEqual(lastShade, colours.Suit, $"{context}: the suit's shade holds for four frames");
                }
                AssertTrue(speed.UpdateSpeedBoosterPalette(colours, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.NormalJumping, AnimationFrame = 0, EquippedItems = equipped }),
                    $"{context}: the next copy follows four frames later");
                AssertEqual(lastShade, colours.Suit, $"{context}: the cycle holds its last shade");
            }
        }
        Console.WriteLine("Speed Booster inherited step: Screw Attack steps four and five show each suit's own last shade, with the original step, timer and hold.");
    }

    /// <summary>Runs the focused Speed Booster inherited-step check.</summary>
    [VerificationCommand("--speed-booster-inherited-step")]
    internal static int SpeedBoosterInheritedStepCommand()
    {
        Suite(nameof(VerifySpeedBoosterInheritedStep), () => VerifySpeedBoosterInheritedStep());
        return 0;
    }
}
