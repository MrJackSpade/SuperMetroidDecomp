namespace SuperMetroid.Core.Spatial;

/// <summary>A signed native 16.16 displacement with explicit whole and fractional halves.</summary>
public readonly record struct SnesSignedSixteenSixteen
{
    /// <summary>Wraps an existing signed 32-bit displacement without altering its whole and fraction bits.</summary>
    private SnesSignedSixteenSixteen(int rawValue) => RawValue = rawValue;

    /// <summary>Gets the signed native word containing sixteen whole and sixteen fractional bits.</summary>
    public int RawValue { get; }

    /// <summary>Preserves an existing signed 32-bit native fixed-point value.</summary>
    public static SnesSignedSixteenSixteen FromRaw(int rawValue) => new(rawValue);

    /// <summary>Combines the native signed whole word and unsigned fractional word.</summary>
    public static SnesSignedSixteenSixteen FromParts(short wholePixels, ushort fraction) =>
        new(unchecked((wholePixels << 16) | fraction));

    /// <summary>The zero displacement.</summary>
    public static SnesSignedSixteenSixteen Zero => default;

    /// <summary>The whole-pixel word as its unsigned native bit pattern.</summary>
    public ushort WholeWord => unchecked((ushort)(RawValue >> 16));

    /// <summary>The signed whole-pixel word.</summary>
    public short WholePixels => unchecked((short)(RawValue >> 16));

    /// <summary>The unsigned fractional word.</summary>
    public ushort Fraction => unchecked((ushort)RawValue);

    /// <summary>Replaces only the whole word, as cartridge routines that store one half do.</summary>
    public SnesSignedSixteenSixteen WithWholePixels(short wholePixels) => FromParts(wholePixels, Fraction);

    /// <summary>Replaces only the fractional word, as cartridge routines that store one half do.</summary>
    public SnesSignedSixteenSixteen WithFraction(ushort fraction) => FromParts(WholePixels, fraction);

    /// <summary>Adds two displacements, carrying the fraction and wrapping at 32 bits.</summary>
    public static SnesSignedSixteenSixteen operator +(SnesSignedSixteenSixteen left, SnesSignedSixteenSixteen right) =>
        new(unchecked(left.RawValue + right.RawValue));

    /// <summary>Subtracts two displacements, borrowing from the whole word and wrapping at 32 bits.</summary>
    public static SnesSignedSixteenSixteen operator -(SnesSignedSixteenSixteen left, SnesSignedSixteenSixteen right) =>
        new(unchecked(left.RawValue - right.RawValue));

    /// <summary>Adds this delta to an unsigned wrapped 16.16 world-position pair.</summary>
    public SnesFixedPosition AddTo(ushort wholePosition, ushort subposition)
    {
        uint position = ((uint)wholePosition << 16) | subposition;
        position = unchecked(position + (uint)RawValue);
        return new SnesFixedPosition(
            unchecked((ushort)(position >> 16)),
            unchecked((ushort)position));
    }
}
