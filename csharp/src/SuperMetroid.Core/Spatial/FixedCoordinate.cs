namespace SuperMetroid.Core.Spatial;

/// <summary>One axis of a 16.16 fixed-point position: the whole pixel and its fraction, each wrapping at sixteen bits.</summary>
[EncodingOwner("16.16 fixed-point coordinate")]
public readonly record struct FixedCoordinate
{
    /// <summary>The position at <paramref name="whole"/> pixels and <paramref name="fraction"/> 65536ths, each 0 through 65535.</summary>
    /// <param name="whole">The whole pixel, 0 through 65535; the coordinate range wraps past either end.</param>
    /// <param name="fraction">The fraction of a pixel, in 65536ths.</param>
    public FixedCoordinate(int whole, int fraction)
    {
        Whole = checked((ushort)whole);
        Fraction = checked((ushort)fraction);
    }

    /// <summary>The whole pixel, 0 through 65535; the coordinate range wraps past either end.</summary>
    [IntegerQuantity("whole-pixel coordinate")]
    public ushort Whole { get; init; }

    /// <summary>The fraction of a pixel, in 65536ths, 0 through 65535.</summary>
    public ushort Fraction { get; init; }

    /// <summary>The position at <paramref name="whole"/> pixels and <paramref name="fraction"/> 65536ths, each 0 through 65535.</summary>
    public static FixedCoordinate Of(int whole, int fraction) => new(whole, fraction);

    /// <summary>
    /// The whole pixel, 0 through 65535, of a position counted in 65536ths of a pixel (<see cref="Subpixels"/>), whatever
    /// the count's sign. Kept for the Samus and boss callers and for checks that read a recorded count; gameplay in other
    /// areas moves positions with <see cref="MovedBy(SignedSixteenSixteen)"/>.
    /// </summary>
    public static ushort WholeOf(int subpixels) => unchecked((ushort)(subpixels >> 16));

    /// <summary>
    /// The position <paramref name="subpixels"/> 65536ths of a pixel from the origin, around the wrapping coordinate range.
    /// Kept for checks that state a recorded position as one count; gameplay builds positions from their whole pixel and
    /// fraction.
    /// </summary>
    public static FixedCoordinate FromRaw(uint subpixels) =>
        new(unchecked((ushort)(subpixels >> 16)), unchecked((ushort)subpixels));

    /// <summary>
    /// The position counted in 65536ths of a pixel from the origin. Kept for the Samus collision callers and for checks
    /// that compare a recorded position as one count; gameplay elsewhere reads <see cref="Whole"/> and <see cref="Fraction"/>.
    /// </summary>
    public uint RawValue => Subpixels;

    /// <summary>The position counted in 65536ths of a pixel from the origin: the whole pixel times 65536 plus the fraction.</summary>
    private uint Subpixels => ((uint)Whole << 16) | Fraction;

    /// <summary>Replaces only the whole pixel, 0 through 65535, keeping the fraction.</summary>
    public FixedCoordinate WithWhole(int whole) => this with { Whole = checked((ushort)whole) };

    /// <summary>
    /// Replaces only the whole pixel with a computed coordinate, which wraps around the coordinate range
    /// (<see cref="CoordinateAxis.Wrap"/>), keeping the fraction.
    /// </summary>
    public FixedCoordinate WithWrappedWhole(int whole) => WithWhole(CoordinateAxis.Wrap(whole));

    /// <summary>Replaces only the fraction, 0 through 65535 65536ths of a pixel, keeping the whole pixel.</summary>
    public FixedCoordinate WithFraction(int fraction) => this with { Fraction = checked((ushort)fraction) };

    /// <summary>
    /// This position moved by <paramref name="displacement"/>: the fraction carries into the whole pixel, and the whole
    /// pixel wraps around the coordinate range in either direction, so a mover pushed past either edge reappears at the
    /// other, exactly as the game's 16.16 movers place it.
    /// </summary>
    public FixedCoordinate MovedBy(SignedSixteenSixteen displacement) =>
        FromRaw(unchecked(Subpixels + (uint)displacement.RawValue));

    /// <summary>
    /// This position moved toward increasing coordinates by <paramref name="distance"/>, the fraction carrying into the
    /// whole pixel, which wraps around the coordinate range as <see cref="MovedBy(SignedSixteenSixteen)"/> does.
    /// </summary>
    public FixedCoordinate MovedForward(UnsignedSixteenSixteen distance) => FromRaw(unchecked(Subpixels + distance.RawValue));

    /// <summary>
    /// This position moved toward decreasing coordinates by <paramref name="distance"/>, the fraction borrowing from the
    /// whole pixel, which wraps around the coordinate range as <see cref="MovedBy(SignedSixteenSixteen)"/> does.
    /// </summary>
    public FixedCoordinate MovedBack(UnsignedSixteenSixteen distance) => FromRaw(unchecked(Subpixels - distance.RawValue));

    /// <summary>
    /// This position moved by a signed 8.8 velocity, in 256ths of a pixel: the velocity's fraction lands in the upper
    /// half of the position's fraction, so the 256ths below it are left as they were, and its carry and whole pixels move
    /// the whole pixel, which wraps around the coordinate range.
    /// </summary>
    public FixedCoordinate MovedBy(SignedEightEight velocity) => MovedBy(velocity.ToSixteenSixteenDelta());

    /// <summary>
    /// The displacement from <paramref name="from"/> to <paramref name="to"/> around the wrapping coordinate range: their
    /// difference in 65536ths of a pixel, wrapped to 32 bits, so moving <paramref name="from"/> by it lands on
    /// <paramref name="to"/>.
    /// </summary>
    public static SignedSixteenSixteen operator -(FixedCoordinate to, FixedCoordinate from)
    {
        int difference = unchecked((int)(to.Subpixels - from.Subpixels));
        return SignedSixteenSixteen.FromParts(difference >> 16, difference & ushort.MaxValue);
    }

}
