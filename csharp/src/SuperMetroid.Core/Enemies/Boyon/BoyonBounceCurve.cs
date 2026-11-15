using static SuperMetroid.Core.Enemies.Boyon.BoyonSpeedDefinitions;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>A Boyon's bounce curve: the sample its multiplier scales at each place along the curve.</summary>
public static class BoyonBounceCurve
{
    /// <summary>The curve is k(k+1)/2 at place k from the top, saturating at 255 beyond its authored samples.</summary>
    /// <remarks>
    /// Each of the 23 authored samples equals k*(k+1)/2 for place k from 0 through 22, ending at 253. From the
    /// twenty-fourth place down the bounce uses 255 before it multiplies, so no sample past the authored 23 ever reaches
    /// it; this method gives the sample the bounce uses. The compiled Boyon speed check checks all authored samples, the
    /// first 65,536 places, and every multiplier of 0 through 255 through the real production calculation.
    /// </remarks>
    /// <param name="place">The place along the curve, never past the top.</param>
    public static int Sample(BoyonCurvePlace place) => place.EntryFromTop < StoredSampleCount
        ? place.EntryFromTop * (place.EntryFromTop + 1) / 2
        : SaturatedSample;

    /// <summary>
    /// The sample at <paramref name="place"/> scaled by <paramref name="multiplier"/>, 0 through
    /// <see cref="BoyonSpeedDefinitions.LargestMultiplier"/>; the product is 0 through <see cref="BoyonSpeedDefinitions.LargestScaledSample"/>.
    /// </summary>
    /// <param name="place">The place along the curve, never past the top.</param>
    /// <param name="multiplier">The Boyon's speed multiplier.</param>
    public static int Multiply(BoyonCurvePlace place, int multiplier) => Sample(place) * Ensure.BetweenInclusive(0, LargestMultiplier, multiplier);
}
