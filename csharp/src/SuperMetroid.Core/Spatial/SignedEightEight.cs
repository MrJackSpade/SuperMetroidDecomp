namespace SuperMetroid.Core.Spatial;

/// <summary>A signed 8.8 fixed-point velocity: eight whole bits and eight fraction bits, kept exactly.</summary>
[EncodingOwner("signed 8.8 fixed-point number")]
public readonly record struct SignedEightEight : IComparable<SignedEightEight>
{
    /// <summary>The value whose two's-complement word is <paramref name="rawValue"/>, 0 through 65535.</summary>
    /// <param name="rawValue">The unchanged two's-complement word containing eight whole and eight fractional bits.</param>
    public SignedEightEight(int rawValue) => RawValue = checked((ushort)rawValue);

    /// <summary>The unchanged two's-complement word containing eight whole and eight fractional bits.</summary>
    public ushort RawValue { get; init; }

    /// <summary>No velocity at all.</summary>
    public static SignedEightEight Zero => default;

    /// <summary>
    /// A speed of <paramref name="twoHundredFiftySixths"/> 256ths of a pixel an update, -32768 through 32767: the precision
    /// the game's speeds and accelerations are authored in.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed lies outside the type's range.</exception>
    public static SignedEightEight FromTwoHundredFiftySixthsOfPixel(int twoHundredFiftySixths) =>
        new(unchecked((ushort)Ensure.BetweenInclusive(short.MinValue, short.MaxValue, twoHundredFiftySixths)));

    /// <summary>A speed of <paramref name="pixels"/> whole pixels an update, -128 through 127, with no fraction.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The speed lies outside the type's range.</exception>
    public static SignedEightEight FromPixels(int pixels) => FromParts(pixels, 0);

    /// <summary>The speed in 256ths of a pixel an update, negative toward decreasing coordinates.</summary>
    [IntegerQuantity("signed speed in 256ths of a pixel an update")]
    public int InTwoHundredFiftySixthsOfPixel => unchecked((short)RawValue);

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
    public int UnsignedWholePixels => EightEight.WholeOf(RawValue);

    /// <summary>
    /// The speed in 256ths of a pixel an update read without its sign, 0 through 65535, as a multiply that takes the whole
    /// word as an unsigned magnitude does: a negative velocity reads 65536 larger.
    /// </summary>
    [IntegerQuantity("unsigned reading of an 8.8 speed in 256ths of a pixel an update")]
    public int UnsignedTwoHundredFiftySixthsOfPixel => RawValue;

    /// <summary>The fraction's eight bits, 0 through 255 256ths of a pixel.</summary>
    [IntegerQuantity("fraction byte of an 8.8 velocity")]
    public int FractionSubpixels => EightEight.FractionOf(RawValue);

    /// <summary>Whether the velocity points toward decreasing coordinates, leftward or upward.</summary>
    public bool IsNegative => InTwoHundredFiftySixthsOfPixel < 0;

    /// <summary>The velocity's whole pixels, its fraction dropped toward negative infinity.</summary>
    [IntegerQuantity("whole-pixel part of a signed 8.8 velocity")]
    public int WholePixels => InTwoHundredFiftySixthsOfPixel >> 8;

    /// <summary>The whole pixels of the velocity's speed, whichever way it points: its magnitude, the fraction dropped.</summary>
    [IntegerQuantity("whole-pixel part of a signed 8.8 speed")]
    public int MagnitudeWholePixels => Math.Abs(InTwoHundredFiftySixthsOfPixel) >> 8;

    /// <summary>The velocity halved, its lowest fraction bit dropped toward negative infinity.</summary>
    public SignedEightEight Halved() => FromTwoHundredFiftySixthsOfPixel(InTwoHundredFiftySixthsOfPixel >> 1);

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
    public static SignedEightEight operator -(SignedEightEight value) =>
        FromTwoHundredFiftySixthsOfPixel(-value.InTwoHundredFiftySixthsOfPixel);

    /// <summary>
    /// The velocity times a factor in 256ths, <paramref name="factorInTwoHundredFiftySixths"/>, as a 16.16 displacement
    /// that keeps the product's full precision: the product of the two 8.8 values, nothing truncated.
    /// </summary>
    /// <exception cref="OverflowException">The product lies outside 32 bits.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The product's whole pixels lie outside the 16.16 range.</exception>
    public SignedSixteenSixteen ScaledBy(int factorInTwoHundredFiftySixths)
    {
        // A product of two 256ths counts 65536ths of a pixel: its whole pixels above sixteen fraction bits.
        int product = checked(InTwoHundredFiftySixthsOfPixel * factorInTwoHundredFiftySixths);
        return SignedSixteenSixteen.FromParts(product >> 16, product & ushort.MaxValue);
    }

    /// <summary>The same velocity as the 16.16 displacement position adders move by.</summary>
    public SignedSixteenSixteen ToSixteenSixteenDelta() =>
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(InTwoHundredFiftySixthsOfPixel);

    /// <summary>Whether the velocity is no movement at all.</summary>
    public bool IsZero => RawValue == 0;

    /// <summary>The velocity of <paramref name="wholePixels"/> pixels, -128 through 127, and <paramref name="fraction"/> 256ths, 0 through 255, an update.</summary>
    public static SignedEightEight FromParts(int wholePixels, int fraction) =>
        FromTwoHundredFiftySixthsOfPixel(Ensure.BetweenInclusive(sbyte.MinValue, sbyte.MaxValue, wholePixels) * 256 + Ensure.BetweenInclusive(0, byte.MaxValue, fraction));

    /// <summary>
    /// The velocity's whole quarter pixels an update, rounded toward decreasing coordinates, as the game reads a speed's
    /// quarter pixels from its word.
    /// </summary>
    [IntegerQuantity("whole quarter pixels of a signed 8.8 speed")]
    public int WholeQuarterPixels => InTwoHundredFiftySixthsOfPixel >> 6;

    /// <summary>
    /// The velocity with its whole pixels lowered to <paramref name="maximumWhole"/>, -128 through 127, when they lie above
    /// it, its fraction kept, as routines that cap only a speed's whole byte do.
    /// </summary>
    public SignedEightEight WithWholePixelsAtMost(int maximumWhole) =>
        WholePixels > maximumWhole ? FromParts(maximumWhole, FractionSubpixels) : this;

    /// <summary>The velocity's speed regardless of direction; the most negative value has none within the type's range and fails.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is the most negative one.</exception>
    public SignedEightEight Magnitude() => IsNegative ? -this : this;

    /// <summary>This speed pointed the way <paramref name="direction"/> points: reversed when that velocity is negative.</summary>
    public SignedEightEight AlongDirectionOf(SignedEightEight direction) => direction.IsNegative ? -this : this;

    /// <summary>
    /// The velocity slowed by <paramref name="drag"/>: its speed lowered by the drag, stopping at rest, and its direction kept,
    /// as the game applies drag to a speed's magnitude.
    /// </summary>
    public SignedEightEight SlowedBy(SignedEightEight drag)
    {
        int speed = Math.Max(0, Math.Abs(InTwoHundredFiftySixthsOfPixel) - drag.InTwoHundredFiftySixthsOfPixel);
        return FromTwoHundredFiftySixthsOfPixel(IsNegative ? -speed : speed);
    }

    /// <summary>
    /// The velocity divided by <paramref name="divisor"/>, a positive whole number, rounding toward decreasing coordinates,
    /// as the game halves or quarters a velocity by shifting its word.
    /// </summary>
    public SignedEightEight DividedRoundingDown(int divisor)
    {
        int quotient = Math.DivRem(InTwoHundredFiftySixthsOfPixel, Ensure.BetweenInclusive(1, int.MaxValue, divisor), out int remainder);
        return FromTwoHundredFiftySixthsOfPixel(remainder < 0 ? quotient - 1 : quotient);
    }

    /// <summary>The smaller of two velocities, read signed.</summary>
    public static SignedEightEight Min(SignedEightEight left, SignedEightEight right) => left <= right ? left : right;

    /// <summary>The larger of two velocities, read signed.</summary>
    public static SignedEightEight Max(SignedEightEight left, SignedEightEight right) => left >= right ? left : right;

    /// <summary>Orders velocities by their signed value.</summary>
    public int CompareTo(SignedEightEight other) => InTwoHundredFiftySixthsOfPixel.CompareTo(other.InTwoHundredFiftySixthsOfPixel);

    /// <summary>Whether <paramref name="left"/> is the lesser velocity, read signed.</summary>
    public static bool operator <(SignedEightEight left, SignedEightEight right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> is the greater velocity, read signed.</summary>
    public static bool operator >(SignedEightEight left, SignedEightEight right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> is no greater than <paramref name="right"/>, read signed.</summary>
    public static bool operator <=(SignedEightEight left, SignedEightEight right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> is no less than <paramref name="right"/>, read signed.</summary>
    public static bool operator >=(SignedEightEight left, SignedEightEight right) => left.CompareTo(right) >= 0;
}
