using SuperMetroid.Core.Enemies.Projectiles;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>
/// Confirms that the random scatter's chances of turning its offsets (#2479) decide every one of the 65536 samples
/// exactly as the tests of the sample's two top bits they replace did.
/// </summary>
internal static class EnemyProjectileScatterChancesVerification
{
    /// <summary>Compares every sample's named answers with the bit tests they replace.</summary>
    internal static void VerifyEnemyProjectileScatterChances()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
        {
            AssertSameAt((sample & 32768) != 0, EnemyProjectileScatter.XOffsetTurnsLeft.IsTakenBy(sample), "X offset turns left", sample);
            AssertSameAt((sample & 16384) != 0, EnemyProjectileScatter.YOffsetTurnsUp.IsTakenBy(sample), "Y offset turns up", sample);
        }

        Console.WriteLine("Enemy projectile scatter chances: all 65536 samples decide as the bit tests they replace.");
    }
}
