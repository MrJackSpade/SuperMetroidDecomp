using SuperMetroid.Core.Bosses.Kraid;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// Confirms that Kraid's single-bit random chances (#2479) and his thinking delay's choice (#2510) decide every one of the
/// 65536 samples exactly as the expressions they replace did: a nail waiting for the lint, a rising rock's pose and side,
/// and the steps he thinks for.
/// </summary>
internal static class KraidRandomChancesVerification
{
    /// <summary>Compares every sample's named answers with the bit tests they replace.</summary>
    internal static void VerifyKraidRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
        {
            AssertSameAt((sample & 1) != 0, KraidRandomDefinitions.NailWaitsForLint.IsTakenBy(sample), "nail waits for the lint", sample);
            AssertSameAt((sample & 16) != 0, KraidRandomDefinitions.RisingRockFliesRight.IsTakenBy(sample), "rising rock flies right", sample);
            AssertSameAt((sample & 1) != 0, KraidRandomDefinitions.RisingRockStartsOnRight.IsTakenBy(sample), "rising rock starts on the right", sample);
            AssertSameAt((sample & 7) == 0 ? 2 : sample & 7, KraidRandomDefinitions.ThinkingDelaySteps.Choose(sample), "thinking delay steps", sample);
        }

        Console.WriteLine("Kraid random chances: all 65536 samples decide as the expressions they replace.");
    }
}
