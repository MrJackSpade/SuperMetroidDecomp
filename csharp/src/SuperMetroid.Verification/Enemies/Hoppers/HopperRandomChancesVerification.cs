using SuperMetroid.Core.Enemies.Hoppers;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Hoppers;

/// <summary>
/// Confirms that the chance a Sidehopper prepares a big hop (#2479) decides every one of the 65536 samples exactly as
/// the test of the sample's lowest bit it replaces did.
/// </summary>
internal static class HopperRandomChancesVerification
{
    /// <summary>Compares every sample's named answer with the bit test it replaces.</summary>
    internal static void VerifyHopperRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 1) != 0, HopperDecisions.HopsBig.IsTakenBy(sample), "big hop", sample);

        Console.WriteLine("Sidehopper random chances: all 65536 samples decide as the bit test they replace.");
    }
}
