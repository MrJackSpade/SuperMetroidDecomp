namespace SuperMetroid.Core.Compatibility;

/// <summary>Shared operations for cartridge-owned wrapping and saturating word counters.</summary>
[CompatibilityService("counters that wrap at their width instead of saturating, so an expired timer keeps counting from the top")]
[EncodingOwner("wrapping and saturating word counters")]
public static class WrappingCounter
{
    /// <summary>Executes one wrapping 16-bit DEC of a word, 0 through 65535, and exposes its resulting N/Z flags.</summary>
    public static CountdownStep Decrement(int value) =>
        new(unchecked((ushort)(Ensure.BetweenInclusive(0, ushort.MaxValue, value) - 1)));

    /// <summary>
    /// Executes one 8-bit <c>DEC</c> of a byte (for example an 8-bit color or brightness component), wrapping
    /// <c>$00</c> to <c>$FF</c>. Guarded callers decrement only a nonzero byte, so they never wrap.
    /// </summary>
    public static byte DecrementByte(byte value) => unchecked((byte)(value - 1));

    /// <summary>
    /// Executes one 16-bit <c>INC</c> of a word held as an int, 0 through 65535, wrapping <c>$FFFF</c> to <c>$0000</c>.
    /// </summary>
    public static int IncrementWord(int value) => (Ensure.BetweenInclusive(0, ushort.MaxValue, value) + 1) & ushort.MaxValue;

    /// <summary>
    /// Executes one 16-bit <c>ADC</c> of <paramref name="step"/> to a word counter held as an int, 0 through 65535: a
    /// counter stepped past <c>$FFFF</c> (for example a palette cycle a wrapping decrement left at the top) restarts
    /// from the bottom instead of growing past the word.
    /// </summary>
    /// <param name="value">The counter, 0 through 65535.</param>
    /// <param name="step">The step, 0 through 65535.</param>
    public static int AddWord(int value, int step) =>
        (Ensure.BetweenInclusive(0, ushort.MaxValue, value) + Ensure.BetweenInclusive(0, ushort.MaxValue, step)) & ushort.MaxValue;

    /// <summary>
    /// Executes one 8-bit <c>DEC</c> of <paramref name="word"/>'s low byte, as the cartridge does for a
    /// byte countdown packed beside independent state. The low byte wraps from <c>$00</c> to <c>$FF</c>
    /// without borrowing into the high byte, which is kept unchanged; the step's flags are byte-width.
    /// </summary>
    public static LowByteCountdownStep DecrementLowByte(ushort word)
    {
        byte count = unchecked((byte)word);
        return new((ushort)(word - count + unchecked((byte)(count - 1))));
    }
}
