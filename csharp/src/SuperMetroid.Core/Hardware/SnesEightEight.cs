namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The native 8.8 layout as raw arithmetic (#223): whole units in the high byte and 1/256ths in the low byte, as
/// 8.8 velocities, speeds and displacements keep them. The <c>int</c> overloads keep two's-complement sign.
/// </summary>
[SnesEncodingOwner]
public static class SnesEightEight
{
    /// <summary>Eight low bits represent one unit's fractional portion in a native 8.8 word.</summary>
    private const int FractionBits = 8;
    /// <summary>Extracts the fractional byte from a raw 8.8 value.</summary>
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
