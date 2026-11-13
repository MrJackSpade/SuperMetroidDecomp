namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing shutter's behaviours: four initialization waits and two growth directions.
/// Its values are arbitrary.
/// </summary>
public enum GrowingShutterFunction
{
    /// <summary>Checks parameter 1 before decrementing; zero starts upward growth.</summary>
    WaitToGrowUpForTimer = 1,
    /// <summary>Starts upward growth when Samus's absolute horizontal distance is strictly below parameter 1.</summary>
    WaitToGrowUpForProximity,
    /// <summary>Starts downward growth at the strict parameter-1 horizontal proximity threshold.</summary>
    WaitToGrowDownForProximity,
    /// <summary>Checks parameter 1 before decrementing; zero starts downward growth.</summary>
    WaitToGrowDownForTimer,
    /// <summary>Advances four growth sections with positive fixed-point speed, then remains at growth level 4.</summary>
    GrowDown,
    /// <summary>Subtracts fixed-point growth speed and carries a riding Samus by the whole-pixel upward delta.</summary>
    GrowUp,
}
