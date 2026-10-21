namespace SuperMetroid.Core.Compatibility;

/// <summary>Shared operations for cartridge-owned wrapping and saturating word counters.</summary>
[CompatibilityService("counters that wrap at their width instead of saturating, so an expired timer keeps counting from the top")]
[SnesEncodingOwner]
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
    /// Executes one 8-bit <c>DEC</c> of a byte held as an int, 0 through 255, wrapping <c>$00</c> to <c>$FF</c>.
    /// </summary>
    public static int DecrementByte(int value) => (Ensure.BetweenInclusive(0, byte.MaxValue, value) - 1) & byte.MaxValue;

    /// <summary>
    /// Executes one 16-bit <c>INC</c> of a word held as an int, 0 through 65535, wrapping <c>$FFFF</c> to <c>$0000</c>.
    /// </summary>
    public static int IncrementWord(int value) => (Ensure.BetweenInclusive(0, ushort.MaxValue, value) + 1) & ushort.MaxValue;

    /// <summary>
    /// Executes one 8-bit <c>DEC</c> of <paramref name="word"/>'s low byte, as the cartridge does for a
    /// byte countdown packed beside independent state. The low byte wraps from <c>$00</c> to <c>$FF</c>
    /// without borrowing into the high byte, which is kept unchanged; the step's flags are byte-width.
    /// </summary>
    public static LowByteCountdownStep DecrementLowByte(ushort word) =>
        new(SnesLittleEndian.Word(unchecked((byte)(SnesLittleEndian.LowByte(word) - 1)), SnesLittleEndian.HighByte(word)));
}
