
namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Physical launch and hand-anchor definitions; flare artwork placement is separate.</summary>
internal static class GrappleFiringDefinitions
{
    /// <summary>Launch speed along an axis, in 1/256 pixels per frame: twelve pixels times the sine of a right angle, 255/256.</summary>
    internal const int CardinalVelocity = 12 * 255;
    /// <summary>Each component of a diagonal launch, in 1/256 pixels per frame: twelve pixels times the sine of an eighth turn, 181/256.</summary>
    internal const int DiagonalVelocity = 12 * 181;
}
