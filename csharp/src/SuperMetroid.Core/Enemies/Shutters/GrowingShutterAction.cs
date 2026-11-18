namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The growing shutter's behaviours: its two waits and its growth, each the way it grows (#2512), which the shutter keeps
/// (<see cref="GrowingShutterEnemyState.Growth"/>). Its values are arbitrary.
/// </summary>
public enum GrowingShutterAction
{
    /// <summary>Waits out its activation wait, then grows.</summary>
    WaitForTimer,
    /// <summary>Starts growing when Samus's absolute horizontal distance is strictly below its activation distance.</summary>
    WaitForProximity,
    /// <summary>
    /// Grows at its growth speed, section by section, until all four sections stand; growing up, it carries a riding Samus
    /// up by the whole pixels it rises.
    /// </summary>
    Grow,
}
