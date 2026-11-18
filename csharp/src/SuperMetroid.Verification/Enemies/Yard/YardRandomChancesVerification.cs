using SuperMetroid.Core.Enemies.Yard;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>
/// Confirms that the chance a Yard keeps its closed shell for another update (#2479) decides every one of the 65536
/// samples exactly as the test of the sample's lowest bit it replaces did.
/// </summary>
internal static class YardRandomChancesVerification
{
    /// <summary>Compares every sample's named answer with the bit test it replaces.</summary>
    internal static void VerifyYardRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 1) != 0, YardDecisions.HoldsShellClosedAgain.IsTakenBy(sample), "shell held closed again", sample);

        Console.WriteLine("Yard random chances: all 65536 samples decide as the bit test they replace.");
    }
}
