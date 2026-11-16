namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The reach of Ridley's tail: its swing limits by facing and the directions a whip may not aim through his body.</summary>
internal static class RidleyTailReach
{
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
}
