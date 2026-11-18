using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Colours;
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
            var lastPalette = new SamusFullBodyCycleShade(SamusFullBodyCycleFamily.SpeedBooster, suit,
                SamusShadeDefinitions.FullBodyCycles.SpeedBoosterLastStep);
            PictureShade lastShade = SamusShades.FullBodyCycle(lastPalette);
            foreach (int inherited in new[] { 4, 5 })
            {
                string context = $"{suit} step {inherited}";
                var speed = new SamusHorizontalSpeedState(SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus()) { SpeedBoostCounter = new SamusSpeedBoostCounter(4, 0) };
                PrivateState.Set(speed, nameof(SamusHorizontalSpeedState.CycleShadeTimer), 1);
                var colours = new SamusColours();
                for (int call = 0; call < inherited; call++)
                    speed.UpdateSpeedBoosterGlow(colours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.SpinJumping, AnimationOnSpinStart = (1) == 0, AnimationPastScrewAttackSpin = (1) >= 27, AnimationBeforeWallJumpSpin = (1) < 3, EquippedItems = equipped, SuppressActiveSpeedBoosterGlow = false, BottomBoundarySubmerged = false });
                AssertEqual(inherited, speed.CycleShadeStep, $"{context}: Screw Attack leaves its shared step for the next movement handler");
                AssertTrue(speed.UpdateSpeedBoosterGlow(colours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.NormalJumping, AnimationOnSpinStart = (0) == 0, AnimationPastScrewAttackSpin = (0) >= 27, AnimationBeforeWallJumpSpin = (0) < 3, EquippedItems = equipped, SuppressActiveSpeedBoosterGlow = false, BottomBoundarySubmerged = false }),
                    $"{context}: leaving Screw Attack while boosted copies a Speed Booster palette");
                AssertEqual(lastShade, colours.Suit, $"{context}: the inherited step shows the suit's own last Speed Booster shade");
                RgbColor[] shown = colours.ShownSuit(InstalledFixtures.SamusShadeSources);
                for (int colour = 0; colour < SamusColourSets.ColoursPerSet; colour++)
                    AssertEqual(InstalledFixtures.SamusShadeSources.FullBodyCycles.Resolve(lastPalette, colour), shown[colour],
                        $"{context}: suit colour {colour} is the suit's own last Speed Booster colour");
                AssertEqual(SamusShadeDefinitions.FullBodyCycles.SpeedBoosterLastStep, speed.CycleShadeStep, $"{context}: the step is clamped only after the copy");
                AssertEqual(4, speed.CycleShadeTimer, $"{context}: the four-frame timer is reloaded");
                for (int call = 0; call < 3; call++)
                {
                    AssertTrue(!speed.UpdateSpeedBoosterGlow(colours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.NormalJumping, AnimationOnSpinStart = (0) == 0, AnimationPastScrewAttackSpin = (0) >= 27, AnimationBeforeWallJumpSpin = (0) < 3, EquippedItems = equipped, SuppressActiveSpeedBoosterGlow = false, BottomBoundarySubmerged = false }),
                        $"{context}: the shade holds until the timer expires");
                    AssertEqual(lastShade, colours.Suit, $"{context}: the suit's shade holds for four frames");
                }
                AssertTrue(speed.UpdateSpeedBoosterGlow(colours, new SpeedBoosterGlowFrame { MovementType = SamusMovementType.NormalJumping, AnimationOnSpinStart = (0) == 0, AnimationPastScrewAttackSpin = (0) >= 27, AnimationBeforeWallJumpSpin = (0) < 3, EquippedItems = equipped, SuppressActiveSpeedBoosterGlow = false, BottomBoundarySubmerged = false }),
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
