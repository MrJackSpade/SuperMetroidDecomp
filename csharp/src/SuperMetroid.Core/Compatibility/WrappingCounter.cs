namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Counters that wrap at the top or bottom of their range instead of stopping there, as every counter of the
/// game does: a count of 0 through 65535 stepped below zero continues from 65535, and one stepped past 65535 continues
/// from zero; a count of 0 through 255 wraps the same way at 255.
/// <para>
/// <b>Gameplay behaviour.</b> A counter its owner steps once more than intended keeps going rather than stopping: a
/// projectile count stepped below zero by a cancelled shot reads as a full table, a palette cycle stepped past its top
/// restarts from the bottom, a step of a timer held as a plain count runs past zero into a large count
/// (<see cref="Decrement"/> reports it as a <see cref="CountdownStep"/>, tested by the owner's expiry rule).
/// </para>
/// <para>
/// <b>Frame point.</b> None of its own: each owner steps its counter once per call of the owner's update (an enemy's AI,
/// the beam producer's firing step).
/// </para>
/// <para>
/// <b>Evidence:</b> the original game's counters are words and bytes stepped with no clamp; the full-movie replays (the
/// Ridley full movie and the 13% SMV) step every counter here and match the recording on every update.
/// </para>
/// </summary>
[CompatibilityService("counters that wrap at their width instead of saturating, so an expired timer keeps counting from the top")]
[EncodingOwner("wrapping and saturating word counters")]
public static class WrappingCounter
{
    /// <summary>The largest count a wrapping counter holds: a count stepped past it continues from zero.</summary>
    public const int LargestCount = ushort.MaxValue;

    /// <summary>
    /// Takes one away from a timer held as a plain count, 0 through 65535: a count at zero wraps to 65535. The step's
    /// tests are the owner's expiry rule.
    /// </summary>
    public static CountdownStep Decrement(int value) =>
        new(WrappingCountdown.FromRaw(DecrementCount(value)));

    /// <summary>Takes one away from a count of 0 through 65535; a count at zero continues from 65535.</summary>
    /// <param name="value">The count, 0 through 65535.</param>
    public static int DecrementCount(int value) => (Ensure.BetweenInclusive(0, ushort.MaxValue, value) - 1) & ushort.MaxValue;

    /// <summary>Adds one to a count of 0 through 65535; a count at 65535 continues from zero.</summary>
    public static int IncrementCount(int value) => (Ensure.BetweenInclusive(0, ushort.MaxValue, value) + 1) & ushort.MaxValue;

    /// <summary>
    /// Adds <paramref name="step"/> to a count of 0 through 65535: a count stepped past 65535 (for example a palette cycle
    /// a wrapping decrement left at the top) continues from the bottom instead of growing past its range.
    /// </summary>
    /// <param name="value">The count, 0 through 65535.</param>
    /// <param name="step">The step, 0 through 65535.</param>
    public static int AddCount(int value, int step) =>
        (Ensure.BetweenInclusive(0, ushort.MaxValue, value) + Ensure.BetweenInclusive(0, ushort.MaxValue, step)) & ushort.MaxValue;

    /// <summary>
    /// Takes <paramref name="step"/> from a count of 0 through 65535: a count stepped below zero continues from the top
    /// (for example a speed decelerated past zero, which then reads as below zero, <see cref="ReadsBelowZero"/>).
    /// </summary>
    /// <param name="value">The count, 0 through 65535.</param>
    /// <param name="step">The step, 0 through 65535.</param>
    public static int SubtractCount(int value, int step) =>
        AddCount(value, (ushort.MaxValue + 1 - Ensure.BetweenInclusive(0, ushort.MaxValue, step)) & ushort.MaxValue);

    /// <summary>
    /// Whether a count of 0 through 65535 reads as having run below zero, as the game tests a count by its sign: a
    /// count stepped below zero continues from the top, so every count of 32768 or more reads as below zero.
    /// </summary>
    /// <param name="value">The count, 0 through 65535.</param>
    public static bool ReadsBelowZero(int value) => Ensure.BetweenInclusive(0, ushort.MaxValue, value) > short.MaxValue;

    /// <summary>
    /// Whether <paramref name="value"/> is behind <paramref name="limit"/> as the game compares two counts: by the sign
    /// of their difference, so a value up to 32768 below the limit is behind it, and one that has run 32768 or more past
    /// the limit (or a count that wrapped from zero to a large one) reads as behind it again.
    /// </summary>
    /// <param name="value">The count compared, 0 through 65535.</param>
    /// <param name="limit">The count it is compared with, 0 through 65535.</param>
    public static bool IsBehind(int value, int limit) =>
        unchecked((short)(Ensure.BetweenInclusive(0, ushort.MaxValue, value) - Ensure.BetweenInclusive(0, ushort.MaxValue, limit))) < 0;
}
