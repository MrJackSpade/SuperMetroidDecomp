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

    /// <summary>No velocity at all.</summary>
    public static SignedEightEight Zero => default;

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

    /// <summary>Whether the velocity points toward decreasing coordinates, leftward or upward.</summary>
    public bool IsNegative => SignedRawValue < 0;

    /// <summary>The velocity's whole pixels, its fraction dropped toward negative infinity.</summary>
    [IntegerQuantity("whole-pixel part of a signed 8.8 velocity")]
    public int WholePixels => SignedRawValue >> 8;

    /// <summary>The velocity halved, its lowest fraction bit dropped toward negative infinity.</summary>
    public SignedEightEight Halved() => FromSignedRaw(SignedRawValue >> 1);

    /// <summary>Whether <paramref name="left"/> is the slower velocity, comparing signed values.</summary>
    public static bool operator <(SignedEightEight left, SignedEightEight right) => left.SignedRawValue < right.SignedRawValue;

    /// <summary>Whether <paramref name="left"/> is the faster velocity, comparing signed values.</summary>
    public static bool operator >(SignedEightEight left, SignedEightEight right) => left.SignedRawValue > right.SignedRawValue;

    /// <summary>Whether <paramref name="left"/> is no faster than <paramref name="right"/>, comparing signed values.</summary>
    public static bool operator <=(SignedEightEight left, SignedEightEight right) => left.SignedRawValue <= right.SignedRawValue;

    /// <summary>Whether <paramref name="left"/> is no slower than <paramref name="right"/>, comparing signed values.</summary>
    public static bool operator >=(SignedEightEight left, SignedEightEight right) => left.SignedRawValue >= right.SignedRawValue;

    /// <summary>
    /// A velocity changed by an acceleration, as the game's velocity arithmetic adds them: a sum past the largest
    /// positive value continues from the most negative one and the reverse (<see cref="Wrapped"/>).
    /// </summary>
    public static SignedEightEight operator +(SignedEightEight velocity, SignedEightEight acceleration) =>
        Wrapped(velocity.RawValue + acceleration.RawValue);

    /// <summary>
    /// A velocity changed by a deceleration, as the game's velocity arithmetic subtracts them: a difference past the most
    /// negative value continues from the largest positive one and the reverse (<see cref="Wrapped"/>).
    /// </summary>
    public static SignedEightEight operator -(SignedEightEight velocity, SignedEightEight deceleration) =>
        Wrapped(velocity.RawValue - deceleration.RawValue);

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
