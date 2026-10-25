namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Result of one native 65C816 word decrement. The flags describe the wrapped result, not
/// the pre-decrement value, matching the processor's DEC/BEQ/BMI/BPL branch sequence.
/// </summary>
[CompatibilityService("the outcome of one countdown step, tested the way each timer's expiry was originally tested (reached zero, went negative, or either)")]
[EncodingOwner("wrapping countdown decrement result")]
public readonly record struct CountdownStep(ushort Value)
{
    /// <summary>
    /// The decremented word as countdown storage. A wrapped result (<c>$FFFF</c> from a cleared
    /// counter) is kept exactly, so a later read observes the same word the cartridge would.
    /// </summary>
    public WrappingCountdown Remaining => WrappingCountdown.FromRaw(Value);

    /// <summary>Whether DEC produced <c>$0000</c> and set the processor zero flag (DEC then BEQ/BNE).</summary>
    public bool IsZero => Value == 0;

    /// <summary>Whether DEC produced a word with bit 15 set and set the negative flag (DEC then BMI).</summary>
    public bool IsNegative => (Value & 0x8000) != 0;

    /// <summary>Whether the result would take a BPL branch (DEC then BPL).</summary>
    public bool IsNonNegative => !IsNegative;

    /// <summary>Common native expiry predicate implemented as DEC, BEQ, BMI.</summary>
    public bool IsZeroOrNegative => IsZero || IsNegative;
}
