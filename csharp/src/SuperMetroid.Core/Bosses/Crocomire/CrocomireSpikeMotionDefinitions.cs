namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Physical-slot motion profiles for Crocomire's falling spike wall.</summary>
internal static class CrocomireSpikeMotionDefinitions
{
    /// <summary>
    /// Native $86:91C3 accelerationDelta, in 1/65536 pixels/frame cubed, by physical slot.
    /// Slots 0 and 1 are stationary; slots 2..9 and 10..17 repeat a descending
    /// eight-step ramp of 544, saturated at 4080. The peak is deliberately
    /// capped rather than extrapolated to 4352. All eighteen words agree with
    /// the pinned NTSC J/U v1.0 definition.
    /// </summary>
    internal static readonly ushort[] AccelerationDeltas =
    [
        0, 0,
        4080, 3808, 3264, 2720, 2176, 1632, 1088, 544,
        4080, 3808, 3264, 2720, 2176, 1632, 1088, 544,
    ];

    /// <summary>
    /// Each of the eighteen acceleration limits is sixteen times its acceleration increment: the increment shifted
    /// left this far. The saturated increment keeps the largest limit at 65280 without wrap.
    /// </summary>
    internal const int MaximumAccelerationShift = 4;

    /// <summary>
    /// Native $86:920B maxVelocity low-byte view, by physical slot 0..17.
    /// Slots 0..1 are stationary. Each eight-slot group descends one unit per
    /// pair; the second group's first four slots use the larger single-slot
    /// ramp 6,5,4,3. Native $86:913F/$9144 reads only the low byte; no high-byte view is used.
    /// </summary>
    internal static readonly byte[] MaximumVelocities =
    [
        0, 0,
        4, 4, 3, 3, 2, 2, 1, 1,
        6, 5, 4, 3, 2, 2, 1, 1,
    ];
}
