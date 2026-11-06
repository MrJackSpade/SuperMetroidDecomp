namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// How Samus's speeds behave past the ends of their range. A speed is a magnitude, its direction held apart, whose whole
/// pixels and fraction each count 0 through 65535 steps. Slowing a speed past zero continues it from the top of that
/// range rather than stopping it, and the game compares a speed part with a threshold by the sign of their difference
/// within the range. A speed in the upper half of the range therefore reads as having passed zero and as below every
/// threshold: a rise that reads as past its apex, a moonfall that lands without a bounce, a fraction more than half a
/// pixel above a cap's that reads as under it.
/// </summary>
internal static class SamusSpeedMagnitude
{
    /// <summary>The steps one speed part counts through before it continues from zero.</summary>
    private const int PartRange = 65536;

    /// <summary>The half of the range a difference must stay within to keep its sign.</summary>
    private const int HalfRange = PartRange / 2;

    /// <summary>
    /// Whether <paramref name="part"/>, a whole speed or fraction, has been slowed past zero: it lies in the upper half of
    /// its range, where only a speed that wrapped below zero ever arrives.
    /// </summary>
    /// <param name="part">The speed part, 0 through 65535; other values are rejected.</param>
    internal static bool HasPassedZero(int part) => Ensure.BetweenInclusive(0, PartRange - 1, part) >= HalfRange;

    /// <summary>
    /// Whether <paramref name="part"/> reads as reaching <paramref name="threshold"/>: their difference is non-negative
    /// once taken within the range, so a part more than half the range above the threshold reads as below it.
    /// </summary>
    /// <param name="part">The speed part, 0 through 65535; other values are rejected.</param>
    /// <param name="threshold">The threshold, 0 through 65535; other values are rejected.</param>
    internal static bool IsAtLeast(int part, int threshold) =>
        ReadsNonNegative(Ensure.BetweenInclusive(0, PartRange - 1, part) - Ensure.BetweenInclusive(0, PartRange - 1, threshold));

    /// <summary>Whether <paramref name="part"/> reads as above <paramref name="threshold"/>: it reads as reaching it and differs from it.</summary>
    /// <param name="part">The speed part, 0 through 65535; other values are rejected.</param>
    /// <param name="threshold">The threshold, 0 through 65535; other values are rejected.</param>
    internal static bool Exceeds(int part, int threshold) => part != threshold && IsAtLeast(part, threshold);

    /// <summary>Whether a difference of two parts keeps a non-negative sign once taken within the range.</summary>
    private static bool ReadsNonNegative(int difference) => difference is (>= 0 and < HalfRange) or < -HalfRange;
}
