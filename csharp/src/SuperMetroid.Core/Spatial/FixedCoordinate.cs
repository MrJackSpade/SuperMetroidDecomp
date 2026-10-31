namespace SuperMetroid.Core.Spatial;

/// <summary>One axis of a 16.16 fixed-point position: the whole pixel and its fraction, each wrapping at sixteen bits.</summary>
[EncodingOwner("16.16 fixed-point coordinate")]
public readonly record struct FixedCoordinate
{
    /// <summary>The position with a whole word and a fraction word that must each be 0 through 65535.</summary>
    /// <param name="whole">The unsigned whole-position word, wrapping at the native sixteen-bit boundary.</param>
    /// <param name="fraction">The unsigned fractional-position word representing fractions of one pixel.</param>
    public FixedCoordinate(int whole, int fraction)
    {
        Whole = checked((ushort)whole);
        Fraction = checked((ushort)fraction);
    }

    /// <summary>The unsigned whole-position word, wrapping at the native sixteen-bit boundary.</summary>
    [IntegerQuantity("whole-pixel coordinate")]
    public ushort Whole { get; init; }

    /// <summary>The unsigned fractional-position word representing fractions of one pixel.</summary>
    public ushort Fraction { get; init; }

    /// <summary>The position with a whole word and a fraction word that must each be 0 through 65535.</summary>
    public static FixedCoordinate Of(int whole, int fraction) => new(whole, fraction);

    /// <summary>The whole-pixel word of a native 16.16 value: its upper sixteen bits, whatever its sign.</summary>
    public static ushort WholeOf(int rawValue) => unchecked((ushort)(rawValue >> 16));

    /// <summary>Splits one native 32-bit position (whole word high, fraction word low).</summary>
    public static FixedCoordinate FromRaw(uint rawValue) =>
        new(unchecked((ushort)(rawValue >> 16)), unchecked((ushort)rawValue));

    /// <summary>The native 32-bit position: the whole word high and the fraction word low.</summary>
    public uint RawValue => ((uint)Whole << 16) | Fraction;

    /// <summary>Replaces only the whole word, 0 through 65535, as cartridge routines that store one half do.</summary>
    public FixedCoordinate WithWhole(int whole) => this with { Whole = checked((ushort)whole) };

    /// <summary>
    /// Replaces only the whole word with a computed coordinate, which wraps at sixteen bits
    /// (<see cref="CoordinateAxis.Wrap"/>), as cartridge routines that store one half of a moved position do.
    /// </summary>
    public FixedCoordinate WithWrappedWhole(int whole) => WithWhole(CoordinateAxis.Wrap(whole));

    /// <summary>Replaces only the fractional word, 0 through 65535, as cartridge routines that store one half do.</summary>
    public FixedCoordinate WithFraction(int fraction) => this with { Fraction = checked((ushort)fraction) };

    /// <summary>
    /// Adds a signed 8.8 velocity the way the cartridge's split-byte movers do: with 8-bit A, the velocity's fraction
    /// byte adds into the fraction word's high byte only, leaving its low byte untouched, and the byte carry plus the
    /// sign-extended whole byte advance the whole word. A 16.16 addition would place the fraction 256 times too low.
    /// </summary>
    /// <param name="velocity">The signed 8.8 velocity's raw word, 0 through 65535.</param>
    public FixedCoordinate AddEightEightToHighFraction(int velocity)
    {
        ushort word = checked((ushort)velocity);
        int fractionSum = (Fraction >> 8) + unchecked((byte)word);
        return new(
            unchecked((ushort)(Whole + unchecked((sbyte)(word >> 8)) + (fractionSum >> 8))),
            unchecked((ushort)((Fraction & 0xff) | (fractionSum & 0xff) << 8)));
    }
}
