
namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Physical launch and hand-anchor definitions; flare artwork placement is separate.</summary>
internal static class GrappleFiringDefinitions
{
    /// <summary>Launch speed along an axis: twelve pixels an update times the sine of a right angle, 255/256.</summary>
    internal static readonly SignedEightEight CardinalSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(12 * 255);
    /// <summary>Each component of a diagonal launch: twelve pixels an update times the sine of an eighth turn, 181/256.</summary>
    internal static readonly SignedEightEight DiagonalSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(12 * 181);
}
