namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The slope an enemy's aim at a point is read from (#1420, #1479): the shorter side of the offset to the point, in
/// 256ths of a pixel, over the longer side. The short side is kept to sixteen bits of 256ths, so one of 256 pixels or
/// more wraps; the long side is kept to eight bits, so one of 256 pixels or more counts only its remainder past a
/// multiple of 256; and a long side that comes to zero that way gives the steepest slope, 65535. Offsets beyond 255
/// pixels therefore bend the aimed angle exactly as they originally did.
/// </summary>
/// <remarks>
/// Consulted whenever an enemy calculates its bearing toward a point, during that enemy's update. Evidence: the original
/// angle routine (<c>CalculateAngleOf_12_14_Offset</c>, <c>$A0:C0AF</c>) chooses its octant from the full words and then
/// writes a sixteen-bit dividend and an eight-bit divisor to the hardware divider, whose quotient for a zero divisor is
/// all ones; the enemy-angle check compares 225 offsets with the angles the original CPU computed
/// (<c>csharp/test-fixtures/movement-release/enemy-angle-402.csv</c>).
/// </remarks>
[CompatibilityService("the aim-angle slope: the short side in 256ths over the long side, kept to a word and a byte, a zero long side giving the steepest slope")]
public static class AimAngleSlope
{
    /// <summary>
    /// The slope of <paramref name="shortSide"/> over <paramref name="longSide"/>, each a whole-pixel distance of at
    /// least zero: 0 through 65535 in 256ths, 65535 when the long side comes to zero.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A side is negative.</exception>
    public static int Of(int shortSide, int longSide)
    {
        int rise = unchecked((ushort)EightEight.FromWhole(Ensure.AtLeastZero(shortSide)));
        int run = unchecked((byte)Ensure.AtLeastZero(longSide));
        return run == 0 ? ushort.MaxValue : rise / run;
    }
}
