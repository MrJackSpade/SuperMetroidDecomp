namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Counters that wrap at the top or bottom of their range instead of stopping there, as every counter of the original
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
/// the beam producer's firing step, a sound queue's clear delay at the end of the update).
/// </para>
/// <para>
/// <b>Evidence.</b> The original game's counters are words and bytes stepped with no clamp; the full-movie replays (the
/// Ridley full movie and the 13% SMV) step every counter here and match the recording on every update.
/// </para>
/// </summary>
[CompatibilityService("counters that wrap at their width instead of saturating, so an expired timer keeps counting from the top")]
[EncodingOwner("wrapping and saturating word counters")]
public static class WrappingCounter
{
    /// <summary>
    /// Takes one away from a timer held as a plain count, 0 through 65535: a count at zero wraps to 65535. The step's
    /// tests are the owner's expiry rule.
    /// </summary>
    public static CountdownStep Decrement(int value) =>
        new(WrappingCountdown.FromRaw(DecrementWord(value)));

    /// <summary>Takes one away from a count of 0 through 65535; a count at zero continues from 65535.</summary>
    /// <param name="value">The count, 0 through 65535.</param>
    public static int DecrementWord(int value) => (Ensure.BetweenInclusive(0, ushort.MaxValue, value) - 1) & ushort.MaxValue;

    /// <summary>
    /// Takes one away from a count of 0 through 255 (for example a colour component or a sound queue's clear delay); a
    /// count at zero continues from 255. Owners that step only a nonzero count never wrap it.
    /// </summary>
    public static byte DecrementByte(byte value) => unchecked((byte)(value - 1));

    /// <summary>Adds one to a count of 0 through 65535; a count at 65535 continues from zero.</summary>
    public static int IncrementWord(int value) => (Ensure.BetweenInclusive(0, ushort.MaxValue, value) + 1) & ushort.MaxValue;

    /// <summary>
    /// Adds <paramref name="step"/> to a count of 0 through 65535: a count stepped past 65535 (for example a palette cycle
    /// a wrapping decrement left at the top) continues from the bottom instead of growing past its range.
    /// </summary>
    /// <param name="value">The count, 0 through 65535.</param>
    /// <param name="step">The step, 0 through 65535.</param>
    public static int AddWord(int value, int step) =>
        (Ensure.BetweenInclusive(0, ushort.MaxValue, value) + Ensure.BetweenInclusive(0, ushort.MaxValue, step)) & ushort.MaxValue;

    /// <summary>
    /// Takes one away from the eight-bit count kept in the lower half of <paramref name="value"/>, 0 through 65535: the
    /// eight-bit count wraps from 0 to 255 and the upper half is kept unchanged. The step's tests describe the eight-bit
    /// count alone.
    /// </summary>
    /// <param name="value">The value holding the count, 0 through 65535.</param>
    public static LowByteCountdownStep DecrementLowByte(int value) =>
        WrappingCountdown.FromRaw(value).DecrementEightBitCount();
}
