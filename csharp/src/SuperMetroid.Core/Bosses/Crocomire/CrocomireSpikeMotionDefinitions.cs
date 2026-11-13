namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The motion profile of each piece of Crocomire's falling spike wall, by its place from the top piece.</summary>
internal static class CrocomireSpikeMotionDefinitions
{
    /// <summary>The pieces the wall breaks into, the top piece first.</summary>
    internal const int PieceCount = 8;

    /// <summary>
    /// The acceleration delta, in 1/65536 pixels/frame cubed, of each piece from the top: a descending
    /// eight-step ramp of 544, saturated at 4080. The peak is deliberately
    /// capped rather than extrapolated to 4352.
    /// </summary>
    internal static readonly int[] AccelerationDeltas =
    [
        4080, 3808, 3264, 2720, 2176, 1632, 1088, 544,
    ];

    /// <summary>
    /// Each piece's acceleration limit is this many times its acceleration increment. The saturated increment keeps the
    /// largest limit at 65280, so no limit leaves the acceleration's range.
    /// </summary>
    internal const int MaximumAccelerationPerIncrement = 16;

    /// <summary>
    /// The fastest whole-pixel horizontal speed, in pixels per frame, of each piece from the top: the top four
    /// pieces descend one unit a piece from six, the bottom four one unit a pair.
    /// </summary>
    internal static readonly int[] MaximumVelocities =
    [
        6, 5, 4, 3, 2, 2, 1, 1,
    ];
}
