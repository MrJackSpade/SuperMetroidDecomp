namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The reach of Ridley's tail: each segment's extension limits and initial angle, and its swing limits by facing.</summary>
internal static class RidleyTailReach
{
    /// <summary>The longest segment <paramref name="segmentIndex"/> may extend while whipping, base through tip ($A6:CF5A-$D09E).</summary>
    internal static ushort MaximumDistance(int segmentIndex) => Segment(RidleyTailDefinitions.MaximumDistances, segmentIndex);

    /// <summary>The tail returns to its initialized rest geometry: base, five shaft links, then tip.</summary>
    internal static ushort RestDistance(int segmentIndex) => Segment(RidleyTailDefinitions.RestDistances, segmentIndex);

    /// <summary>$A6:D38A..D397: the initial link angle of segment <paramref name="segmentIndex"/>, base through tip.</summary>
    internal static ushort InitialAngle(int segmentIndex) => Segment(RidleyTailDefinitions.InitialAngles, segmentIndex);

    /// <summary>$A6:CC12, SetRidleyTailAngleExtrema_minClockwiseAngle for left/front/right facing.</summary>
    internal static RotationAngle MinimumClockwise(RidleyFacing facing) => RidleyTailDefinitions.MinimumClockwiseAngles[facing];

    /// <summary>$A6:CC18, SetRidleyTailAngleExtrema_maxCounterClockwiseAngle for left/front/right facing.</summary>
    internal static RotationAngle MaximumCounterClockwise(RidleyFacing facing) => RidleyTailDefinitions.MaximumCounterClockwiseAngles[facing];

    /// <summary>Reads one of the seven segments' values, rejecting a segment outside the tail.</summary>
    private static ushort Segment(ushort[] table, int segmentIndex) =>
        (uint)segmentIndex < (uint)table.Length ? table[segmentIndex] : throw new ArgumentOutOfRangeException(nameof(segmentIndex));
}
