using SuperMetroid.Core.Areas.Ceres;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// Confirms that the chance a Ceres escape debris piece falls dark (#2479) decides every one of the 65536 samples
/// exactly as the test of the sample's top bit it replaces did.
/// </summary>
internal static class CeresFallingDebrisChanceVerification
{
    /// <summary>Compares every sample's named answer with the bit test it replaces.</summary>
    internal static void VerifyCeresFallingDebrisChance()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 32768) != 0, CeresFallingDebrisDefinitions.PieceIsDark.IsTakenBy(sample), "dark debris piece", sample);

        Console.WriteLine("Ceres falling debris chance: all 65536 samples decide as the bit test it replaces.");
    }
}
