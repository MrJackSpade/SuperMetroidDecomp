using SuperMetroid.Core.Bosses.Crocomire;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Confirms that Crocomire's named spit-attack chance (#1619) decides every one of the 65536 samples exactly as the
/// threshold comparison it replaces did.
/// </summary>
internal static class CrocomireRandomChancesVerification
{
    /// <summary>Compares every sample's named answer with the threshold expression it replaces.</summary>
    internal static void VerifyCrocomireRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt((sample & 4095) < 1024, CrocomireRandomDefinitions.ProjectileAttack.IsTakenBy(sample), "spit attack", sample);

        Console.WriteLine("Crocomire random chances: all 65536 samples decide as the threshold expression they replace.");
    }
}
