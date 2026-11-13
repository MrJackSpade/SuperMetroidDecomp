using SuperMetroid.Core.Bosses.Draygon;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>
/// Confirms that Draygon's named random chances (#1619) decide every one of the 65536 samples exactly as the zero tests of
/// the sample fields they replace did: his goop volley's step and the tail whip he inserts while he holds Samus.
/// </summary>
internal static class DraygonRandomChancesVerification
{
    /// <summary>Compares every sample's named answers with the field tests they replace.</summary>
    internal static void VerifyDraygonRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
        {
            AssertSameAt((sample & 15) == 0, DraygonRandomDefinitions.GoopVolleySteps.IsTakenBy(sample), "goop volley step", sample);
            AssertSameAt((sample & 255) == 0, DraygonRandomDefinitions.TailWhips.IsTakenBy(sample), "tail whip", sample);
        }

        Console.WriteLine("Draygon random chances: all 65536 samples decide as the field tests they replace.");
    }
}
