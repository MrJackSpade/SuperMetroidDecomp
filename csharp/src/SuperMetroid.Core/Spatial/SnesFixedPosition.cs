namespace SuperMetroid.Core.Spatial;

/// <summary>The two WRAM words produced after applying a fixed-point displacement.</summary>
/// <param name="Whole">The unsigned whole-position word, wrapping at the native sixteen-bit boundary.</param>
/// <param name="Fraction">The unsigned fractional-position word representing fractions of one pixel.</param>
[SnesEncodingOwner]
public readonly record struct SnesFixedPosition([property: IntegerQuantity("whole-pixel coordinate")] ushort Whole, ushort Fraction)
{
    /// <summary>The whole-pixel word of a native 16.16 value: its upper sixteen bits, whatever its sign.</summary>
    public static ushort WholeOf(int rawValue) => unchecked((ushort)(rawValue >> 16));

    /// <summary>Splits one native 32-bit position (whole word high, fraction word low).</summary>
    public static SnesFixedPosition FromRaw(uint rawValue) =>
        new(unchecked((ushort)(rawValue >> 16)), unchecked((ushort)rawValue));

    /// <summary>The native 32-bit position: the whole word high and the fraction word low.</summary>
    public uint RawValue => ((uint)Whole << 16) | Fraction;

    /// <summary>Replaces only the whole word, 0 through 65535, as cartridge routines that store one half do.</summary>
    public SnesFixedPosition WithWhole(int whole) => this with { Whole = checked((ushort)whole) };

    /// <summary>Replaces only the fractional word, 0 through 65535, as cartridge routines that store one half do.</summary>
    public SnesFixedPosition WithFraction(int fraction) => this with { Fraction = checked((ushort)fraction) };

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
