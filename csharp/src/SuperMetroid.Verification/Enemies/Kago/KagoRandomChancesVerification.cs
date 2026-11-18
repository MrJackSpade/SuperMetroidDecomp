using SuperMetroid.Core.Enemies.Kago;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Kago;

/// <summary>
/// Confirms that the chance a Kago bug near its shell jumps left (#2479) decides every one of the 65536 samples exactly
/// as the test of the sample's ninth bit it replaces did.
/// </summary>
internal static class KagoRandomChancesVerification
{
    /// <summary>Compares every sample's named answer with the bit test it replaces.</summary>
    internal static void VerifyKagoRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 256) != 0, KagoDefinitions.BugJumpsLeft.IsTakenBy(sample), "bug jumps left", sample);

        Console.WriteLine("Kago random chances: all 65536 samples decide as the bit test they replace.");
    }
}
