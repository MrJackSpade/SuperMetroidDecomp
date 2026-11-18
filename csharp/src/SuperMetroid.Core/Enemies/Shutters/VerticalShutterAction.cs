namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The behaviours the three vertical shutter definitions share.
/// Its values are arbitrary.
/// </summary>
public enum VerticalShutterAction
{
    /// <summary>Decrements the initial timer and activates on zero, restoring the configured wait value.</summary>
    WaitForTimer,
    /// <summary>Waits for Samus's strict horizontal proximity threshold.</summary>
    WaitForHorizontalProximity,
    /// <summary>Selects primary upward/downward movement and queues the activation sound when on screen.</summary>
    Activate,
    /// <summary>
    /// Stands still until a reaction starts it: a reaction-triggered shutter before its first reaction, or any shutter
    /// that has stopped at an end for good.
    /// </summary>
    Still,
    /// <summary>Moves up by its speed, carries a rider, and stops at the minimum Y boundary.</summary>
    MovingUp,
    /// <summary>Moves down by its speed, carries a rider, and stops at the maximum Y boundary.</summary>
    MovingDown,
    /// <summary>Waits out the rest at the upper end before moving down or restoring proximity waiting.</summary>
    StoppedAfterMovingUp,
    /// <summary>Waits out the rest at the lower end before moving up or restoring proximity waiting.</summary>
    StoppedAfterMovingDown,
}
