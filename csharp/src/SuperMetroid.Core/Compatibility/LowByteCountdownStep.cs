namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Result of one native 8-bit DEC of the low byte of a word whose high byte is independent state.
/// The flags describe the decremented low byte only, as the processor sets them in 8-bit mode.
/// </summary>
[CompatibilityService("a countdown kept in the low half of a value whose high half is independent state, wrapping without disturbing it")]
[EncodingOwner("low-byte countdown decrement result")]
public readonly record struct LowByteCountdownStep(ushort Remaining)
{
    /// <summary>The decremented low byte.</summary>
    public byte Low => unchecked((byte)Remaining);

    /// <summary>Whether the 8-bit DEC produced <c>$00</c> (DEC then BEQ/BNE).</summary>
    public bool IsZero => Low == 0;

    /// <summary>Whether the 8-bit DEC produced a byte with bit 7 set (DEC then BMI).</summary>
    public bool IsNegative => (Low & 0x80) != 0;

    /// <summary>The 8-bit DEC, BEQ, BMI expiry predicate.</summary>
    public bool IsZeroOrNegative => IsZero || IsNegative;
}
