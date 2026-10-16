namespace SuperMetroid.Core.Hardware;

/// <summary>Shared operations for cartridge-owned wrapping and saturating word counters.</summary>
public static class NativeWordCounter
{
    /// <summary>Executes one wrapping 16-bit DEC and exposes its resulting N/Z flags.</summary>
    public static NativeWordCounterStep Decrement(ushort value) =>
        new(unchecked((ushort)(value - 1)));

    /// <summary>
    /// Executes one 8-bit <c>DEC</c> of a byte (for example an 8-bit color or brightness component), wrapping
    /// <c>$00</c> to <c>$FF</c>. Guarded callers decrement only a nonzero byte, so they never wrap.
    /// </summary>
    public static byte DecrementByte(byte value) => unchecked((byte)(value - 1));

    /// <summary>
    /// Executes one 8-bit <c>DEC</c> of <paramref name="word"/>'s low byte, as the cartridge does for a
    /// byte countdown packed beside independent state. The low byte wraps from <c>$00</c> to <c>$FF</c>
    /// without borrowing into the high byte, which is kept unchanged; the step's flags are byte-width.
    /// </summary>
    public static NativeLowByteCounterStep DecrementLowByte(ushort word) =>
        new(SnesLittleEndian.Word(unchecked((byte)(SnesLittleEndian.LowByte(word) - 1)), SnesLittleEndian.HighByte(word)));
}
