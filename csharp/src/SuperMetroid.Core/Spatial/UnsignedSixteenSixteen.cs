namespace SuperMetroid.Core.Spatial;

/// <summary>
/// An unsigned 16.16 fixed-point speed kept as its two halves: a whole part and a fraction, each 0 through 65535.
/// Samus's speeds are magnitudes in this form; their direction is held separately.
/// </summary>
[EncodingOwner("unsigned 16.16 fixed-point number")]
public readonly record struct UnsignedSixteenSixteen
{
    /// <summary>Holds the two parts, each checked to lie in 0 through 65535.</summary>
    public UnsignedSixteenSixteen(int whole, int fraction)
    {
        Whole = Ensure.BetweenInclusive(0, ushort.MaxValue, whole);
        Fraction = Ensure.BetweenInclusive(0, ushort.MaxValue, fraction);
    }

    /// <summary>The whole part, 0 through 65535.</summary>
    [IntegerQuantity("whole-pixel part of an unsigned distance")]
    public int Whole { get; }

    /// <summary>The fraction, 0 through 65535.</summary>
    public int Fraction { get; }

    /// <summary>Both parts as one unsigned 32-bit value, the whole part high.</summary>
    public uint RawValue => ((uint)Whole << 16) | (uint)Fraction;

    /// <summary>Whether the speed is no motion at all: zero whole pixels and zero fraction.</summary>
    public bool IsZero => Whole == 0 && Fraction == 0;

    /// <summary>
    /// The speed counted in 256ths of a pixel, the finer part of the fraction dropped: the precision at which the game
    /// compares Samus's speed with the quicksand surface's upward cap.
    /// </summary>
    public int InTwoFiftySixths => (int)(RawValue >> 8);

    /// <summary>Splits an unsigned 32-bit value into its whole part and fraction.</summary>
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

    /// <summary>A distance of <paramref name="wholePixels"/> whole pixels and no fraction, 0 through 65535.</summary>
    public static UnsignedSixteenSixteen FromWholePixels(int wholePixels) => new(wholePixels, 0);

    /// <summary>A speed of <paramref name="sixteenths"/> sixteenths of a pixel, which must not be negative.</summary>
    public static UnsignedSixteenSixteen FromSixteenthsOfPixel(int sixteenths) => FromRaw(checked((uint)sixteenths * 4096u));

    /// <summary>A speed of <paramref name="twoHundredFiftySixths"/> 256ths of a pixel, which must not be negative.</summary>
    public static UnsignedSixteenSixteen FromTwoHundredFiftySixthsOfPixel(int twoHundredFiftySixths) =>
        FromRaw(checked((uint)twoHundredFiftySixths * 256u));

    /// <summary>
    /// A speed of <paramref name="numerator"/> <paramref name="denominator"/>ths of a pixel (#2539), the denominator a power
    /// of two from 1 through 65536 so the fraction is kept exactly, as <see cref="SignedSixteenSixteen.FromPixelFraction"/>
    /// reads a displacement.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The numerator is negative, or the denominator is no power of two from 1 through 65536.
    /// </exception>
    /// <exception cref="OverflowException">The speed lies outside the type's range.</exception>
    public static UnsignedSixteenSixteen FromPixelFraction(int numerator, int denominator)
    {
        int fractionsEach = FractionsPerPixel / Ensure.BetweenInclusive(1, FractionsPerPixel, denominator);
        if (fractionsEach * denominator != FractionsPerPixel)
            throw new ArgumentOutOfRangeException(nameof(denominator), denominator, "The denominator must be a power of two from 1 through 65536.");
        return FromRaw(checked((uint)Ensure.AtLeastZero(numerator) * (uint)fractionsEach));
    }

    /// <summary>The fractions of a pixel the speed counts: 65536 to the pixel.</summary>
    private const int FractionsPerPixel = 65536;

    /// <summary>Whether this speed is slower than <paramref name="other"/>, comparing whole pixels and fraction together.</summary>
    public bool IsSlowerThan(UnsignedSixteenSixteen other) => RawValue < other.RawValue;

    /// <summary>
    /// Adds two values: a fraction sum that passes a whole moves the whole part by one, and the whole part wraps at its
    /// sixteen bits.
    /// </summary>
    public static UnsignedSixteenSixteen operator +(UnsignedSixteenSixteen left, UnsignedSixteenSixteen right) =>
        FromRaw(unchecked(left.RawValue + right.RawValue));

    /// <summary>
    /// Subtracts two values: a fraction that drops below zero borrows one from the whole part, and the whole part wraps
    /// at sixteen bits, so a speed slowed past zero holds a whole part of 32768 or more, which the game's sign tests read as
    /// having passed zero.
    /// </summary>
    public static UnsignedSixteenSixteen operator -(UnsignedSixteenSixteen left, UnsignedSixteenSixteen right) =>
        FromRaw(unchecked(left.RawValue - right.RawValue));

    /// <summary>
    /// Adds a signed change, as the game's speed arithmetic adds a signed 16.16 acceleration to an unsigned speed: the
    /// sum overflows and wraps exactly as the two parts' 32-bit addition does.
    /// </summary>
    public static UnsignedSixteenSixteen operator +(UnsignedSixteenSixteen speed, SignedSixteenSixteen change) =>
        FromRaw(unchecked(speed.RawValue + (uint)change.RawValue));

    /// <summary>
    /// The value halved <paramref name="halvings"/> times, 0 through 16: each halving moves the lowest whole bit into
    /// the top of the fraction and drops the fraction's lowest bit.
    /// </summary>
    public UnsignedSixteenSixteen Halved(int halvings) => FromRaw(RawValue >> Ensure.BetweenInclusive(0, 16, halvings));

    /// <summary>
    /// The speed as a displacement along its positive direction, as the game moves by a speed's two parts: the same
    /// whole and fraction, the whole read as signed, so a speed whose whole part is 32768 or more moves backward.
    /// </summary>
    public SignedSixteenSixteen AsDisplacement() => SignedSixteenSixteen.FromParts(unchecked((short)Whole), Fraction);

    /// <summary>
    /// The displacement's whole and fraction kept as a speed, as the game stores a signed result in a speed's parts:
    /// a negative whole becomes a whole part of 32768 or more.
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

    /// <summary>
    /// The value divided by <paramref name="divisor"/> as one amount in 256ths (<see cref="InTwoFiftySixths"/>): the
    /// fraction's part finer than a 256th drops out first, and the quotient's remainder below a 256th is dropped, so a
    /// whole unit the division moves into the fraction is kept to the 256th.
    /// </summary>
    /// <param name="divisor">The divisor, one or more.</param>
    public UnsignedSixteenSixteen DividedInTwoFiftySixths(int divisor) =>
        FromTwoHundredFiftySixthsOfPixel(InTwoFiftySixths / Ensure.BetweenInclusive(1, int.MaxValue, divisor));

    /// <summary>This speed with its whole part replaced.</summary>
    public UnsignedSixteenSixteen WithWhole(int whole) => new(whole, Fraction);

    /// <summary>
    /// How many whole steps of this size fit within <paramref name="wholePixels"/> pixels, the part of a step left over
    /// dropped (#2181): the lines a band climbing this far each line takes to climb that far.
    /// </summary>
    /// <exception cref="DivideByZeroException">This step is zero.</exception>
    public int WholeStepsWithin(int wholePixels) =>
        checked((int)(((ulong)Ensure.AtLeastZero(wholePixels) << 16) / RawValue));
}
