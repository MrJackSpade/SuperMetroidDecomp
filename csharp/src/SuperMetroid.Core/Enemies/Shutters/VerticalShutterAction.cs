namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The behaviours the three vertical shutter definitions share.
/// Its values are arbitrary.
/// </summary>
public enum VerticalShutterAction
{
    /// <summary>Calls the trigger-mode table entry during this same AI update until activation replaces the action.</summary>
    Initial = 1,
    /// <summary>Decrements the initial timer and activates on zero, restoring the configured wait value.</summary>
    WaitForTimer,
    /// <summary>Waits for Samus's strict horizontal proximity threshold.</summary>
    WaitForHorizontalProximity,
    /// <summary>Selects primary upward/downward movement and queues the activation sound when on screen.</summary>
    Activate,
    /// <summary>Initial inert action for the one-shot and repeatable reaction-triggered modes.</summary>
    InitialNoOp,
    /// <summary>Adds upward 1/65536-pixel velocity, carries a rider, and stops at the minimum Y boundary.</summary>
    MovingUp,
    /// <summary>Adds downward 1/65536-pixel velocity, carries a rider, and stops at the maximum Y boundary.</summary>
    MovingDown,
    /// <summary>Waits for the timer to pass zero before moving down or restoring proximity waiting.</summary>
    StoppedAfterMovingUp,
    /// <summary>Waits for the timer to pass zero before moving up or restoring proximity waiting.</summary>
    StoppedAfterMovingDown,
    /// <summary>Inert endpoint selected by rest time 4080; eligible reaction modes can still reactivate it.</summary>
    PermanentNoOp,
}
