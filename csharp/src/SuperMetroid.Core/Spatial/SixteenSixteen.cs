namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Arithmetic on 16.16 fixed-point numbers (#223, #1420): sixteen whole bits above sixteen fraction bits, for the
/// motion that combines or splits velocities, speeds and positions. The <c>int</c> overloads keep two's-complement
/// sign (arithmetic shifts); the <c>uint</c> overloads are unsigned.
/// </summary>
[EncodingOwner("16.16 fixed-point number")]
public static class SixteenSixteen
{
    /// <summary>Sixteen low bits represent one pixel's fractional portion in a 16.16 value.</summary>
    private const int FractionBits = 16;
    /// <summary>Extracts the fraction without changing the signed whole half.</summary>
    private const int FractionMask = 0xffff;

    /// <summary>The raw 16.16 value with whole part <paramref name="whole"/> and no fraction.</summary>
    public static int FromWhole(int whole) => whole << FractionBits;

    /// <summary>A raw 8.8 value widened to 16.16: whole units move to the whole part, 1/256ths to the fraction's upper half.</summary>
    public static int FromEightEight(int raw) => raw << (FractionBits - 8);

    /// <summary>The whole part of a raw 16.16 value.</summary>
    public static int WholeOf(int raw) => raw >> FractionBits;

    /// <summary>The fraction of a raw 16.16 value.</summary>
    public static int FractionOf(int raw) => raw & FractionMask;
}
