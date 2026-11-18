namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing shutter's behaviours: four initialization waits and two growth directions.
/// Its values are arbitrary.
/// </summary>
public enum GrowingShutterAction
{
    /// <summary>Waits out its activation wait, then grows up.</summary>
    WaitToGrowUpForTimer,
    /// <summary>Starts upward growth when Samus's absolute horizontal distance is strictly below its activation distance.</summary>
    WaitToGrowUpForProximity,
    /// <summary>Starts downward growth at its strict horizontal activation distance.</summary>
    WaitToGrowDownForProximity,
    /// <summary>Waits out its activation wait, then grows down.</summary>
    WaitToGrowDownForTimer,
    /// <summary>Grows down at its growth speed, section by section, until all four sections stand.</summary>
    GrowDown,
    /// <summary>
    /// Grows up at its growth speed, section by section, until all four sections stand, carrying a riding Samus up by the
    /// whole pixels it rises.
    /// </summary>
    GrowUp,
}
