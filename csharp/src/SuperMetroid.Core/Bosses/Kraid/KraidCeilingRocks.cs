namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Where Kraid's ceiling-break rocks fall, one per rock his growth drops.</summary>
/// <remarks>
/// The game selects each rock by a cursor stepped one rock at a time from the first and stops before the cursor passes the
/// last rock, so it only ever reads whole rock entries: no partial or past-the-end read is reachable, and
/// none is reproduced (#1473).
/// </remarks>
public static class KraidCeilingRocks
{
    /// <summary>The number of ceiling rocks Kraid's growth drops.</summary>
    public static int Count => KraidCeilingRockPositionDefinitions.Xs.Length;

    /// <summary>The room X at which ceiling rock <paramref name="rock"/> falls.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rock"/> is not one of the ceiling rocks.</exception>
    public static int X(int rock) =>
        (uint)rock < (uint)Count
            ? KraidCeilingRockPositionDefinitions.Xs[rock]
            : throw new ArgumentOutOfRangeException(nameof(rock));
}
