namespace SuperMetroid.Core.Spatial;

/// <summary>A signed 16.16 fixed-point displacement with explicit whole and fractional halves.</summary>
[EncodingOwner("signed 16.16 fixed-point number")]
public readonly record struct SignedSixteenSixteen : IComparable<SignedSixteenSixteen>, System.Numerics.IUnaryNegationOperators<SignedSixteenSixteen, SignedSixteenSixteen>
{
    /// <summary>Wraps an existing signed 32-bit displacement without altering its whole and fraction bits.</summary>
    private SignedSixteenSixteen(int rawValue) => RawValue = rawValue;

    /// <summary>The displacement in 65536ths of a pixel: its whole pixels above sixteen fractional bits, signed.</summary>
    public int RawValue { get; }

    /// <summary>
    /// Combines the signed whole pixels, -32768 through 32767, and the unsigned fractional word, 0 through 65535.
    /// </summary>
    public static SignedSixteenSixteen FromParts(int wholePixels, int fraction) =>
        new(unchecked((Ensure.BetweenInclusive(short.MinValue, short.MaxValue, wholePixels) << 16) |
            Ensure.BetweenInclusive(0, ushort.MaxValue, fraction)));

    /// <summary>A displacement of <paramref name="wholePixels"/> whole pixels, -32768 through 32767, with no fraction.</summary>
    public static SignedSixteenSixteen FromWholePixels(int wholePixels) => FromParts(wholePixels, 0);

    /// <summary>A displacement of <paramref name="sixteenths"/> sixteenths of a pixel.</summary>
    public static SignedSixteenSixteen FromSixteenthsOfPixel(int sixteenths) => new(checked(sixteenths * 4096));

    /// <summary>
    /// A displacement of <paramref name="twoHundredFiftySixths"/> 256ths of a pixel, the precision of an 8.8 speed: the
    /// whole pixels above eight fraction bits, the fraction's low byte zero.
    /// </summary>
    public static SignedSixteenSixteen FromTwoHundredFiftySixthsOfPixel(int twoHundredFiftySixths) =>
        new(checked(twoHundredFiftySixths * 256));

    /// <summary>
    /// A displacement of <paramref name="numerator"/> <paramref name="denominator"/>ths of a pixel (#2491), the denominator a
    /// power of two from 1 through 65536 so the fraction is kept exactly: the precision the game's finest speeds are
    /// authored in, such as a 512th or a 2048th of a pixel.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The denominator is no power of two from 1 through 65536.</exception>
    /// <exception cref="OverflowException">The displacement lies outside the type's range.</exception>
    public static SignedSixteenSixteen FromPixelFraction(int numerator, int denominator) =>
        new(PowerOfTwoFraction.InFinestUnits(numerator, denominator, FractionsPerPixel));

    /// <summary>
    /// The velocity that is the product of two signed quantities each counted in 256ths (#2569), as a sine share scales a
    /// speed or a turn: 256ths times 256ths are 65536ths of a pixel, the product's sign its direction.
    /// </summary>
    /// <exception cref="OverflowException">The product lies outside the type's range.</exception>
    public static SignedSixteenSixteen ProductOfTwoFiftySixths(int twoFiftySixths, int otherTwoFiftySixths) =>
        new(checked(twoFiftySixths * otherTwoFiftySixths));

    /// <summary>The fractions of a pixel the displacement counts: 65536 to the pixel.</summary>
    private const int FractionsPerPixel = 65536;

    /// <summary>The displacement in pixels, its fraction kept exactly: the amount an installed file states (#2555).</summary>
    public decimal Pixels => RawValue / (decimal)FractionsPerPixel;

    /// <summary>
    /// The displacement of <paramref name="pixels"/> pixels (#2555), as an installed file states it: an amount the type
    /// keeps exactly, a whole number of its finest fractions of a pixel, within its range.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The type cannot keep the amount exactly.</exception>
    public static SignedSixteenSixteen FromPixels(decimal pixels)
    {
        decimal fractions = pixels * FractionsPerPixel;
        if (fractions != decimal.Truncate(fractions) || fractions < int.MinValue || fractions > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(pixels), pixels, "The displacement cannot keep this amount of pixels exactly.");
        return new((int)fractions);
    }

    /// <summary>The zero displacement.</summary>
    public static SignedSixteenSixteen Zero => default;

    /// <summary>
    /// The velocity toward increasing coordinates that covers <paramref name="wholePixels"/> pixels in
    /// <paramref name="updates"/> updates (#2569): the distance in 65536ths of a pixel divided by the updates, the remainder
    /// dropped. A velocity the other way is this one taken toward its direction.
    /// </summary>
    /// <param name="wholePixels">The distance, 0 through 65535 pixels.</param>
    /// <param name="updates">The updates, one or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">The distance or the update count is out of range.</exception>
    /// <exception cref="OverflowException">The velocity lies outside the type's range.</exception>
    public static SignedSixteenSixteen Covering(int wholePixels, int updates) =>
        new(checked((int)(((long)Ensure.BetweenInclusive(0, ushort.MaxValue, wholePixels) << 16) /
            Ensure.BetweenInclusive(1, int.MaxValue, updates))));

    /// <summary>The whole pixels read as unsigned, 0 through 65535: a displacement of -1 whole pixels reads as 65535.</summary>
    public ushort UnsignedWholePixels => unchecked((ushort)(RawValue >> 16));

    /// <summary>The signed whole-pixel word.</summary>
    [IntegerQuantity("whole-pixel part of a signed distance")]
    public int WholePixels => unchecked((short)(RawValue >> 16));

    /// <summary>The unsigned fractional word.</summary>
    public ushort Fraction => unchecked((ushort)RawValue);

    /// <summary>Replaces only the whole pixels and keeps the fraction, as the game does when it sets a position's whole pixels alone.</summary>
    public SignedSixteenSixteen WithWholePixels(int wholePixels) => FromParts(wholePixels, Fraction);

    /// <summary>Replaces only the fraction and keeps the whole pixels, as the game does when it sets a position's fraction alone.</summary>
    public SignedSixteenSixteen WithFraction(int fraction) => FromParts(WholePixels, fraction);

    /// <summary>
    /// The displacement with its whole pixels replaced by <paramref name="speed"/>'s and its own fraction kept (#2555), as
    /// the game sets only a speed's whole pixels and lets the fraction it held carry on.
    /// </summary>
    public SignedSixteenSixteen WithWholePixelsOf(SignedSixteenSixteen speed) => WithWholePixels(speed.WholePixels);

    /// <summary>
    /// The displacement with its whole pixels reversed and its fraction kept (#2555), as the game negates only a speed's
    /// whole pixels when it bounces.
    /// </summary>
    public SignedSixteenSixteen WithWholePixelsReversed() => WithWholePixels(-WholePixels);

    /// <summary>
    /// Whether the displacement's whole pixels reach <paramref name="speed"/>'s whole pixels whichever way it points
    /// (#2555), as a cap that tests only a speed's whole pixels compares them.
    /// </summary>
    public bool WholePixelsReachEitherWay(SignedSixteenSixteen speed) => Math.Abs(WholePixels) >= speed.WholePixels;

    /// <summary>
    /// The part of a pixel the displacement holds past its whole pixels, its whole pixels cleared (#2555): none up to the
    /// finest step short of a pixel, the whole pixels taken toward decreasing coordinates for a negative displacement.
    /// </summary>
    public SignedSixteenSixteen WithoutWholePixels() => FromParts(0, Fraction);

    /// <summary>The displacement with no fraction: its whole pixels, the fraction dropped.</summary>
    public SignedSixteenSixteen WithoutFraction() => FromParts(WholePixels, 0);

    /// <summary>
    /// The displacement with its whole pixels lowered to <paramref name="maximumWhole"/> when they lie above it, its
    /// fraction kept, as the game does when it limits only the whole pixels.
    /// </summary>
    public SignedSixteenSixteen WithWholePixelsAtMost(int maximumWhole) =>
        WholePixels > maximumWhole ? WithWholePixels(maximumWhole) : this;

    /// <summary>
    /// The displacement with its whole pixels raised to <paramref name="minimumWhole"/> when they lie below it, its
    /// fraction kept, as the game does when it limits only the whole pixels.
    /// </summary>
    public SignedSixteenSixteen WithWholePixelsAtLeast(int minimumWhole) =>
        WholePixels < minimumWhole ? WithWholePixels(minimumWhole) : this;

    /// <summary>
    /// The displacement with its whole pixels lowered to <paramref name="most"/>'s when they lie above them, its fraction
    /// kept (#2555), as the game limits only a displacement's whole pixels.
    /// </summary>
    public SignedSixteenSixteen WithWholePixelsAtMost(SignedSixteenSixteen most) => WithWholePixelsAtMost(most.WholePixels);

    /// <summary>
    /// The displacement with its whole pixels raised to <paramref name="least"/>'s when they lie below them, its fraction
    /// kept (#2555), as the game limits only a displacement's whole pixels.
    /// </summary>
    public SignedSixteenSixteen WithWholePixelsAtLeast(SignedSixteenSixteen least) => WithWholePixelsAtLeast(least.WholePixels);

    /// <summary>Adds two displacements, carrying the fraction and wrapping at 32 bits.</summary>
    public static SignedSixteenSixteen operator +(SignedSixteenSixteen left, SignedSixteenSixteen right) =>
        new(unchecked(left.RawValue + right.RawValue));

    /// <summary>Subtracts two displacements, borrowing from the whole word and wrapping at 32 bits.</summary>
    public static SignedSixteenSixteen operator -(SignedSixteenSixteen left, SignedSixteenSixteen right) =>
        new(unchecked(left.RawValue - right.RawValue));

    /// <summary>
    /// The displacement scaled by a whole factor, carrying the fraction and wrapping at 32 bits as the repeated
    /// doublings or additions the game scales a displacement with do.
    /// </summary>
    public static SignedSixteenSixteen operator *(SignedSixteenSixteen value, int factor) =>
        new(unchecked(value.RawValue * factor));

    /// <summary>The displacement reversed: whole and fraction negated together, wrapping at 32 bits.</summary>
    public static SignedSixteenSixteen operator -(SignedSixteenSixteen value) => new(unchecked(-value.RawValue));

    /// <summary>Whether the displacement points backward along its axis: its whole pixels are negative.</summary>
    public bool IsNegative => RawValue < 0;

    /// <summary>
    /// <paramref name="amount"/> taken the way the displacement points (#2569): as it is for a displacement toward
    /// increasing coordinates, or none, and reversed for one toward decreasing coordinates.
    /// </summary>
    public T Along<T>(T amount) where T : System.Numerics.IUnaryNegationOperators<T, T> => IsNegative ? -amount : amount;

    /// <summary>Whether the displacement is no movement at all: zero whole pixels and zero fraction.</summary>
    public bool IsZero => RawValue == 0;

    /// <summary>Whether the displacement points forward along its axis: it is neither backward nor zero.</summary>
    public bool IsPositive => RawValue > 0;

    /// <summary>The fraction bits finer than a 256th of a pixel, which lie below a displacement's reading in 256ths.</summary>
    private const int NearReadingShift = 8;

    /// <summary>
    /// The displacement counted in 256ths of a pixel, the finer part of the fraction dropped toward negative infinity:
    /// the precision at which the game compares Samus's speed with the quicksand surface's upward cap.
    /// </summary>
    public int InTwoFiftySixths => RawValue >> NearReadingShift;

    /// <summary>
    /// The displacement read in 256ths of a pixel and kept at that precision (#2537): its fraction finer than a 256th
    /// dropped toward negative infinity, as <see cref="InTwoFiftySixths"/> drops it.
    /// </summary>
    public SignedSixteenSixteen RoundedDownToTwoFiftySixths() => new(RawValue & ~FinerThanTwoFiftySixth);

    /// <summary>
    /// The displacement scaled by <paramref name="share"/>, as grounded slope movement scales Samus's horizontal travel:
    /// the displacement is first counted in 256ths of a pixel (<see cref="InTwoFiftySixths"/>), then multiplied by the
    /// share's 256ths. The product must fit the type; a displacement whose scaled amount does not fails loudly.
    /// </summary>
    /// <exception cref="OverflowException">The scaled displacement lies outside the type's range.</exception>
    public SignedSixteenSixteen ScaledBy(SpeedShare share) => new(checked(InTwoFiftySixths * share.TwoHundredFiftySixths));

    /// <summary>
    /// The displacement divided by <paramref name="divisor"/> as one amount in 256ths of a pixel (<see cref="InTwoFiftySixths"/>,
    /// #2453): the fraction's part finer than a 256th drops out first, and the quotient is rounded toward negative infinity
    /// to the 256th, so a share backward that falls short of a 256th reads a whole 256th further back.
    /// </summary>
    /// <param name="divisor">The divisor, one or more.</param>
    public SignedSixteenSixteen DividedInTwoFiftySixths(int divisor)
    {
        int quotient = Math.DivRem(InTwoFiftySixths, Ensure.BetweenInclusive(1, int.MaxValue, divisor), out int remainder);
        return FromTwoHundredFiftySixthsOfPixel(remainder < 0 ? quotient - 1 : quotient);
    }

    /// <summary>
    /// The displacement limited to <paramref name="limit"/> counted in 256ths of a pixel, as a quicksand surface limits a
    /// stationary Samus's probe: when its count in 256ths (<see cref="InTwoFiftySixths"/>) lies above the limit's, the
    /// limit's count replaces it and the displacement's fraction finer than a 256th is kept; otherwise it is unchanged.
    /// </summary>
    /// <param name="limit">The largest displacement in 256ths of a pixel.</param>
    public SignedSixteenSixteen WithTwoFiftySixthsAtMost(SignedSixteenSixteen limit) =>
        InTwoFiftySixths > limit.InTwoFiftySixths
            ? new(checked((limit.InTwoFiftySixths << NearReadingShift) + (RawValue & FinerThanTwoFiftySixth)))
            : this;

    /// <summary>The fraction bits finer than a 256th of a pixel.</summary>
    private const int FinerThanTwoFiftySixth = (1 << NearReadingShift) - 1;

    /// <summary>
    /// The displacement with its whole pixels clamped to <paramref name="minimumWhole"/> through
    /// <paramref name="maximumWhole"/>, keeping its fraction, as the game clamps only the whole word of a move.
    /// </summary>
    public SignedSixteenSixteen WithWholePixelsClamped(int minimumWhole, int maximumWhole) =>
        FromParts(Math.Clamp(WholePixels, minimumWhole, maximumWhole), Fraction);

    /// <summary>
    /// The displacement's distance regardless of direction, as the game hands a mover's collision probe an unsigned
    /// magnitude: the negation of a backward displacement, kept as a speed's words.
    /// </summary>
    public UnsignedSixteenSixteen Magnitude() => UnsignedSixteenSixteen.FromDisplacement(IsNegative ? -this : this);

    /// <summary>The smaller of two displacements, read signed.</summary>
    public static SignedSixteenSixteen Min(SignedSixteenSixteen left, SignedSixteenSixteen right) => left <= right ? left : right;

    /// <summary>Orders displacements by their signed value.</summary>
    public int CompareTo(SignedSixteenSixteen other) => RawValue.CompareTo(other.RawValue);

    /// <summary>Whether <paramref name="left"/> is the lesser displacement, read signed.</summary>
    public static bool operator <(SignedSixteenSixteen left, SignedSixteenSixteen right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> is the greater displacement, read signed.</summary>
    public static bool operator >(SignedSixteenSixteen left, SignedSixteenSixteen right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> is no greater than <paramref name="right"/>, read signed.</summary>
    public static bool operator <=(SignedSixteenSixteen left, SignedSixteenSixteen right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> is no less than <paramref name="right"/>, read signed.</summary>
    public static bool operator >=(SignedSixteenSixteen left, SignedSixteenSixteen right) => left.CompareTo(right) >= 0;
}
