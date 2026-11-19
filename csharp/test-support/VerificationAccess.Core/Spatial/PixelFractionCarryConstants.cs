namespace SuperMetroid.Core.Spatial;

/// <summary>Cartridge values and helpers of <see cref="PixelFractionCarry"/> that only verification reads.</summary>
internal static class PixelFractionCarryConstants
{
    /// <summary>The carry of <paramref name="part"/>, a part of a pixel from none up to, but not including, one pixel.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The part is negative or reaches a whole pixel.</exception>
    public static PixelFractionCarry Of(SignedEightEight part) =>
        part.IsNegative || part.WholePixels != 0
            ? throw new ArgumentOutOfRangeException(nameof(part), part, "A carried part of a pixel lies from none up to one pixel.")
            : new(part);
}
