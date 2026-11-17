using static SuperMetroid.Core.Enemies.Boyon.BoyonSpeedDefinitions;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>A Boyon's bounce curve: the sample its multiplier scales at each place along the curve.</summary>
public static class BoyonBounceCurve
{
    /// <summary>The curve is k(k+1)/2 at place k from the top, saturating at 255 beyond its authored samples.</summary>
    /// <remarks>
    /// Each of the first 23 places has the sample k*(k+1)/2 for place k from 0 through 22, ending at 253. From the
    /// twenty-fourth place down the sample is the saturated 255.
    /// </remarks>
    /// <param name="place">The place along the curve, never past the top.</param>
    public static int Sample(BoyonCurvePlace place) => place.EntryFromTop < StoredSampleCount
        ? place.EntryFromTop * (place.EntryFromTop + 1) / 2
        : SaturatedSample;

    /// <summary>The sample at <paramref name="place"/> scaled by <paramref name="multiplier"/>, one or more.</summary>
    /// <param name="place">The place along the curve, never past the top.</param>
    /// <param name="multiplier">The Boyon's speed multiplier.</param>
    public static int Multiply(BoyonCurvePlace place, int multiplier) => Sample(place) * (multiplier >= 1
        ? multiplier
        : throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, "A Boyon's curve is scaled by one or more."));
}
