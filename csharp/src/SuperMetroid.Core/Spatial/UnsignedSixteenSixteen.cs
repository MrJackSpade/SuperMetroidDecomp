namespace SuperMetroid.Core.Spatial;

/// <summary>
/// An unsigned 16.16 fixed-point speed kept as its two halves: a whole part and a fraction, each 0 through 65535.
/// Samus's speeds are magnitudes in this form; their direction is held separately.
/// </summary>
[EncodingOwner("unsigned 16.16 fixed-point number")]
public readonly record struct UnsignedSixteenSixteen
{
    /// <summary>Holds the two words, each checked to lie within a native word.</summary>
    public UnsignedSixteenSixteen(int whole, int fraction)
    {
        Whole = Ensure.BetweenInclusive(0, ushort.MaxValue, whole);
        Fraction = Ensure.BetweenInclusive(0, ushort.MaxValue, fraction);
    }

    /// <summary>The whole word, 0 through 65535.</summary>
    [IntegerQuantity("whole-pixel part of an unsigned distance")]
    public int Whole { get; }

    /// <summary>The fraction word, 0 through 65535.</summary>
    public int Fraction { get; }

    /// <summary>Both words as one unsigned 32-bit value, the whole word high.</summary>
    public uint RawValue => ((uint)Whole << 16) | (uint)Fraction;

    /// <summary>Splits an unsigned 32-bit value into its whole and fraction words.</summary>
    public static UnsignedSixteenSixteen FromRaw(uint rawValue) =>
        new(unchecked((ushort)(rawValue >> 16)), unchecked((ushort)rawValue));

    /// <summary>A speed of <paramref name="sixteenths"/> sixteenths of a pixel, which must not be negative.</summary>
    public static UnsignedSixteenSixteen FromSixteenthsOfPixel(int sixteenths) => FromRaw(checked((uint)sixteenths * 4096u));

    /// <summary>
    /// Adds two values: a fraction sum that passes a whole moves the whole part by one, and the whole part wraps at its
    /// word.
    /// </summary>
    public static UnsignedSixteenSixteen operator +(UnsignedSixteenSixteen left, UnsignedSixteenSixteen right) =>
        FromRaw(unchecked(left.RawValue + right.RawValue));

    /// <summary>
    /// Subtracts two values: a fraction that drops below zero borrows one from the whole part, and the whole part wraps
    /// at its word, so a speed slowed past zero holds a whole word of 32768 or more, which the game's sign tests read as
    /// having passed zero.
    /// </summary>
    public static UnsignedSixteenSixteen operator -(UnsignedSixteenSixteen left, UnsignedSixteenSixteen right) =>
        FromRaw(unchecked(left.RawValue - right.RawValue));

    /// <summary>
    /// Adds a signed change, as the game's speed arithmetic adds a signed 16.16 acceleration to an unsigned speed: the
    /// sum carries and wraps exactly as the two words' 32-bit addition does.
    /// </summary>
    public static UnsignedSixteenSixteen operator +(UnsignedSixteenSixteen speed, SignedSixteenSixteen change) =>
        FromRaw(unchecked(speed.RawValue + (uint)change.RawValue));

    /// <summary>
    /// Subtracts a signed change, as the game's speed arithmetic subtracts a signed 16.16 deceleration from an unsigned
    /// speed: the difference borrows and wraps exactly as the two words' 32-bit subtraction does.
    /// </summary>
    public static UnsignedSixteenSixteen operator -(UnsignedSixteenSixteen speed, SignedSixteenSixteen change) =>
        FromRaw(unchecked(speed.RawValue - (uint)change.RawValue));

    /// <summary>
    /// The value halved <paramref name="halvings"/> times, 0 through 16: each halving moves the lowest whole bit into
    /// the top of the fraction and drops the fraction's lowest bit.
    /// </summary>
    public UnsignedSixteenSixteen Halved(int halvings) => FromRaw(RawValue >> Ensure.BetweenInclusive(0, 16, halvings));

    /// <summary>
    /// The speed as a displacement along its positive direction, as the game moves by a speed word pair: the same
    /// whole and fraction, the whole read as signed, so a speed whose whole word is 32768 or more moves backward.
    /// </summary>
    public SignedSixteenSixteen AsDisplacement() => SignedSixteenSixteen.FromRaw(unchecked((int)RawValue));

    /// <summary>
    /// The displacement's whole and fraction kept as a speed, as the game stores a signed result in a speed's words:
    /// a negative whole becomes a whole word of 32768 or more.
    /// </summary>
    public static UnsignedSixteenSixteen FromDisplacement(SignedSixteenSixteen displacement) =>
        FromRaw(unchecked((uint)displacement.RawValue));

    /// <summary>This speed with its whole word replaced.</summary>
    public UnsignedSixteenSixteen WithWhole(int whole) => new(whole, Fraction);

    /// <summary>This speed with its fraction word replaced.</summary>
    public UnsignedSixteenSixteen WithFraction(int fraction) => new(Whole, fraction);

    /// <summary>
    /// The cartridge's cap test: each word is compared with the cap's word on its own, and both differences must have
    /// the sign bit clear. This is not an unsigned 32-bit comparison; a fraction word of $8000 or more fails it even
    /// when the whole word is above the cap.
    /// </summary>
    public bool WordsAtLeast(UnsignedSixteenSixteen cap) =>
        unchecked((short)(Whole - cap.Whole)) >= 0 && unchecked((short)(Fraction - cap.Fraction)) >= 0;
}
