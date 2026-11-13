using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Verification.Compatibility;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Stoke;

/// <summary>
/// Confirms that a moving Stoke's named attack chance (#1619) decides every sample, at every compared enemy count, exactly
/// as the roll and threshold it replaces did: the sample's low byte plus the count, within 256, below two.
/// </summary>
internal static class StokeAttackChanceVerification
{
    /// <summary>Compares every sample at each compared enemy count with the replaced roll and threshold.</summary>
    internal static void VerifyStokeAttackChance()
    {
        foreach (int count in AdvancedRollCounts.All)
        {
            string rule = $"attack at count {count}";
            for (int sample = 0; sample <= ushort.MaxValue; sample++)
                AssertSameAt(((sample & 255) + count) % 256 < 2, StokeAttackDefinitions.Attacks.IsTakenBy(sample, count), rule, sample);
        }

        Console.WriteLine("Stoke attack chance: every sample at every compared count decides as the roll and threshold it replaces.");
    }
}
