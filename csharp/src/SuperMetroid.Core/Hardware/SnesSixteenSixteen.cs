namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The native 16.16 layout as raw arithmetic (#223): a whole word in the upper sixteen bits and a fraction word in
/// the lower sixteen, for routines that combine or split velocities, speeds and positions in registers. The
/// <c>int</c> overloads keep two's-complement sign (arithmetic shifts); the <c>uint</c> overloads are unsigned.
/// </summary>
[SnesEncodingOwner]
public static class SnesSixteenSixteen
{
    /// <summary>Sixteen low bits represent one pixel's fractional portion in a native 16.16 word.</summary>
    private const int FractionBits = 16;
    /// <summary>Extracts the low fractional word without changing the signed whole half.</summary>
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
