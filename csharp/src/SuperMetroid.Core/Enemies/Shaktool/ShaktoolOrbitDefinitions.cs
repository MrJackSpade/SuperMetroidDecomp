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

    /// <summary>The subpixels in one pixel.</summary>
    private const int SubpixelsPerPixel = 65536;

    /// <summary>The fraction middle segments reset to on an endpoint swap: half a pixel.</summary>
    internal const int SwapFraction = SubpixelsPerPixel / 2;
}
