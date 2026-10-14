namespace SuperMetroid.Core.Hardware;

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
