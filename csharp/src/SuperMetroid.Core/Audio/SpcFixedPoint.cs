using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Audio;

/// <summary>
/// The SPC sound driver's 8.8 fixed-point words (#223). Channel volume, pan, pitch (note number and fraction),
/// master volume, tempo and echo volume, and the per-tick slopes that fade them, keep a whole byte high and a
/// fraction byte low. The driver scales one value by another with <c>MUL YA</c> and keeps the product's high byte:
/// a multiplication by a fraction of 256.
/// </summary>
[SnesEncodingOwner]
internal static class SpcFixedPoint
{
    /// <summary>Fraction bits below the whole byte.</summary>
    private const int FractionBits = 8;

    /// <summary>One whole unit, the fraction's denominator.</summary>
    private const int One = 1 << FractionBits;

    /// <summary>The driver's nine-bit sign: <c>SBC</c> leaves the borrow in bit 8 of a whole-byte difference.</summary>
    private const int NineBitSign = 0x100;

    /// <summary>The whole byte of an 8.8 word: a volume, pan position, note number or tempo.</summary>
    public static byte Whole(ushort value) => unchecked((byte)(SnesLittleEndian.HighByte(value)));

    /// <summary>The fraction byte of an 8.8 word.</summary>
    public static byte Fraction(ushort value) => unchecked((byte)value);

    /// <summary>The 8.8 word with whole part <paramref name="whole"/> and no fraction.</summary>
    public static ushort FromWhole(int whole) => unchecked((ushort)(SnesEightEight.FromWhole(whole)));

    /// <summary>The 8.8 word with whole part <paramref name="whole"/> and fraction <paramref name="fraction"/>.</summary>
    public static ushort Compose(int whole, byte fraction) => unchecked((ushort)((SnesEightEight.FromWhole(whole)) | fraction));

    /// <summary>Replaces only the whole byte, as the driver's single-byte stores to a word's high half do.</summary>
    public static ushort WithWhole(ushort value, byte whole) => Compose(whole, Fraction(value));

    /// <summary>
    /// <paramref name="value"/> scaled by <paramref name="fraction"/>/256: the whole part of their product, as
    /// <c>MUL YA</c> followed by keeping Y does for byte operands.
    /// </summary>
    public static int Scale(int value, int fraction) => value * fraction >> FractionBits;

    /// <summary>
    /// The driver's per-tick 8.8 slope from a signed nine-bit whole-byte distance over <paramref name="ticks"/>:
    /// the magnitude's quotient becomes the whole byte and the remainder's fraction the low byte. A zero tick count
    /// saturates both bytes, as the native divide does.
    /// </summary>
    public static ushort DivideSigned(int distance, byte ticks)
    {
        bool negative = (distance & NineBitSign) != 0;
        int magnitude = negative ? -distance : distance;
        int quotient = ticks != 0 ? Fraction(unchecked((ushort)magnitude)) / ticks : byte.MaxValue;
        int remainder = ticks != 0 ? Fraction(unchecked((ushort)magnitude)) % ticks : Fraction(unchecked((ushort)magnitude));
        int fraction = ticks != 0 ? Fraction(unchecked((ushort)(remainder * One / ticks))) : byte.MaxValue;
        int slope = quotient * One + fraction;
        return unchecked((ushort)(negative ? -slope : slope));
    }
}
