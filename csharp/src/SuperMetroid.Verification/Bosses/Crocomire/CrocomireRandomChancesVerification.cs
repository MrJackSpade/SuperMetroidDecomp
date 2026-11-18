using SuperMetroid.Core.Bosses.Crocomire;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Confirms that Crocomire's named random chances decide every one of the 65536 samples exactly as the expressions they
/// replace did: his spit attack's threshold comparison (#1619) and his dying acid smoke's side bit (#2479).
/// </summary>
internal static class CrocomireRandomChancesVerification
{
    /// <summary>Compares every sample's named answers with the expressions they replace.</summary>
    internal static void VerifyCrocomireRandomChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
        {
            AssertSameAt((sample & 4095) < 1024, CrocomireRandomDefinitions.ProjectileAttack.IsTakenBy(sample), "spit attack", sample);
            AssertSameAt((sample & 2) != 0, CrocomireRandomDefinitions.AcidSmokeRisesOnRight.IsTakenBy(sample), "acid smoke on the right", sample);
        }

        Console.WriteLine("Crocomire random chances: all 65536 samples decide as the expressions they replace.");
    }
}
