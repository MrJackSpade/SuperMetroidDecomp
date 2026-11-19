using SuperMetroid.Core.Enemies.Projectiles;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>
/// Confirms that a projectile animation's one-in-four change of animation (#2510) decides every one of the 65536 samples
/// exactly as the comparison of the sample's top two bits with three it replaces did.
/// </summary>
internal static class EnemyProjectileRandomBranchChanceVerification
{
    /// <summary>Compares every sample's named answer with the comparison it replaces.</summary>
    internal static void VerifyEnemyProjectileRandomBranchChance()
    {
        for (int sample = 0; sample <= ushort.MaxValue; sample++)
            AssertSameAt(((sample >> 14) & 3) == 3, EnemyProjectileRandomBranchDefinitions.OneInFourBranch.IsTakenBy(sample), "one-in-four branch", sample);

        Console.WriteLine("Enemy projectile random branch chance: all 65536 samples decide as the comparison it replaces.");
    }
}
