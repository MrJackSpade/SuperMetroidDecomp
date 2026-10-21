namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The stored form of a cartridge 16-bit countdown: a word that gameplay starts, decrements and tests.
/// It keeps the exact native word, including a wrapped <c>$FFFF</c> or any value with bit 15 set, so
/// a phase that reads a timer left wrapped by an earlier phase sees what the cartridge would.
/// <list type="bullet">
/// <item>Went negative: <c>DEC</c> then <c>BPL</c>. Use <see cref="Decrement"/> and
/// <see cref="CountdownStep.IsNegative"/>. A cleared timer expires on the first call; a timer
/// started at N expires on call N + 1.</item>
/// <item>Reached zero: <c>DEC</c> then <c>BNE</c>/<c>BEQ</c>. Use <see cref="Decrement"/> and
/// <see cref="CountdownStep.IsZero"/>. A cleared timer wraps to <c>$FFFF</c> and waits
/// 65,535 more calls.</item>
/// <item>Zero or negative: <c>DEC</c>, <c>BEQ</c>, <c>BMI</c>. Use
/// <see cref="CountdownStep.IsZeroOrNegative"/>.</item>
/// <item>Saturating: <c>LDA</c>, <c>BEQ</c>, <c>DEC</c>. Use <see cref="DecrementToZero"/>; it never wraps.</item>
/// </list>
/// </summary>
[CompatibilityService("a countdown that keeps the value it wrapped to, so a timer decremented past zero reads as a large or negative count")]
[SnesEncodingOwner]
public readonly record struct WrappingCountdown
{
    /// <summary>Stores <paramref name="rawValue"/> exactly; callers use <see cref="Start"/> or <see cref="FromRaw"/>.</summary>
    private WrappingCountdown(ushort rawValue) => RawValue = rawValue;

    /// <summary>The exact native word, for sites that also read the timer as a number or index.</summary>
    [IntegerQuantity("the countdown's remaining count")]
    public ushort RawValue { get; }

    /// <summary>Whether the stored word is <c>$0000</c> (LDA then BEQ/BNE).</summary>
    public bool IsZero => RawValue == 0;

    /// <summary>Whether the stored word has bit 15 set (LDA then BMI/BPL).</summary>
    public bool IsNegative => (RawValue & 0x8000) != 0;

    /// <summary>
    /// Starts a countdown at <paramref name="frames"/>, which must fit the native word
    /// (<c>0</c> through <c>$FFFF</c>).
    /// </summary>
    public static WrappingCountdown Start(int frames) =>
        new((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, frames));

    /// <summary>Preserves a native countdown word exactly, for codecs, slot-word views and native projections.</summary>
    public static WrappingCountdown FromRaw(int rawValue) => new((ushort)Ensure.BetweenInclusive(0, ushort.MaxValue, rawValue));

    /// <summary>
    /// Executes one wrapping 16-bit <c>DEC</c>. Store <see cref="CountdownStep.Remaining"/> and keep
    /// the site's native branch: <see cref="CountdownStep.IsNegative"/> for <c>BPL</c>,
    /// <see cref="CountdownStep.IsZero"/> for <c>BNE</c>/<c>BEQ</c>, or
    /// <see cref="CountdownStep.IsZeroOrNegative"/> for <c>BEQ</c> then <c>BMI</c>.
    /// </summary>
    public CountdownStep Decrement() => WrappingCounter.Decrement(RawValue);

    /// <summary>
    /// The saturating form <c>LDA</c>, <c>BEQ</c>, <c>DEC</c>: a zero word stays zero, any other word is
    /// decremented by one. It never wraps.
    /// </summary>
    public WrappingCountdown DecrementToZero() => RawValue == 0 ? this : new((ushort)(RawValue - 1));

    /// <inheritdoc />
    public override string ToString() => $"${RawValue:X4}";
}
