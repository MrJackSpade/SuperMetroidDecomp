using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Confirms the #1684 typed Spazer turns against the signed steps they replace: every signed step the cartridge stores
/// turns all 256 angles as the typed turn does, and the turn's signed step is the stored word.
/// </summary>
internal static class SpazerTurnsVerification
{
    /// <summary>Checks the Spazer particles' turn magnitudes and every stored signed step's turn.</summary>
    internal static void VerifySpazerTurns()
    {
        AssertEqual(new TrigonometryAngle(4), BeamComboDefinitions.SpazerCirclingTurn, "Spazer circling turn");
        AssertEqual(new TrigonometryAngle(2), BeamComboDefinitions.SpazerFlyingTurn, "Spazer flying turn");
        foreach (int step in new[] { 4, -4, 2, -2 })
        {
            TrigonometryAngle turn = TrigonometryAngle.Wrapped(step);
            AssertEqual(step, turn.StepsFrom(TrigonometryAngle.Zero), $"Spazer turn of {step} keeps its signed step");
            for (int angle = 0; angle < 256; angle++)
                AssertEqual(new TrigonometryAngle(angle).TurnedBySteps(step), new TrigonometryAngle(angle) + turn, $"Spazer turn of {step} from angle {angle}");
        }
        Console.WriteLine("  Spazer turns: each stored signed step turns all 256 angles as its typed turn does and is the turn's signed step.");
    }
}
