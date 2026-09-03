namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Result of one native 65C816 word decrement. The flags describe the wrapped result, not
/// the pre-decrement value, matching the processor's DEC/BEQ/BMI/BPL branch sequence.
/// </summary>
public readonly record struct NativeWordCounterStep(ushort Value)
{
    /// <summary>Whether DEC produced <c>$0000</c> and set the processor zero flag.</summary>
    public bool IsZero => Value == 0;

    /// <summary>Whether DEC produced a word with bit 15 set and set the negative flag.</summary>
    public bool IsNegative => (Value & 0x8000) != 0;

    /// <summary>Whether the result would take a BPL branch.</summary>
    public bool IsNonNegative => !IsNegative;

    /// <summary>Common native expiry predicate implemented as BEQ followed by BMI.</summary>
    public bool IsZeroOrNegative => IsZero || IsNegative;

    /// <summary>Whether decrementing zero wrapped through <c>$FFFF</c>.</summary>
    public bool Underflowed => Value == ushort.MaxValue;
}

/// <summary>Shared operations for cartridge-owned wrapping and saturating word counters.</summary>
public static class NativeWordCounter
{
    /// <summary>Executes one wrapping 16-bit DEC and exposes its resulting N/Z flags.</summary>
    public static NativeWordCounterStep Decrement(ushort value) =>
        new(unchecked((ushort)(value - 1)));

    /// <summary>
    /// Decrements a nonzero word and leaves zero saturated, matching the common
    /// <c>LDA timer / BEQ done / DEC timer</c> pattern rather than unconditional DEC.
    /// </summary>
    public static ushort DecrementSaturating(ushort value) =>
        value == 0 ? (ushort)0 : unchecked((ushort)(value - 1));
}
