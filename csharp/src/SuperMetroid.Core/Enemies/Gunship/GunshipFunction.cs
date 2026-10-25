namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The Landing Site gunship's routines, run by its top hull.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
internal enum GunshipFunction : ushort
{
    /// <summary>Shared no-op function at $A2:804C.</summary>
    NoOperation = 1,
    /// <summary>High-altitude post-Ceres descent at $A2:A80C.</summary>
    DescendAfterCeres,
    /// <summary>Landing brake function at $A2:A8D0.</summary>
    ApplyLandingBrakes,
    /// <summary>Wait for the landing entrance to open at $A2:A942.</summary>
    WaitForLandingEntranceToOpen,
    /// <summary>Wait for the entrance to close after landing at $A2:A987.</summary>
    FinishLanding,
    /// <summary>Eject Samus after the Landing Site descent at $A2:A950.</summary>
    EjectSamus,
    /// <summary>Idle entry/exit handler at $A2:A9BD.</summary>
    Idle,
    /// <summary>Wait for the entrance to open at $A2:AA4F.</summary>
    WaitForEntranceToOpen,
    /// <summary>Lower Samus into the gunship at $A2:AA5D.</summary>
    LowerSamus,
    /// <summary>Wait for the entrance to close at $A2:AA94.</summary>
    WaitForEntranceToClose,
    /// <summary>Begin liftoff or restore Samus at $A2:AAA2.</summary>
    BeginLiftoffOrRestoreSamus,
    /// <summary>Handle the gunship save confirmation at $A2:AB1F.</summary>
    HandleSaveConfirmation,
    /// <summary>Wait for the exit pad to open at $A2:AB60.</summary>
    WaitForExitPadToOpen,
    /// <summary>Raise Samus out of the gunship at $A2:AB6E.</summary>
    RaiseSamus,
    /// <summary>Wait for the entrance to close after Samus exits at $A2:ABA5.</summary>
    FinishSamusExit,
    /// <summary>Load liftoff dust-cloud tiles at $A2:ABC7.</summary>
    LoadLiftoffDustTiles,
    /// <summary>Run the engine-rumble/dust phase at $A2:AC1B.</summary>
    FireUpEngines,
    /// <summary>Constant-speed first liftoff phase at $A2:ACD7.</summary>
    SteadyLiftoff,
    /// <summary>Accelerating liftoff and game-state handoff at $A2:AD0E.</summary>
    AcceleratingLiftoff,
    /// <summary>Shared accelerating vertical integrator at $A2:AD2D.</summary>
    MoveAccelerating,
}
