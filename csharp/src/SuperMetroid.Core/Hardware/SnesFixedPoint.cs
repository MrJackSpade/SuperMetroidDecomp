namespace SuperMetroid.Core.Hardware;

/// <summary>A signed native 8.8 velocity word, retained losslessly in cartridge form.</summary>
/// <param name="RawValue">The unchanged two's-complement word containing eight whole and eight fractional bits.</param>
public readonly record struct SnesSignedEightEight(ushort RawValue)
{
    /// <summary>The raw word read as its signed two's-complement value.</summary>
    public short SignedRawValue => unchecked((short)RawValue);

    /// <summary>Creates a value from a signed raw 8.8 word without changing its bits.</summary>
    public static SnesSignedEightEight FromSignedRaw(short rawValue) =>
        new(unchecked((ushort)rawValue));

    /// <summary>Promotes the signed word to the 16.16 delta used by native position adders.</summary>
    public SnesSignedSixteenSixteen ToSixteenSixteenDelta() =>
        SnesSignedSixteenSixteen.FromRaw(unchecked((short)RawValue) << 8);
}

/// <summary>A signed native 16.16 displacement with explicit whole and fractional halves.</summary>
public readonly record struct SnesSignedSixteenSixteen
{
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

/// <summary>The two WRAM words produced after applying a fixed-point displacement.</summary>
/// <param name="Whole">The unsigned whole-position word, wrapping at the native sixteen-bit boundary.</param>
/// <param name="Fraction">The unsigned fractional-position word representing fractions of one pixel.</param>
public readonly record struct SnesFixedPosition(ushort Whole, ushort Fraction)
{
    /// <summary>Splits one native 32-bit position (whole word high, fraction word low).</summary>
    public static SnesFixedPosition FromRaw(uint rawValue) =>
        new(unchecked((ushort)(rawValue >> 16)), unchecked((ushort)rawValue));

    /// <summary>The native 32-bit position: the whole word high and the fraction word low.</summary>
    public uint RawValue => ((uint)Whole << 16) | Fraction;

    /// <summary>Replaces only the whole word, as cartridge routines that store one half do.</summary>
    public SnesFixedPosition WithWhole(ushort whole) => this with { Whole = whole };

    /// <summary>Replaces only the fractional word, as cartridge routines that store one half do.</summary>
    public SnesFixedPosition WithFraction(ushort fraction) => this with { Fraction = fraction };
}
