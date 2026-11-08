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

    /// <summary>
    /// The whole part's eight bits read as an unsigned count of pixels, 0 through 255, as routines that take only the
    /// velocity's high byte do: a negative velocity's whole part reads 256 larger.
    /// </summary>
    [IntegerQuantity("whole-pixel byte of an 8.8 velocity")]
    public int WholeByte => EightEight.WholeOf(RawValue);

    /// <summary>The fraction's eight bits, 0 through 255 256ths of a pixel.</summary>
    [IntegerQuantity("fraction byte of an 8.8 velocity")]
    public int FractionByte => EightEight.FractionOf(RawValue);

    /// <summary>Whether the velocity points toward decreasing coordinates, leftward or upward.</summary>
    public bool IsNegative => SignedRawValue < 0;

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

    /// <summary>Whether <paramref name="left"/> is the lesser signed velocity.</summary>
    public static bool operator <(SignedEightEight left, SignedEightEight right) => left.SignedRawValue < right.SignedRawValue;

    /// <summary>Whether <paramref name="left"/> is the greater signed velocity.</summary>
    public static bool operator >(SignedEightEight left, SignedEightEight right) => left.SignedRawValue > right.SignedRawValue;

    /// <summary>Whether <paramref name="left"/> is no greater than <paramref name="right"/> as a signed velocity.</summary>
    public static bool operator <=(SignedEightEight left, SignedEightEight right) => left.SignedRawValue <= right.SignedRawValue;

    /// <summary>Whether <paramref name="left"/> is no less than <paramref name="right"/> as a signed velocity.</summary>
    public static bool operator >=(SignedEightEight left, SignedEightEight right) => left.SignedRawValue >= right.SignedRawValue;

    /// <summary>
    /// The velocity times a factor in 256ths, <paramref name="factorInTwoHundredFiftySixths"/>, as a 16.16 displacement
    /// that keeps the product's full precision: the product of the two 8.8 values, nothing truncated.
    /// </summary>
    /// <exception cref="OverflowException">The product lies outside the 16.16 range.</exception>
    public SignedSixteenSixteen ScaledBy(int factorInTwoHundredFiftySixths) =>
        SignedSixteenSixteen.FromRaw(checked(SignedRawValue * factorInTwoHundredFiftySixths));

    /// <summary>Promotes the signed word to the 16.16 delta used by native position adders.</summary>
    public SignedSixteenSixteen ToSixteenSixteenDelta() =>
        SignedSixteenSixteen.FromRaw(unchecked((short)RawValue) << 8);
}
