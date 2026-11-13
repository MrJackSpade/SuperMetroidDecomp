using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Verification.Compatibility;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Owtch;

/// <summary>
/// Confirms that a walking Owtch's named burial chance (#1619) decides every sample, at every compared enemy count, exactly
/// as the roll and threshold it replaces did: the sample's low byte plus the count, within 256, below six.
/// </summary>
internal static class OwtchSinkChanceVerification
{
    /// <summary>Compares every sample at each compared enemy count with the replaced roll and threshold.</summary>
    internal static void VerifyOwtchSinkChance()
    {
        foreach (int count in AdvancedRollCounts.All)
        {
            string rule = $"burial at count {count}";
            for (int sample = 0; sample <= ushort.MaxValue; sample++)
                AssertSameAt(((sample & 255) + count) % 256 < 6, OwtchMovementDefinitions.Sinks.IsTakenBy(sample, count), rule, sample);
        }

        Console.WriteLine("Owtch burial chance: every sample at every compared count decides as the roll and threshold it replaces.");
    }
}
