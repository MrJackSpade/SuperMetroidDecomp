namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How Samus's speeds behave past the ends of their range (moved from Samus's movement, #1621). A speed is a magnitude,
/// its direction held apart, whose whole pixels and fraction each count 0 through 65535 steps. Slowing a speed past zero
/// continues it from the top of that range rather than stopping it, and the game compares a speed part with a threshold
/// by the sign of their difference within the range. A speed in the upper half of the range therefore reads as having
/// passed zero and as below every threshold: a rise that reads as past its apex, a moonfall that lands without a bounce,
/// a fraction more than half a pixel above a cap's that reads as under it.
/// </summary>
/// <remarks>
/// A whole speed is compared with its limit part by part: the whole pixels first, and the fraction only to settle a tie
/// in whole pixels (<see cref="ReadsPastLimit"/>), or each part on its own against a cap with no tie-break at all
/// (<see cref="ReadsAtCap"/>). Consulted wherever Samus's movement tests a speed against a threshold during her update:
/// the horizontal base speed against its maximum after each acceleration, the dash's extra speed against its cap before
/// each acceleration, the vertical speed's apex and landing tests, and the enemies that read her speed.
/// Evidence: the original speed tests subtract the threshold from each sixteen-bit part and branch on the result's sign
/// (the sign of a difference that wraps past half the range flips), never comparing the two parts as one number; the
/// Samus horizontal-speed check caps a running speed at its maximum and the dash's extra speed at two and seven pixels,
/// and the speed-limit check compares both tests with those part-wise sign tests at every boundary of the range.
/// </remarks>
[CompatibilityService("Samus's speed parts compare with a threshold by the sign of their wrapped difference, so a part more than half its range above a threshold reads as below it, and a speed slowed past zero reads as past it")]
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

    /// <summary>
    /// Whether <paramref name="speed"/> reads as past <paramref name="limit"/>, so the limit replaces it: its whole pixels
    /// read as above the limit's, or equal them while its fraction reads as above the limit's. A speed exactly at its
    /// limit is not past it, and a fraction more than half a pixel above the limit's reads as under it.
    /// </summary>
    internal static bool ReadsPastLimit(UnsignedSixteenSixteen speed, UnsignedSixteenSixteen limit) =>
        Exceeds(speed.Whole, limit.Whole) || (speed.Whole == limit.Whole && Exceeds(speed.Fraction, limit.Fraction));

    /// <summary>
    /// Whether <paramref name="speed"/> reads as at or past <paramref name="cap"/>, so the cap holds it: its whole pixels
    /// and its fraction each read as reaching the cap's on their own. A fraction more than half a pixel above the cap's
    /// therefore fails the test however far the whole pixels lie above the cap's.
    /// </summary>
    internal static bool ReadsAtCap(UnsignedSixteenSixteen speed, UnsignedSixteenSixteen cap) =>
        IsAtLeast(speed.Whole, cap.Whole) && IsAtLeast(speed.Fraction, cap.Fraction);

    /// <summary>Whether a difference of two parts keeps a non-negative sign once taken within the range.</summary>
    private static bool ReadsNonNegative(int difference) => difference is (>= 0 and < HalfRange) or < -HalfRange;
}
