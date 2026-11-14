namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The Landing Site gunship's phases, run by its top hull.
/// Its values are arbitrary.
/// </summary>
internal enum GunshipPhase
{
    /// <summary>A cleared gunship piece holds no phase.</summary>
    None = 0,
    /// <summary>Shared no-op phase.</summary>
    NoOperation = 1,
    /// <summary>High-altitude post-Ceres descent.</summary>
    DescendAfterCeres,
    /// <summary>Landing brake phase.</summary>
    ApplyLandingBrakes,
    /// <summary>Wait for the landing entrance to open.</summary>
    WaitForLandingEntranceToOpen,
    /// <summary>Wait for the entrance to close after landing.</summary>
    FinishLanding,
    /// <summary>Eject Samus after the Landing Site descent.</summary>
    EjectSamus,
    /// <summary>Idle entry/exit handler.</summary>
    Idle,
    /// <summary>Wait for the entrance to open.</summary>
    WaitForEntranceToOpen,
    /// <summary>Lower Samus into the gunship.</summary>
    LowerSamus,
    /// <summary>Wait for the entrance to close.</summary>
    WaitForEntranceToClose,
    /// <summary>Begin liftoff or restore Samus.</summary>
    BeginLiftoffOrRestoreSamus,
    /// <summary>Handle the gunship save confirmation.</summary>
    HandleSaveConfirmation,
    /// <summary>Wait for the exit pad to open.</summary>
    WaitForExitPadToOpen,
    /// <summary>Raise Samus out of the gunship.</summary>
    RaiseSamus,
    /// <summary>Wait for the entrance to close after Samus exits.</summary>
    FinishSamusExit,
    /// <summary>Ready the engines for the takeoff, which then fire up.</summary>
    ReadyTakeoffEngines,
    /// <summary>Run the engine-rumble/dust phase.</summary>
    FireUpEngines,
    /// <summary>Constant-speed first liftoff phase.</summary>
    SteadyLiftoff,
    /// <summary>Accelerating liftoff and game-state handoff.</summary>
    AcceleratingLiftoff,
    /// <summary>Shared accelerating vertical integrator.</summary>
    MoveAccelerating,
}
