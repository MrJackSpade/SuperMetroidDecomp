using static SuperMetroid.Core.Enemies.Boyon.BoyonSpeedDefinitions;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>A Boyon's bounce curve: the sample its multiplier scales at each curve index.</summary>
public static class BoyonBounceCurve
{
    /// <summary>The curve is k(k+1)/2, with explicit saturation at 255 beyond its stored samples.</summary>
    /// <remarks>
    /// #625 / #643 exact proof: each of the 23 authored samples
    /// equals k*(k+1)/2 for authored index k in 0..22, ending at 253.
    /// The caller reads first, then substitutes 255 for every index at or above
    /// 23 before its multiply; the nominal overread at index 23 is
    /// discarded. This method models that effective caller result, not a larger table.
    /// The compiled Boyon speed check checks all authored samples, all 65,536 input indices, and
    /// every multiplier of 0 through 255 through the real production calculation.
    /// </remarks>
    public static int Sample(int index) => index < StoredSampleCount
        ? index * (index + 1) / 2
        : SaturatedSample;

    /// <summary>
    /// The sample at <paramref name="index"/> scaled by <paramref name="multiplier"/>, 0 through
    /// <see cref="BoyonSpeedDefinitions.LargestMultiplier"/>; the product is 0 through <see cref="BoyonSpeedDefinitions.LargestScaledSample"/>.
    /// </summary>
    public static int Multiply(int index, int multiplier) => Sample(index) * Ensure.BetweenInclusive(0, LargestMultiplier, multiplier);
}
