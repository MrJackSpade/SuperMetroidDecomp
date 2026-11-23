namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far a Yapping Maw measures its distance to Samus by the two axes together (#1627, #2563): nearer than 255 pixels on
/// both axes, the distance is the sum of two short products of the axes' distances along the bearing; from 255 pixels on
/// either axis, it is that axis's distance alone, the reach past which a short product's radius no longer counts every
/// pixel. The Maw's reach follows whichever distance it measures, so its tongue reaches for a far Samus by the one axis.
/// </summary>
/// <remarks>
/// Consulted each time a Maw measures its target. Evidence: the original measures by its short multiplication only while
/// both axis distances fit its radius byte, and takes the axis's distance itself otherwise; the Maw's checks compare the
/// measured distance for targets on both sides of the reach.
/// </remarks>
public static partial class YappingMawDistanceReach
{
    /// <summary>The axis distance, in pixels, from which the Maw measures by that axis alone.</summary>
    private const int ShortProductReach = 255;

    /// <summary>Whether a target <paramref name="axisDistance"/> pixels away on one axis is measured by that axis alone.</summary>
    /// <param name="axisDistance">The target's distance on the axis, in pixels, zero or more.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="axisDistance"/> is negative.</exception>
    public static bool MeasuresByAxisAlone(int axisDistance) => Ensure.AtLeastZero(axisDistance) >= ShortProductReach;
}
