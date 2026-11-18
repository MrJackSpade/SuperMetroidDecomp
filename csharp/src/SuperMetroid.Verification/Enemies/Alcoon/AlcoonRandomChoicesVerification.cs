using SuperMetroid.Core.Enemies.Alcoon;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Alcoon;

/// <summary>
/// Confirms that an Alcoon's choice of how many walk cycles to take before its next volley (#2510) decides every one of the
/// 65536 samples exactly as the expression it replaces did: the sample's lowest two bits, a zero taken as two.
/// </summary>
internal static class AlcoonRandomChoicesVerification
{
    /// <summary>Compares every sample's named answer with the expression it replaces.</summary>
    internal static void VerifyAlcoonRandomChoices()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 3) == 0 ? 2 : sample & 3, AlcoonWalkDefinitions.WalkCycles.Choose(sample), "Alcoon walk cycles", sample);

        Console.WriteLine("Alcoon random choices: all 65536 samples decide as the expression they replace.");
    }
}
