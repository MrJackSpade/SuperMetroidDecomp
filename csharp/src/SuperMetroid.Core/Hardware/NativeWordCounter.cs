namespace SuperMetroid.Core.Hardware;

/// <summary>
/// Result of one native 65C816 word decrement. The flags describe the wrapped result, not
/// the pre-decrement value, matching the processor's DEC/BEQ/BMI/BPL branch sequence.
/// </summary>
[SnesEncodingOwner]
public readonly record struct NativeWordCounterStep(ushort Value)
{
    /// <summary>
    /// The decremented word as countdown storage. A wrapped result (<c>$FFFF</c> from a cleared
    /// counter) is kept exactly, so a later read observes the same word the cartridge would.
    /// </summary>
    public NativeCountdown Remaining => NativeCountdown.FromRaw(Value);

    /// <summary>Whether DEC produced <c>$0000</c> and set the processor zero flag (DEC then BEQ/BNE).</summary>
    public bool IsZero => Value == 0;

    /// <summary>Whether DEC produced a word with bit 15 set and set the negative flag (DEC then BMI).</summary>
    public bool IsNegative => (Value & 0x8000) != 0;

    /// <summary>Whether the result would take a BPL branch (DEC then BPL).</summary>
    public bool IsNonNegative => !IsNegative;

    /// <summary>Common native expiry predicate implemented as DEC, BEQ, BMI.</summary>
    public bool IsZeroOrNegative => IsZero || IsNegative;
}

/// <summary>
/// Result of one native 8-bit DEC of the low byte of a word whose high byte is independent state.
/// The flags describe the decremented low byte only, as the processor sets them in 8-bit mode.
/// </summary>
[SnesEncodingOwner]
public readonly record struct NativeLowByteCounterStep(ushort Remaining)
{
    /// <summary>The decremented low byte.</summary>
    public byte Low => SnesLittleEndian.LowByte(Remaining);

    /// <summary>Whether the 8-bit DEC produced <c>$00</c> (DEC then BEQ/BNE).</summary>
    public bool IsZero => Low == 0;

    /// <summary>Whether the 8-bit DEC produced a byte with bit 7 set (DEC then BMI).</summary>
    public bool IsNegative => (Low & 0x80) != 0;

    /// <summary>Whether the result would take an 8-bit BPL branch.</summary>
    public bool IsNonNegative => !IsNegative;

    /// <summary>The 8-bit DEC, BEQ, BMI expiry predicate.</summary>
    public bool IsZeroOrNegative => IsZero || IsNegative;
}

/// <summary>
/// The stored form of a cartridge 16-bit countdown: a word that gameplay starts, decrements and tests.
/// It keeps the exact native word, including a wrapped <c>$FFFF</c> or any value with bit 15 set, so
/// a phase that reads a timer left wrapped by an earlier phase sees what the cartridge would.
/// <list type="bullet">
/// <item>Went negative: <c>DEC</c> then <c>BPL</c>. Use <see cref="Decrement"/> and
/// <see cref="NativeWordCounterStep.IsNegative"/>. A cleared timer expires on the first call; a timer
/// started at N expires on call N + 1.</item>
/// <item>Reached zero: <c>DEC</c> then <c>BNE</c>/<c>BEQ</c>. Use <see cref="Decrement"/> and
/// <see cref="NativeWordCounterStep.IsZero"/>. A cleared timer wraps to <c>$FFFF</c> and waits
/// 65,535 more calls.</item>
/// <item>Zero or negative: <c>DEC</c>, <c>BEQ</c>, <c>BMI</c>. Use
/// <see cref="NativeWordCounterStep.IsZeroOrNegative"/>.</item>
/// <item>Saturating: <c>LDA</c>, <c>BEQ</c>, <c>DEC</c>. Use <see cref="DecrementToZero"/>; it never wraps.</item>
/// </list>
/// </summary>
[SnesEncodingOwner]
public readonly record struct NativeCountdown
{
    /// <summary>Stores <paramref name="rawValue"/> exactly; callers use <see cref="Start"/> or <see cref="FromRaw"/>.</summary>
    private NativeCountdown(ushort rawValue) => RawValue = rawValue;

    /// <summary>The exact native word, for sites that also read the timer as a number or index.</summary>
    public ushort RawValue { get; }

    /// <summary>Whether the stored word is <c>$0000</c> (LDA then BEQ/BNE).</summary>
    public bool IsZero => RawValue == 0;

    /// <summary>Whether the stored word has bit 15 set (LDA then BMI/BPL).</summary>
    public bool IsNegative => (RawValue & 0x8000) != 0;

    /// <summary>
    /// Starts a countdown at <paramref name="frames"/>, which must fit the native word
    /// (<c>0</c> through <c>$FFFF</c>).
    /// </summary>
    public static NativeCountdown Start(int frames) =>
        new((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, frames));

    /// <summary>Preserves a native countdown word exactly, for codecs, slot-word views and native projections.</summary>
    public static NativeCountdown FromRaw(ushort rawValue) => new(rawValue);

    /// <summary>
    /// Executes one wrapping 16-bit <c>DEC</c>. Store <see cref="NativeWordCounterStep.Remaining"/> and keep
    /// the site's native branch: <see cref="NativeWordCounterStep.IsNegative"/> for <c>BPL</c>,
    /// <see cref="NativeWordCounterStep.IsZero"/> for <c>BNE</c>/<c>BEQ</c>, or
    /// <see cref="NativeWordCounterStep.IsZeroOrNegative"/> for <c>BEQ</c> then <c>BMI</c>.
    /// </summary>
    public NativeWordCounterStep Decrement() => NativeWordCounter.Decrement(RawValue);

    /// <summary>
    /// The saturating form <c>LDA</c>, <c>BEQ</c>, <c>DEC</c>: a zero word stays zero, any other word is
    /// decremented by one. It never wraps.
    /// </summary>
    public NativeCountdown DecrementToZero() => RawValue == 0 ? this : new((ushort)(RawValue - 1));

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X4}";
}

/// <summary>Shared operations for cartridge-owned wrapping and saturating word counters.</summary>
[SnesEncodingOwner]
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
