namespace SuperMetroid.Core.Hardware;

/// <summary>A signed native 8.8 velocity word, retained losslessly in cartridge form.</summary>
/// <param name="RawValue">The unchanged two's-complement word containing eight whole and eight fractional bits.</param>
[SnesEncodingOwner]
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

/// <summary>
/// The unsigned native 8.8 layout (#223): a word whose high byte is whole units and low byte is 1/256ths, as
/// radii, speeds and accelerations the cartridge advances by a fraction each frame keep it.
/// </summary>
[SnesEncodingOwner]
public static class SnesUnsignedEightEight
{
    /// <summary>The whole units of an unsigned 8.8 word.</summary>
    public static byte Whole(ushort value) => unchecked((byte)(value >> 8));
}

/// <summary>
/// The native 16.16 layout as raw arithmetic (#223): a whole word in the upper sixteen bits and a fraction word in
/// the lower sixteen, for routines that combine or split velocities, speeds and positions in registers. The
/// <c>int</c> overloads keep two's-complement sign (arithmetic shifts); the <c>uint</c> overloads are unsigned.
/// </summary>
[SnesEncodingOwner]
public static class SnesSixteenSixteen
{
    private const int FractionBits = 16;
    private const int FractionMask = 0xffff;

    /// <summary>The raw 16.16 value with whole part <paramref name="whole"/> and no fraction.</summary>
    public static int FromWhole(int whole) => whole << FractionBits;

    /// <inheritdoc cref="FromWhole(int)"/>
    public static uint FromWhole(uint whole) => whole << FractionBits;

    /// <summary>A raw 8.8 value widened to 16.16: whole units move to the whole word, 1/256ths to the fraction's high byte.</summary>
    public static int FromEightEight(int raw) => raw << (FractionBits - 8);

    /// <inheritdoc cref="FromEightEight(int)"/>
    public static uint FromEightEight(uint raw) => raw << (FractionBits - 8);

    /// <summary>The raw 16.16 value of a whole word and a fraction word.</summary>
    public static uint Compose(ushort whole, ushort fraction) => (uint)whole << FractionBits | fraction;

    /// <summary><paramref name="value"/> times a 16.16 factor, truncated to whole units (arithmetic shift).</summary>
    public static int Multiply(int value, int factor) => value * factor >> FractionBits;

    /// <summary>The whole part of a raw 16.16 value.</summary>
    public static int WholeOf(int raw) => raw >> FractionBits;

    /// <inheritdoc cref="WholeOf(int)"/>
    public static uint WholeOf(uint raw) => raw >> FractionBits;

    /// <summary>The fraction word of a raw 16.16 value.</summary>
    public static int FractionOf(int raw) => raw & FractionMask;

    /// <inheritdoc cref="FractionOf(int)"/>
    public static uint FractionOf(uint raw) => raw & FractionMask;
}

/// <summary>
/// The native 8.8 layout as raw arithmetic (#223): whole units in the high byte and 1/256ths in the low byte, as
/// 8.8 velocities, speeds and displacements keep them. The <c>int</c> overloads keep two's-complement sign.
/// </summary>
[SnesEncodingOwner]
public static class SnesEightEight
{
    private const int FractionBits = 8;
    private const int FractionMask = 0xff;

    /// <summary>The raw 8.8 value with whole part <paramref name="whole"/> and no fraction.</summary>
    public static int FromWhole(int whole) => whole << FractionBits;

    /// <summary>
    /// <paramref name="value"/> times an 8.8 factor, truncated to whole units: the native multiply that keeps the
    /// product's high byte, such as a sine sample or volume scaling a magnitude.
    /// </summary>
    public static int Multiply(int value, int factor) => value * factor >> FractionBits;

    /// <summary>The whole part of a raw 8.8 value.</summary>
    public static int WholeOf(int raw) => raw >> FractionBits;

    /// <inheritdoc cref="WholeOf(int)"/>
    public static uint WholeOf(uint raw) => raw >> FractionBits;

    /// <summary>The fraction byte of a raw 8.8 value.</summary>
    public static int FractionOf(int raw) => raw & FractionMask;

    /// <summary>
    /// The 8.8 value of a 16.16 whole and fraction pair, truncating the fraction to its high byte: the unaligned word
    /// a routine reads across the fraction's high byte and the whole's low byte.
    /// </summary>
    public static int FromSixteenSixteen(int whole, int fraction) => (whole << FractionBits) | (fraction >> FractionBits);
}

/// <summary>A signed native 16.16 displacement with explicit whole and fractional halves.</summary>
[SnesEncodingOwner]
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
[SnesEncodingOwner]
public readonly record struct SnesFixedPosition(ushort Whole, ushort Fraction)
{
    /// <summary>The whole-pixel word of a native 16.16 value: its upper sixteen bits, whatever its sign.</summary>
    public static ushort WholeOf(int rawValue) => unchecked((ushort)(rawValue >> 16));

    /// <summary>Splits one native 32-bit position (whole word high, fraction word low).</summary>
    public static SnesFixedPosition FromRaw(uint rawValue) =>
        new(unchecked((ushort)(rawValue >> 16)), unchecked((ushort)rawValue));

    /// <summary>The native 32-bit position: the whole word high and the fraction word low.</summary>
    public uint RawValue => ((uint)Whole << 16) | Fraction;

    /// <summary>Replaces only the whole word, as cartridge routines that store one half do.</summary>
    public SnesFixedPosition WithWhole(ushort whole) => this with { Whole = whole };

    /// <summary>Replaces only the fractional word, as cartridge routines that store one half do.</summary>
    public SnesFixedPosition WithFraction(ushort fraction) => this with { Fraction = fraction };

    /// <summary>
    /// Adds a signed 8.8 velocity the way the cartridge's split-byte movers do: with 8-bit A, the velocity's fraction
    /// byte adds into the fraction word's high byte only, leaving its low byte untouched, and the byte carry plus the
    /// sign-extended whole byte advance the whole word. A 16.16 addition would place the fraction 256 times too low.
    /// </summary>
    public SnesFixedPosition AddEightEightToHighFraction(ushort velocity)
    {
        int fractionSum = (Fraction >> 8) + unchecked((byte)velocity);
        return new(
            unchecked((ushort)(Whole + unchecked((sbyte)(velocity >> 8)) + (fractionSum >> 8))),
            unchecked((ushort)((Fraction & 0xff) | (fractionSum & 0xff) << 8)));
    }
}
