namespace SuperMetroid.Core.Spatial;

/// <summary>
/// An unsigned 16.16 fixed-point speed kept as its two halves: a whole part and a fraction, each 0 through 65535. It is how
/// fast something moves, never which way: a velocity type gives a speed its direction (Samus's vertical velocity,
/// <see cref="Samus.Movement.SamusVerticalVelocity"/>, and her running speed taken toward her side), and how a speed slowed
/// past zero reads is <see cref="SamusSpeedMagnitude"/>'s.
/// </summary>
public readonly partial record struct UnsignedSixteenSixteen
{
    /// <summary>Holds the two parts, each checked to lie in 0 through 65535.</summary>
    public UnsignedSixteenSixteen(int whole, int fraction)
    {
        Whole = Ensure.BetweenInclusive(0, ushort.MaxValue, whole);
        Fraction = Ensure.BetweenInclusive(0, ushort.MaxValue, fraction);
    }

    /// <summary>The whole part, 0 through 65535.</summary>
    public int Whole { get; }

    /// <summary>The fraction, 0 through 65535.</summary>
    public int Fraction { get; }

    /// <summary>Both parts as one unsigned 32-bit value, the whole part high.</summary>
    public uint RawValue => ((uint)Whole << 16) | (uint)Fraction;

    /// <summary>Whether the speed holds no fraction of a pixel: it is a whole number of pixels (#2555).</summary>
    public bool IsWholePixels => Fraction == 0;

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

    /// <summary>No speed or distance at all.</summary>
    public static UnsignedSixteenSixteen Zero => default;

    /// <summary>A distance of one whole pixel and no fraction.</summary>
    public static UnsignedSixteenSixteen OnePixel => FromWholePixels(1);

    /// <summary>A distance of <paramref name="wholePixels"/> whole pixels and no fraction, 0 through 65535.</summary>
    public static UnsignedSixteenSixteen FromWholePixels(int wholePixels) => new(wholePixels, 0);

    /// <summary>A speed of <paramref name="twoHundredFiftySixths"/> 256ths of a pixel, which must not be negative.</summary>
    internal static UnsignedSixteenSixteen FromTwoHundredFiftySixthsOfPixel(int twoHundredFiftySixths) =>
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
    public static UnsignedSixteenSixteen FromPixelFraction(int numerator, int denominator) =>
        FromRaw((uint)PowerOfTwoFraction.InFinestUnits(Ensure.AtLeastZero(numerator), denominator, FractionsPerPixel));

    /// <summary>The fractions of a pixel the speed counts: 65536 to the pixel.</summary>
    private const int FractionsPerPixel = 65536;

    /// <summary>
    /// Whether this speed is faster than <paramref name="limit"/> when both are read to a 256th of a pixel (#2555), the
    /// precision the game compares Samus's speed with a quicksand surface's upward cap at: finer fractions take no part.
    /// </summary>
    public bool IsFasterInTwoFiftySixthsThan(SignedSixteenSixteen limit) => InTwoFiftySixths > limit.InTwoFiftySixths;

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
    /// at sixteen bits, so a speed slowed past zero holds a whole part of 32768 or more, which reads as having passed zero
    /// (<see cref="SamusSpeedMagnitude.HasPassedZero"/>).
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
    /// The value divided by <paramref name="divisor"/> as one amount in 256ths (<see cref="InTwoFiftySixths"/>): the
    /// fraction's part finer than a 256th drops out first, and the quotient's remainder below a 256th is dropped, so a
    /// whole unit the division moves into the fraction is kept to the 256th.
    /// </summary>
    /// <param name="divisor">The divisor, one or more.</param>
    public UnsignedSixteenSixteen DividedInTwoFiftySixths(int divisor) =>
        FromTwoHundredFiftySixthsOfPixel(InTwoFiftySixths / Ensure.BetweenInclusive(1, int.MaxValue, divisor));

    /// <summary>This speed with its whole part replaced.</summary>
    internal UnsignedSixteenSixteen WithWhole(int whole) => new(whole, Fraction);

    /// <summary>
    /// This speed scaled by a whole <paramref name="factor"/>, zero or more (#2555): the product must fit the type.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The factor is negative.</exception>
    /// <exception cref="OverflowException">The product lies outside the type's range.</exception>
    public UnsignedSixteenSixteen Times(int factor) => FromRaw(checked(RawValue * (uint)Ensure.AtLeastZero(factor)));

    /// <summary>
    /// This speed with its whole pixels lowered to <paramref name="most"/>'s when they lie above them, its own fraction kept
    /// (#2555), as the game caps only a speed's whole pixels.
    /// </summary>
    public UnsignedSixteenSixteen WithWholePixelsAtMost(UnsignedSixteenSixteen most) => Whole > most.Whole ? WithWhole(most.Whole) : this;

    /// <summary>
    /// This speed with its whole pixels halved, an odd pixel dropped rather than halved into the fraction, and its fraction
    /// kept (#2555).
    /// </summary>
    public UnsignedSixteenSixteen WithWholePixelsHalved() => new(Whole / 2, Fraction);

    /// <summary>
    /// This speed and <paramref name="other"/> added part by part (#2555): the whole pixels added, and the fractions added
    /// with the carry past a whole pixel dropped, so a fraction sum past a pixel keeps only its remainder.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The whole pixels' sum lies outside the type's range.</exception>
    public UnsignedSixteenSixteen AddedPartwise(UnsignedSixteenSixteen other) =>
        new(Whole + other.Whole, (Fraction + other.Fraction) % FractionsPerPixel);

    /// <summary>
    /// This speed with its whole pixels replaced by <paramref name="speed"/>'s and its own fraction kept (#2555), as the
    /// game sets only a speed's whole pixels.
    /// </summary>
    public UnsignedSixteenSixteen WithWholePixelsOf(UnsignedSixteenSixteen speed) => new(speed.Whole, Fraction);

    /// <summary>
    /// How many whole steps of this size fit within <paramref name="wholePixels"/> pixels, the part of a step left over
    /// dropped (#2181): the lines a band climbing this far each line takes to climb that far.
    /// </summary>
    /// <exception cref="DivideByZeroException">This step is zero.</exception>
    public int WholeStepsWithin(int wholePixels) =>
        checked((int)(((ulong)Ensure.AtLeastZero(wholePixels) << 16) / RawValue));
}
