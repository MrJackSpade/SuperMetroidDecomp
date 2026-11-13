
namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Physical launch and hand-anchor definitions; flare artwork placement is separate.</summary>
internal static class GrappleFiringDefinitions
{
    /// <summary>Cardinal component: twelve times the sine table's maximum, 255.</summary>
    internal const int CardinalVelocity = 12 * 255;
    /// <summary>Diagonal component: twelve times the sine table's octant sample, 181.</summary>
    internal const int DiagonalVelocity = 12 * 181;
}
