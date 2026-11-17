using static SuperMetroid.Core.Enemies.Boyon.BoyonSpeedDefinitions;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>A Boyon's bounce curve: the sample its multiplier scales at each place along the curve.</summary>
public static class BoyonBounceCurve
{
    /// <summary>
    /// The sample at <paramref name="place"/>: its rising sample (<see cref="BoyonSpeedDefinitions.RisingSamples"/>), or
    /// the saturated sample past them.
    /// </summary>
    /// <param name="place">The place along the curve, never past the top.</param>
    public static int Sample(BoyonCurvePlace place) => place.EntryFromTop < RisingSamples.Length
        ? RisingSamples[place.EntryFromTop]
        : SaturatedSample;

    /// <summary>The sample at <paramref name="place"/> scaled by <paramref name="multiplier"/>, one or more.</summary>
    /// <param name="place">The place along the curve, never past the top.</param>
    /// <param name="multiplier">The Boyon's speed multiplier.</param>
    public static int Multiply(BoyonCurvePlace place, int multiplier) => Sample(place) * (multiplier >= 1
        ? multiplier
        : throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, "A Boyon's curve is scaled by one or more."));
}
