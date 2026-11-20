using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Reads of the X-ray revealed-block shapes' definitions (<see cref="XrayRevealShapeDefinitions"/>).</summary>
internal static class XrayRevealShapes
{
    /// <summary>Whether <paramref name="shape"/> copies blocks of its own, rather than revealing as an extended block does.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> is undefined.</exception>
    internal static bool CopiesBlocks(XrayRevealShape shape) => Ensure.Listed(XrayRevealShapeDefinitions.Shapes, shape).CopiedBlocks > 0;

    /// <summary>Whether <paramref name="shape"/> reveals in a room of <paramref name="area"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> is undefined.</exception>
    internal static bool RevealsIn(XrayRevealShape shape, AreaId area) =>
        Ensure.Listed(XrayRevealShapeDefinitions.Shapes, shape).OnlyIn is not { } only || only == area;
}
