using SuperMetroid.Core.Enemies.Beetom;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Beetom;

/// <summary>
/// Confirms that the chance a distant Beetom faces right (#2479) decides every one of the 65536 samples exactly as the
/// test of the sample's lowest bit it replaces did.
/// </summary>
internal static class BeetomRandomChancesVerification
{
    /// <summary>Compares every sample's named answer with the bit test it replaces.</summary>
    internal static void VerifyBeetomRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 1) != 0, BeetomDecisions.FacesRight.IsTakenBy(sample), "distant Beetom faces right", sample);

        Console.WriteLine("Beetom random chances: all 65536 samples decide as the bit test they replace.");
    }
}
