namespace SuperMetroid.Core.Spatial;

/// <summary>A signed 8.8 fixed-point velocity: eight whole bits and eight fraction bits, kept exactly.</summary>
[EncodingOwner("signed 8.8 fixed-point number")]
public readonly record struct SignedEightEight
{
    /// <summary>The value whose two's-complement word is <paramref name="rawValue"/>, 0 through 65535.</summary>
    /// <param name="rawValue">The unchanged two's-complement word containing eight whole and eight fractional bits.</param>
    public SignedEightEight(int rawValue) => RawValue = checked((ushort)rawValue);

    /// <summary>The unchanged two's-complement word containing eight whole and eight fractional bits.</summary>
    public ushort RawValue { get; init; }

    /// <summary>The raw word read as its signed two's-complement value.</summary>
    public short SignedRawValue => unchecked((short)RawValue);

    /// <summary>Creates a value from a signed raw 8.8 word, -32768 through 32767, without changing its bits.</summary>
    public static SignedEightEight FromSignedRaw(int rawValue) =>
        new(unchecked((ushort)Ensure.BetweenInclusive(short.MinValue, short.MaxValue, rawValue)));

    /// <summary>
    /// The value a raw computation lands on: a sum or difference of 8.8 words, in 1/256 units, wraps at sixteen bits,
    /// so a velocity stepped past the largest positive value continues from the most negative one and the reverse,
    /// as the game's velocity arithmetic does.
    /// </summary>
    /// <param name="rawValue">The computed raw value, in 1/256 units, which may lie outside sixteen bits.</param>
    public static SignedEightEight Wrapped(int rawValue) => new(unchecked((ushort)rawValue));

    /// <summary>The velocity reversed: its raw word negated, wrapping at sixteen bits, so the most negative value stays.</summary>
    public SignedEightEight Negated() => Wrapped(-RawValue);

    /// <summary>Promotes the signed word to the 16.16 delta used by native position adders.</summary>
    public SignedSixteenSixteen ToSixteenSixteenDelta() =>
        SignedSixteenSixteen.FromRaw(unchecked((short)RawValue) << 8);
}
