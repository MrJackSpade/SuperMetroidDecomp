using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Reads of the X-ray revealed-block shapes' definitions (<see cref="XrayRevealShapeDefinitions"/>).</summary>
internal static class XrayRevealShapes
{

    /// <summary>Whether <paramref name="shape"/> reveals in a room of <paramref name="area"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> is undefined.</exception>
    internal static bool RevealsIn(XrayRevealShape shape, AreaId area) =>
        XrayRevealShapeDefinitions.Shapes[shape].OnlyIn is not { } only || only == area;
}
