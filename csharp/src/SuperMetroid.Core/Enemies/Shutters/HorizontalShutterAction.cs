namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The behaviours of the unused horizontal shutter.
/// Its values are arbitrary.
/// </summary>
public enum HorizontalShutterAction
{
    /// <summary>Installs the trigger-mode table entry for the following AI update.</summary>
    Initial = 1,
    /// <summary>Decrements the initial timer and activates when it becomes zero.</summary>
    WaitForTimer,
    /// <summary>Waits for Samus's strict horizontal proximity threshold.</summary>
    WaitForHorizontalProximity,
    /// <summary>Selects left or right movement from the primary direction.</summary>
    Activate,
    /// <summary>Holds still until an eligible one-shot or repeatable reaction.</summary>
    InitialNoOp,
    /// <summary>Adds leftward 16.16 velocity, displaces overlapping Samus on the leading side, and stops at minimum X.</summary>
    MovingLeft,
    /// <summary>Adds rightward 16.16 velocity, displaces overlapping Samus on the leading side, and stops at maximum X.</summary>
    MovingRight,
    /// <summary>Waits for signed timer underflow before moving right or restoring proximity waiting.</summary>
    StoppedAfterMovingLeft,
    /// <summary>Waits for signed timer underflow before moving left or restoring proximity waiting.</summary>
    StoppedAfterMovingRight,
    /// <summary>Inert endpoint for rest time 4080, retaining stationary manual ejection and eligible reaction activation.</summary>
    PermanentNoOp,
}
