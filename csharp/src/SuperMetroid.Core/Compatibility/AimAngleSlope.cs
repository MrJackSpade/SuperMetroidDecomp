namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The slope an enemy's aim at a point is read from (#1420, #1479): the shorter side of the offset to the point, in
/// 256ths of a pixel, over the longer side in pixels. The short side counts only its 256ths past a multiple of 65536, so
/// one of 256 pixels or more starts again from nothing; the long side counts only its pixels past a multiple of 256;
/// and a long side that comes to nothing that way gives the steepest slope, 65535. Offsets beyond 255 pixels therefore
/// bend the aimed angle exactly as they always have.
/// </summary>
/// <remarks>
/// Consulted whenever an enemy calculates its bearing toward a point, during that enemy's update. Evidence: the original
/// angle routine (<c>CalculateAngleOf_12_14_Offset</c>, <c>$A0:C0AF</c>) chooses its octant from the full words and then
/// writes a sixteen-bit dividend and an eight-bit divisor to the hardware divider, whose quotient for a zero divisor is
/// all ones; the enemy-angle check compares 225 offsets with the angles the original CPU computed
/// (<c>csharp/test-fixtures/movement-release/enemy-angle-402.csv</c>).
/// </remarks>
public static partial class AimAngleSlope
{
    /// <summary>The 256ths in each pixel of the short side.</summary>
    private const int RisePerPixel = 256;

    /// <summary>The 256ths of the short side past whose multiple it counts.</summary>
    private const int RiseSpan = 65536;

    /// <summary>The pixels of the long side past whose multiple it counts.</summary>
    private const int RunSpan = 256;

    /// <summary>The steepest slope: a long side that comes to nothing.</summary>
    private const int Steepest = RiseSpan - 1;

    /// <summary>
    /// The slope of <paramref name="shortSide"/> over <paramref name="longSide"/>, each a whole-pixel distance of at
    /// least zero: 0 through 65535 in 256ths, 65535 when the long side comes to zero.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A side is negative.</exception>
    internal static int Of(int shortSide, int longSide)
    {
        int rise = unchecked(Ensure.AtLeastZero(shortSide) * RisePerPixel) % RiseSpan;
        int run = Ensure.AtLeastZero(longSide) % RunSpan;
        return run == 0 ? Steepest : rise / run;
    }

    /// <summary>The 256ths of slope in each step the aim turns: a slope of one turns an eighth of a turn.</summary>
    private const int SlopePerStep = RisePerPixel / (RotationAngle.StepsPerTurn / 8);

    /// <summary>
    /// The whole steps the aim at an offset turns from the long side's direction toward the short side's (#2453): the
    /// slope (<see cref="Of"/>) in steps, a slope of one turning an eighth of a turn and any part of a step dropped. The
    /// steepest slope turns far past the octant, as it always has.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A side is negative.</exception>
    public static int TurnSteps(int shortSide, int longSide) => Of(shortSide, longSide) / SlopePerStep;
}
