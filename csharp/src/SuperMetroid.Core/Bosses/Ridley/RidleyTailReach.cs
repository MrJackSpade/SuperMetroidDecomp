namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The reach of Ridley's tail: each segment's extension limits and initial angle, and its swing limits by facing.</summary>
internal static class RidleyTailReach
{
    /// <summary>The longest segment <paramref name="segmentIndex"/> may extend while whipping, base through tip.</summary>
    internal static SignedEightEight MaximumDistance(int segmentIndex) => Segment(RidleyTailDefinitions.MaximumDistances, segmentIndex);

    /// <summary>The tail returns to its initialized rest geometry: base, five shaft links, then tip.</summary>
    internal static SignedEightEight RestDistance(int segmentIndex) => Segment(RidleyTailDefinitions.RestDistances, segmentIndex);

    /// <summary>The initial link angle of segment <paramref name="segmentIndex"/>, base through tip.</summary>
    internal static RidleyTailAngle InitialAngle(int segmentIndex) => Segment(RidleyTailDefinitions.InitialAngles, segmentIndex);

    /// <summary>The tail's minimum clockwise angle for left/front/right facing.</summary>
    internal static RidleyTailAngle MinimumClockwise(RidleyFacing facing) => RidleyTailDefinitions.MinimumClockwiseAngles[facing];

    /// <summary>The tail's maximum counter-clockwise angle for left/front/right facing.</summary>
    internal static RidleyTailAngle MaximumCounterClockwise(RidleyFacing facing) => RidleyTailDefinitions.MaximumCounterClockwiseAngles[facing];

    /// <summary>
    /// Whether a whip aimed in <paramref name="direction"/> would pass through Ridley's body: the forward interval from
    /// <see cref="RidleyTailDefinitions.ForbiddenForwardStart"/> up to (not including) <see cref="RidleyTailDefinitions.ForbiddenForwardEnd"/>.
    /// </summary>
    internal static bool AimsThroughBody(TrigonometryAngle direction) =>
        direction >= RidleyTailDefinitions.ForbiddenForwardStart && direction < RidleyTailDefinitions.ForbiddenForwardEnd;

    /// <summary>Reads one of the seven segments' values, rejecting a segment outside the tail.</summary>
    private static T Segment<T>(T[] table, int segmentIndex) =>
        (uint)segmentIndex < (uint)table.Length ? table[segmentIndex] : throw new ArgumentOutOfRangeException(nameof(segmentIndex));
}
