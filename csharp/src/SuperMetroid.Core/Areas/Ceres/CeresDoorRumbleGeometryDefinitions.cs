namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Where the elevator door's destruction rumble bursts around the door. The producer emits separate effects at these
/// places and only then selects their animation through the RNG; no trajectory, collision boundary or physical motion is
/// sampled.
/// </summary>
internal static class CeresDoorRumbleGeometryDefinitions
{
    /// <summary>
    /// The four burst places relative to the door, in the order the rumble bursts at them, starting again from the first
    /// after the last (#2424): below the door on the right, low and left of it, just below its middle, and high and left.
    /// </summary>
    internal static IReadOnlyList<Offset16<RoomPixels>> ExplosionOffsets { get; } =
        [new(2, 12), new(-2, 22), new(0, 4), new(-4, -8)];
}
