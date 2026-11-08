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
    public int SignedRawValue => unchecked((short)RawValue);

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

    /// <summary>Whether the velocity points toward decreasing coordinates.</summary>
    public bool IsNegative => SignedRawValue < 0;

    /// <summary>
    /// The sum of two velocities, or of a velocity and the acceleration applied to it, in 256ths of a pixel. The sum
    /// must lie within the type's range, just under 128 pixels an update either way; a caller whose speed can reach
    /// past it states that wrap with <see cref="Wrapped"/>, and any other sum past it fails instead of wrapping.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The sum lies outside the type's range.</exception>
    public static SignedEightEight operator +(SignedEightEight left, SignedEightEight right) =>
        FromSignedRaw(left.SignedRawValue + right.SignedRawValue);

    /// <summary>
    /// The difference of two velocities, or a velocity less the deceleration applied to it, in 256ths of a pixel; the
    /// difference must lie within the type's range, as for the sum.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The difference lies outside the type's range.</exception>
    public static SignedEightEight operator -(SignedEightEight left, SignedEightEight right) =>
        FromSignedRaw(left.SignedRawValue - right.SignedRawValue);

    /// <summary>The velocity reversed; the most negative value has no reversal within the type's range and fails.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is the most negative one.</exception>
    public static SignedEightEight operator -(SignedEightEight value) => FromSignedRaw(-value.SignedRawValue);

    /// <summary>The velocity scaled by a whole factor; the product must lie within the type's range.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The product lies outside the type's range.</exception>
    public static SignedEightEight operator *(SignedEightEight value, int factor) =>
        FromSignedRaw(checked(value.SignedRawValue * factor));

    /// <summary>Promotes the signed word to the 16.16 delta used by native position adders.</summary>
    public SignedSixteenSixteen ToSixteenSixteenDelta() =>
        SignedSixteenSixteen.FromRaw(unchecked((short)RawValue) << 8);
}
