using SuperMetroid.Core.Bosses.Botwoon;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Botwoon;

/// <summary>
/// Confirms that the chance Botwoon starts his next path inside the hole (#2479) decides every one of the 65536 samples
/// exactly as the test of the sample's lowest bit it replaces did.
/// </summary>
internal static class BotwoonRandomChancesVerification
{
    /// <summary>Compares every sample's named answer with the bit test it replaces.</summary>
    internal static void VerifyBotwoonRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 1) != 0, BotwoonNavigationDefinitions.StartsInsideHole.IsTakenBy(sample), "path starts inside the hole", sample);

        Console.WriteLine("Botwoon random chances: all 65536 samples decide as the bit test they replace.");
    }
}
