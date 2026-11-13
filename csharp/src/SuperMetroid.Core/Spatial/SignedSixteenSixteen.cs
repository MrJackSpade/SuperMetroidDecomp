namespace SuperMetroid.Core.Spatial;

/// <summary>A signed 16.16 fixed-point displacement with explicit whole and fractional halves.</summary>
[EncodingOwner("signed 16.16 fixed-point number")]
public readonly record struct SignedSixteenSixteen
{
    /// <summary>Wraps an existing signed 32-bit displacement without altering its whole and fraction bits.</summary>
    private SignedSixteenSixteen(int rawValue) => RawValue = rawValue;

    /// <summary>Gets the signed native word containing sixteen whole and sixteen fractional bits.</summary>
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

    /// <summary>The zero displacement.</summary>
    public static SignedSixteenSixteen Zero => default;

    /// <summary>The whole-pixel word as its unsigned native bit pattern.</summary>
    public ushort UnsignedWholePixels => unchecked((ushort)(RawValue >> 16));

    /// <summary>The signed whole-pixel word.</summary>
    [IntegerQuantity("whole-pixel part of a signed distance")]
    public int WholePixels => unchecked((short)(RawValue >> 16));

    /// <summary>The unsigned fractional word.</summary>
    public ushort Fraction => unchecked((ushort)RawValue);

    /// <summary>Replaces only the whole word, as cartridge routines that store one half do.</summary>
    public SignedSixteenSixteen WithWholePixels(int wholePixels) => FromParts(wholePixels, Fraction);

    /// <summary>Replaces only the fractional word, as cartridge routines that store one half do.</summary>
    public SignedSixteenSixteen WithFraction(int fraction) => FromParts(WholePixels, fraction);

    /// <summary>The displacement with no fraction: its whole pixels, the fraction dropped.</summary>
    public SignedSixteenSixteen WithoutFraction() => FromParts(WholePixels, 0);

    /// <summary>
    /// The displacement with its whole pixels lowered to <paramref name="maximumWhole"/> when they lie above it, its
    /// fraction kept, as routines that clamp only the whole pixels do.
    /// </summary>
    public SignedSixteenSixteen WithWholePixelsAtMost(int maximumWhole) =>
        WholePixels > maximumWhole ? WithWholePixels(maximumWhole) : this;

    /// <summary>
    /// The displacement with its whole pixels raised to <paramref name="minimumWhole"/> when they lie below it, its
    /// fraction kept, as routines that clamp only the whole pixels do.
    /// </summary>
    public SignedSixteenSixteen WithWholePixelsAtLeast(int minimumWhole) =>
        WholePixels < minimumWhole ? WithWholePixels(minimumWhole) : this;

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

    /// <summary>
    /// The displacement split into <paramref name="parts"/> equal parts, a positive whole number, each part rounded toward
    /// no movement in 65536ths of a pixel.
    /// </summary>
    public SignedSixteenSixteen DividedBy(int parts) => new(RawValue / Ensure.BetweenInclusive(1, int.MaxValue, parts));

    /// <summary>Whether the displacement points backward along its axis: its whole pixels are negative.</summary>
    public bool IsNegative => RawValue < 0;

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
    /// The displacement scaled by <paramref name="twoFiftySixths"/>/256, as grounded slope movement scales Samus's
    /// horizontal travel: the displacement is first counted in 256ths of a pixel (<see cref="InTwoFiftySixths"/>), then
    /// multiplied. The product must fit the type; a displacement whose scaled amount does not fails loudly.
    /// </summary>
    /// <exception cref="OverflowException">The scaled displacement lies outside the type's range.</exception>
    public SignedSixteenSixteen ScaledByTwoFiftySixths(int twoFiftySixths) => new(checked(InTwoFiftySixths * twoFiftySixths));

    /// <summary>
    /// The displacement's near reading in 256ths of a pixel, 0 through 65535, as a quicksand surface measures a stationary
    /// Samus's probe: its whole pixels below 256 above its fraction's 256ths. Whole pixels of 256 or more, the sign and
    /// the fraction's finest part do not take part.
    /// </summary>
    public int NearTwoFiftySixths => unchecked((ushort)(RawValue >> NearReadingShift));

    /// <summary>
    /// The displacement with its near reading in 256ths of a pixel (<see cref="NearTwoFiftySixths"/>) replaced by
    /// <paramref name="nearTwoFiftySixths"/>, 0 through 65535, its whole pixels from 256 upward, its sign and its
    /// fraction's finest part kept, as a quicksand surface limits a stationary Samus's probe.
    /// </summary>
    public SignedSixteenSixteen WithNearTwoFiftySixths(int nearTwoFiftySixths) =>
        new((RawValue & ~(ushort.MaxValue << NearReadingShift)) | (Ensure.BetweenInclusive(0, ushort.MaxValue, nearTwoFiftySixths) << NearReadingShift));

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

    /// <summary>
    /// The displacement reversed by negating its whole pixels and its fraction each on their own, as several enemies
    /// reverse one: the fraction's borrow never reaches the whole pixels. It equals the true reversal when the fraction is
    /// zero and lies one pixel further along the original direction when it is not.
    /// </summary>
    public SignedSixteenSixteen NegatedPartsSeparately() =>
        FromParts(unchecked((short)-WholePixels), unchecked((ushort)-Fraction));

    /// <summary>
    /// The displacement reversed by complementing its whole pixels and negating its fraction on its own, as several
    /// enemies reverse one: it equals the true reversal when the fraction is not zero and lies one pixel further along
    /// the reversed direction when it is.
    /// </summary>
    public SignedSixteenSixteen ComplementedWholeNegatedFraction() =>
        FromParts(~WholePixels, unchecked((ushort)-Fraction));

    /// <summary>Adds this delta to an unsigned wrapped 16.16 world-position pair, each word 0 through 65535.</summary>
    public FixedCoordinate AddTo(int wholePosition, int subposition) =>
        FixedCoordinate.Of(wholePosition, subposition).MovedBy(this);
}
