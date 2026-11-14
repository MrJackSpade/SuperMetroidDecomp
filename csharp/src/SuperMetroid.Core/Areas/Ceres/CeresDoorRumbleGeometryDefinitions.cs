namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Where the elevator door's destruction rumble bursts around the door. The producer emits separate effects at these
/// places and only then selects their animation through the RNG; no trajectory, collision boundary or physical motion is
/// sampled.
/// </summary>
internal static class CeresDoorRumbleGeometryDefinitions
{
    /// <summary>
    /// The four burst places relative to the door, in list order; the rumble bursts at them from the last to the first and
    /// then starts again from the last: high and left, just below the door's middle, low and left of it, and below it on
    /// the right.
    /// </summary>
    internal static IReadOnlyList<Offset16<RoomPixels>> ExplosionOffsets { get; } =
        [new(-4, -8), new(0, 4), new(-2, 22), new(2, 12)];
}
