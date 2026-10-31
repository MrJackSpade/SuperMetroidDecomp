
namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Physical launch and hand-anchor definitions; flare artwork placement is separate.</summary>
internal static class GrappleFiringDefinitions
{
    /// <summary>$9B:C0DF cardinal component: twelve times the byte-sine maximum 255.</summary>
    internal const int CardinalVelocity = 12 * 255;
    /// <summary>$9B:C0DD diagonal component: twelve times the byte-sine octant sample 181.</summary>
    internal const int DiagonalVelocity = 12 * 181;
}
