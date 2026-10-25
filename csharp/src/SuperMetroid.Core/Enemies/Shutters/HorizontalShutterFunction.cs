namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The routines of the unused horizontal shutter.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum HorizontalShutterFunction : ushort
{
    /// <summary><c>$A2:F224 Function_HorizontalShutter_Initial</c>: installs the trigger-mode table entry for the following AI update.</summary>
    Initial = 1,
    /// <summary><c>$A2:F230 Function_HorizontalShutter_Initial_WaitForTimer</c>: decrements the initial timer and activates when it becomes zero.</summary>
    WaitForTimer,
    /// <summary><c>$A2:F243 Function_HorizontalShutter_Initial_WaitForSamusToGetNear</c>: waits for Samus's strict horizontal proximity threshold.</summary>
    WaitForHorizontalProximity,
    /// <summary><c>$A2:F254 Function_HorizontalShutter_Initial_Activate</c>: selects left or right movement from the primary direction.</summary>
    Activate,
    /// <summary><c>$A2:F25B Function_HorizontalShutter_Initial_Nothing</c>: holds still until an eligible one-shot or repeatable reaction.</summary>
    InitialNoOp,
    /// <summary><c>$A2:F272 Function_HorizontalShutter_MovingLeft</c>: adds leftward 16.16 velocity, displaces overlapping Samus on the leading side, and stops at minimum X.</summary>
    MovingLeft,
    /// <summary><c>$A2:F2E4 Function_HorizontalShutter_MovingRight</c>: adds rightward 16.16 velocity, displaces overlapping Samus on the leading side, and stops at maximum X.</summary>
    MovingRight,
    /// <summary><c>$A2:F38C Function_HorizontalShutter_StoppedMovingLeft</c>: waits for signed timer underflow before moving right or restoring proximity waiting.</summary>
    StoppedAfterMovingLeft,
    /// <summary><c>$A2:F3B0 Function_HorizontalShutter_StoppedMovingRight</c>: waits for signed timer underflow before moving left or restoring proximity waiting.</summary>
    StoppedAfterMovingRight,
    /// <summary><c>$A2:F3D4 RTS_A2F3D4</c>: inert endpoint for rest time $0FF0, retaining stationary manual ejection and eligible reaction activation.</summary>
    PermanentNoOp,
}
