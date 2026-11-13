using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Verification.Compatibility;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;

namespace SuperMetroid.Verification.Enemies.Puyo;

/// <summary>
/// Confirms that a Puyo's named hop choices and dropped-bounce chance (#1619) choose, for every sample at every compared
/// enemy count, exactly as the roll, mask and clamp they replace did: the sample's low three bits plus the count, within
/// eight; after a small hop its low bit picks the small or big hop, after a larger hop the roll picks small, big, or high
/// from two on, and a dropped bounce is big when the roll is odd.
/// </summary>
internal static class PuyoHopChoicesVerification
{
    /// <summary>The ordinary hops in the order the replaced roll of zero, one or two indexed them.</summary>
    private static readonly PuyoHopType[] HopsByRoll = [PuyoHopType.NormalSmall, PuyoHopType.NormalBig, PuyoHopType.NormalHigh];

    /// <summary>Compares every sample at each compared enemy count with the replaced roll, mask and clamp.</summary>
    internal static void VerifyPuyoHopChoices()
    {
        foreach (int count in AdvancedRollCounts.All)
        {
            string afterSmall = $"hop after the small hop at count {count}";
            string afterLarger = $"hop after a larger hop at count {count}";
            string bounce = $"big dropped bounce at count {count}";
            for (int sample = 0; sample <= ushort.MaxValue; sample++)
            {
                int roll = ((sample & 7) + count) % 8;
                AssertSameAt(HopsByRoll[roll & 1], PuyoHopDefinitions.HopAfterSmallHop.Choose(sample, count), afterSmall, sample);
                AssertSameAt(HopsByRoll[Math.Min(roll, 2)], PuyoHopDefinitions.HopAfterLargerHop.Choose(sample, count), afterLarger, sample);
                AssertSameAt((roll & 1) != 0, PuyoHopDefinitions.BigDroppedBounce.IsTakenBy(sample, count), bounce, sample);
            }
        }

        Console.WriteLine("Puyo hop choices: every sample at every compared count chooses as the roll, mask and clamp they replace.");
    }
}
