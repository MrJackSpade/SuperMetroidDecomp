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
    internal const int FractionBits = 16;

    /// <summary>A raw 8.8 value widened to 16.16: whole units move to the whole part, 1/256ths to the fraction's upper half.</summary>
    public static int FromEightEight(int raw) => raw << (FractionBits - 8);
}
