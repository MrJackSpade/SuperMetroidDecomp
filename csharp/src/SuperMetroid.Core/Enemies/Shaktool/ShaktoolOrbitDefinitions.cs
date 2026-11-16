namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Shaktool's orbit and orientation encodings, as its segments apply them.</summary>
internal static class ShaktoolOrbitDefinitions
{
    /// <summary>The turn a straightening chain's segments and target make each update: one whole step.</summary>
    internal static RotationAngle StraighteningTurn { get; } = RotationAngle.FromSteps(1);

    /// <summary>
    /// The share of the whole steps left to its target a converging segment turns by each update: a sixty-fourth, four
    /// 256ths of a step for each step left.
    /// </summary>
    internal const int ConvergenceShares = 64;

    /// <summary>
    /// The direction the centre segment's head aims for each eighth of the turn its neighbours' midpoint lies in, in turn
    /// order: each direction's eighth opens eight steps before it.
    /// </summary>
    internal static TurnArcs<CompassDirection> Orientations { get; } = new(TrigonometryAngle.Wrapped(-8),
        CompassDirection.Up, CompassDirection.UpRight, CompassDirection.Right, CompassDirection.DownRight,
        CompassDirection.Down, CompassDirection.DownLeft, CompassDirection.Left, CompassDirection.UpLeft);

    /// <summary>The curl progress installed when the centre reaches its target angle: 120 whole steps.</summary>
    internal static RotationAngle StraighteningStart { get; } = RotationAngle.FromSteps(120);

    /// <summary>Once the curl progress reaches 240 whole steps, the chain starts straightening.</summary>
    internal static RotationAngle StraighteningThreshold { get; } = RotationAngle.FromSteps(240);

    /// <summary>The fraction middle segments reset to on an endpoint swap: half a pixel.</summary>
    internal const int SwapFraction = FixedCoordinate.HalfPixelFraction;

    /// <summary>
    /// How far across a segment sits from the one before it at each step of its orbit angle, in 256ths of a pixel: the
    /// orbit's authored sine, which peaks at 3071 a quarter turn on and is not quite symmetric about its peaks. The value a
    /// quarter turn behind an angle is how far down the segment sits.
    /// </summary>
    internal static TurnArcs<int> OrbitSine { get; } = new(TrigonometryAngle.Zero,
        0, 75, 150, 225, 301, 376, 450, 525, 599, 673, 746, 819, 891, 963, 1034, 1105,
        1175, 1244, 1313, 1381, 1448, 1514, 1579, 1643, 1706, 1768, 1829, 1889, 1948, 2006, 2063, 2118,
        2172, 2224, 2276, 2326, 2374, 2421, 2467, 2511, 2554, 2595, 2634, 2672, 2709, 2743, 2777, 2808,
        2838, 2866, 2892, 2916, 2939, 2960, 2979, 2997, 3012, 3026, 3038, 3048, 3057, 3063, 3068, 3071,
        3071, 3071, 3068, 3063, 3057, 3048, 3038, 3026, 3012, 2997, 2979, 2960, 2939, 2916, 2892, 2866,
        2838, 2808, 2777, 2743, 2709, 2672, 2634, 2595, 2554, 2511, 2467, 2421, 2374, 2326, 2276, 2224,
        2172, 2118, 2063, 2006, 1948, 1889, 1829, 1768, 1706, 1643, 1579, 1514, 1448, 1381, 1313, 1244,
        1175, 1105, 1034, 963, 891, 819, 746, 673, 599, 525, 450, 376, 301, 225, 150, 75,
        0, -75, -150, -225, -301, -376, -450, -525, -599, -673, -746, -819, -891, -963, -1034, -1105,
        -1175, -1244, -1313, -1381, -1448, -1514, -1579, -1643, -1706, -1768, -1829, -1889, -1948, -2006, -2063, -2118,
        -2172, -2224, -2276, -2326, -2374, -2421, -2467, -2511, -2554, -2595, -2634, -2672, -2709, -2743, -2777, -2808,
        -2838, -2866, -2892, -2916, -2939, -2960, -2979, -2997, -3012, -3026, -3038, -3048, -3057, -3063, -3068, -3071,
        -3071, -3071, -3068, -3063, -3057, -3048, -3038, -3026, -3012, -2997, -2979, -2960, -2939, -2916, -2892, -2866,
        -2838, -2808, -2777, -2743, -2709, -2672, -2634, -2595, -2554, -2511, -2467, -2421, -2374, -2326, -2276, -2224,
        -2172, -2118, -2063, -2006, -1948, -1890, -1830, -1768, -1706, -1643, -1579, -1514, -1448, -1381, -1313, -1244,
        -1175, -1105, -1034, -963, -891, -819, -746, -673, -599, -525, -450, -376, -301, -226, -150, -75);
}
