namespace SuperMetroid.Core.Spatial;

/// <summary>A signed 16.16 fixed-point displacement with explicit whole and fractional halves.</summary>
[EncodingOwner("signed 16.16 fixed-point number")]
public readonly record struct SignedSixteenSixteen
{
    /// <summary>Wraps an existing signed 32-bit displacement without altering its whole and fraction bits.</summary>
    private SignedSixteenSixteen(int rawValue) => RawValue = rawValue;

    /// <summary>Gets the signed native word containing sixteen whole and sixteen fractional bits.</summary>
    public int RawValue { get; }

    /// <summary>Preserves an existing signed 32-bit native fixed-point value.</summary>
    public static SignedSixteenSixteen FromRaw(int rawValue) => new(rawValue);

    /// <summary>
    /// Combines the signed whole pixels, -32768 through 32767, and the unsigned fractional word, 0 through 65535.
    /// </summary>
    public static SignedSixteenSixteen FromParts(int wholePixels, int fraction) =>
        new(unchecked((Ensure.BetweenInclusive(short.MinValue, short.MaxValue, wholePixels) << 16) |
            Ensure.BetweenInclusive(0, ushort.MaxValue, fraction)));

    /// <summary>A displacement of <paramref name="sixteenths"/> sixteenths of a pixel.</summary>
    public static SignedSixteenSixteen FromSixteenthsOfPixel(int sixteenths) => new(checked(sixteenths * 4096));

    /// <summary>The zero displacement.</summary>
    public static SignedSixteenSixteen Zero => default;

    /// <summary>The whole-pixel word as its unsigned native bit pattern.</summary>
    public ushort WholeWord => unchecked((ushort)(RawValue >> 16));

    /// <summary>The signed whole-pixel word.</summary>
    [IntegerQuantity("whole-pixel part of a signed distance")]
    public short WholePixels => unchecked((short)(RawValue >> 16));

    /// <summary>The unsigned fractional word.</summary>
    public ushort Fraction => unchecked((ushort)RawValue);

    /// <summary>Replaces only the whole word, as cartridge routines that store one half do.</summary>
    public SignedSixteenSixteen WithWholePixels(int wholePixels) => FromParts(wholePixels, Fraction);

    /// <summary>Replaces only the fractional word, as cartridge routines that store one half do.</summary>
    public SignedSixteenSixteen WithFraction(int fraction) => FromParts(WholePixels, fraction);

    /// <summary>Adds two displacements, carrying the fraction and wrapping at 32 bits.</summary>
    public static SignedSixteenSixteen operator +(SignedSixteenSixteen left, SignedSixteenSixteen right) =>
        new(unchecked(left.RawValue + right.RawValue));

    /// <summary>Subtracts two displacements, borrowing from the whole word and wrapping at 32 bits.</summary>
    public static SignedSixteenSixteen operator -(SignedSixteenSixteen left, SignedSixteenSixteen right) =>
        new(unchecked(left.RawValue - right.RawValue));

    /// <summary>Adds this delta to an unsigned wrapped 16.16 world-position pair, each word 0 through 65535.</summary>
    public FixedCoordinate AddTo(int wholePosition, int subposition)
    {
        uint position = ((uint)checked((ushort)wholePosition) << 16) | checked((ushort)subposition);
        position = unchecked(position + (uint)RawValue);
        return new FixedCoordinate(
            unchecked((ushort)(position >> 16)),
            unchecked((ushort)position));
    }
}
