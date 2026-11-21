using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Puyo;
namespace SuperMetroid.Verification.Enemies.Puyo;

using SuperMetroid.Verification.Harness;

/// <summary>Checks the Puyo state the species owns: the airborne place's two sign-bit restarts.</summary>
internal static class PuyoOwnedStateVerification
{
    /// <summary>
    /// Confirms a rising step landing on zero keeps rising, one that passes below zero restarts the fall at zero (the
    /// apex), a falling step that reaches $8000 restarts the fall at zero as the cartridge's sign test does, an
    /// ordinary falling step accumulates.
    /// </summary>
    internal static void VerifyPuyoOwnedState()
    {
        static QuadraticSpeedCurvePlace Place(int twoHundredFiftySixths) => QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(twoHundredFiftySixths);
        // The airborne update steps the place the way the Puyo moves: up while it rises, down while it falls.
        static void Step(PuyoEnemyState hop, int twoHundredFiftySixths) =>
            hop.StepAirbornePlace(Place(twoHundredFiftySixths), hop.Action == PuyoAction.Rising ? VerticalDirection.Up : VerticalDirection.Down);
        var hop = new PuyoEnemyState(PuyoAction.Rising) { YSpeedCurvePlace = Place(0x0200) };
        Step(hop, 0x0200);
        AssertEqual(new VerifyPuyoOwnedStateCase(0, PuyoAction.Rising), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfMark, hop.Action), "a rising step landing on zero keeps rising");
        Step(hop, 0x0200);
        AssertEqual(new VerifyPuyoOwnedStateCase(0, PuyoAction.Falling), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfMark, hop.Action), "a rising step below zero is the apex");
        Step(hop, 0x01c0);
        AssertEqual(new VerifyPuyoOwnedStateCase(0x01c0, PuyoAction.Falling), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfMark, hop.Action), "a falling step accumulates");
        hop.YSpeedCurvePlace = Place(0x7f00);
        Step(hop, 0x0100);
        AssertEqual(new VerifyPuyoOwnedStateCase(0, PuyoAction.Falling), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfMark, hop.Action), "a falling step reaching the sign bit restarts the fall");
        hop.YSpeedCurvePlace = Place(0x7e00);
        Step(hop, 0x0100);
        AssertEqual(new VerifyPuyoOwnedStateCase(0x7f00, PuyoAction.Falling), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfMark, hop.Action), "a falling step below the sign bit keeps falling");
        hop.YSpeedCurvePlace = Place(0x7f00);
        Step(hop, 0x00ff);
        AssertEqual(new VerifyPuyoOwnedStateCase(0x7fff, PuyoAction.Falling), new VerifyPuyoOwnedStateCase(hop.YSpeedCurvePlace.InTwoHundredFiftySixthsOfMark, hop.Action), "a falling step to the last place before the sign bit keeps falling");
        Console.WriteLine("Puyo owned state: the apex and the overlong-fall restart match.");
    }

    /// <summary>Runs the focused Puyo owned-state check.</summary>
    [VerificationCommand("--puyo-owned-state")]
    internal static int PuyoOwnedStateCommand()
    {
        VerifyPuyoOwnedState();
        return 0;
    }
}
