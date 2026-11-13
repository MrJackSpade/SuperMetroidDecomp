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

    /// <summary>The direction buckets a middle segment's heading is rounded to, one for each equal arc of the turn.</summary>
    internal const int OrientationBuckets = 8;

    /// <summary>The table units a midpoint's heading is advanced by before its bucket is chosen.</summary>
    internal const int OrientationBucketLead = 8;

    /// <summary>The curl progress installed when the centre reaches its target angle: 120 whole steps.</summary>
    internal static RotationAngle StraighteningStart { get; } = RotationAngle.FromSteps(120);

    /// <summary>Once the curl progress reaches 240 whole steps, the chain starts straightening.</summary>
    internal static RotationAngle StraighteningThreshold { get; } = RotationAngle.FromSteps(240);

    /// <summary>The subpixels in one pixel.</summary>
    private const int SubpixelsPerPixel = 65536;

    /// <summary>The fraction middle segments reset to on an endpoint swap: half a pixel.</summary>
    internal const int SwapFraction = SubpixelsPerPixel / 2;
}
