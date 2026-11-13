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

    /// <summary>Whether the speed is no motion at all: zero whole pixels and zero fraction.</summary>
    public bool IsZero => Whole == 0 && Fraction == 0;

    /// <summary>
    /// The speed counted in 256ths of a pixel, the finer part of the fraction dropped: the precision at which the game
    /// compares Samus's speed with the quicksand surface's upward cap.
    /// </summary>
    public int InTwoFiftySixths => (int)(RawValue >> 8);

    /// <summary>Splits an unsigned 32-bit value into its whole and fraction words.</summary>
    public static UnsignedSixteenSixteen FromRaw(uint rawValue) =>
        new(unchecked((ushort)(rawValue >> 16)), unchecked((ushort)rawValue));

    /// <summary>
    /// The speed that is the product of two non-negative quantities each counted in 256ths, as the grapple release
    /// scales the swing's speed by a trigonometric component: 256ths times 256ths are 65536ths of a pixel. The product
    /// must fit the type.
    /// </summary>
    /// <param name="twoFiftySixths">The first factor in 256ths, zero or more.</param>
    /// <param name="otherTwoFiftySixths">The second factor in 256ths, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">A factor is negative.</exception>
    /// <exception cref="OverflowException">The product lies outside the type's range.</exception>
    public static UnsignedSixteenSixteen ProductOfTwoFiftySixths(int twoFiftySixths, int otherTwoFiftySixths) =>
        FromRaw(checked((uint)Ensure.BetweenInclusive(0, int.MaxValue, twoFiftySixths) *
            (uint)Ensure.BetweenInclusive(0, int.MaxValue, otherTwoFiftySixths)));

    /// <summary>
    /// The speed that is a share of <paramref name="wholePixels"/> whole pixels, the share counted in 65536ths, as a mover
    /// scales a whole-pixel speed by a sine sample: whole pixels times 65536ths are 65536ths of a pixel. The product must
    /// fit the type.
    /// </summary>
    /// <param name="wholePixels">The whole-pixel speed, zero or more.</param>
    /// <param name="share">The share in 65536ths, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">A factor is negative.</exception>
    /// <exception cref="OverflowException">The product lies outside the type's range.</exception>
    public static UnsignedSixteenSixteen ShareOfWholePixels(int wholePixels, int share) =>
        FromRaw(checked((uint)Ensure.BetweenInclusive(0, int.MaxValue, wholePixels) *
            (uint)Ensure.BetweenInclusive(0, int.MaxValue, share)));

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

    /// <summary>
    /// The speed that covers <paramref name="wholePixels"/> pixels in <paramref name="updates"/> updates: the distance in
    /// 65536ths of a pixel divided by the updates, the remainder dropped.
    /// </summary>
    /// <param name="wholePixels">The distance, 0 through 65535 pixels.</param>
    /// <param name="updates">The updates, one or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">The distance or the update count is out of range.</exception>
    public static UnsignedSixteenSixteen Covering(int wholePixels, int updates) =>
        FromRaw(checked((uint)(((long)Ensure.BetweenInclusive(0, ushort.MaxValue, wholePixels) << 16) /
            Ensure.BetweenInclusive(1, int.MaxValue, updates))));

    /// <summary>This speed with its whole word replaced.</summary>
    public UnsignedSixteenSixteen WithWhole(int whole) => new(whole, Fraction);
}
