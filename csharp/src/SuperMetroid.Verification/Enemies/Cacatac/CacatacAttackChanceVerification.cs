using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Verification.Compatibility;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Cacatac;

/// <summary>
/// Confirms that a patrolling Cacatac's named attack chance (#1619) decides every sample, at every compared enemy count,
/// exactly as the roll and threshold it replaces did: the sample's low byte plus the count, within 256, below three.
/// </summary>
internal static class CacatacAttackChanceVerification
{
    /// <summary>Compares every sample at each compared enemy count with the replaced roll and threshold.</summary>
    internal static void VerifyCacatacAttackChance()
    {
        foreach (int count in AdvancedRollCounts.All)
        {
            string rule = $"attack at count {count}";
            for (int sample = 0; sample <= ushort.MaxValue; sample++)
                AssertSameAt(((sample & 255) + count) % 256 < 3, CacatacAttackDefinitions.Attacks.IsTakenBy(sample, count), rule, sample);
        }

        Console.WriteLine("Cacatac attack chance: every sample at every compared count decides as the roll and threshold it replaces.");
    }
}
