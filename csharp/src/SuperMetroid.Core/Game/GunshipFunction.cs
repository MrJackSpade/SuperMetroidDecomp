namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A2 Landing Site gunship functions stored in the top actor's variable F ($0FB2).</summary>
internal enum GunshipFunction : ushort
{
    /// <summary>Shared no-op function at $A2:804C.</summary>
    NoOperation = 0x804c,
    /// <summary>High-altitude post-Ceres descent at $A2:A80C.</summary>
    DescendAfterCeres = 0xa80c,
    /// <summary>Landing brake function at $A2:A8D0.</summary>
    ApplyLandingBrakes = 0xa8d0,
    /// <summary>Wait for the landing entrance to open at $A2:A942.</summary>
    WaitForLandingEntranceToOpen = 0xa942,
    /// <summary>Wait for the entrance to close after landing at $A2:A987.</summary>
    FinishLanding = 0xa987,
    /// <summary>Eject Samus after the Landing Site descent at $A2:A950.</summary>
    EjectSamus = 0xa950,
    /// <summary>Idle entry/exit handler at $A2:A9BD.</summary>
    Idle = 0xa9bd,
    /// <summary>Wait for the entrance to open at $A2:AA4F.</summary>
    WaitForEntranceToOpen = 0xaa4f,
    /// <summary>Lower Samus into the gunship at $A2:AA5D.</summary>
    LowerSamus = 0xaa5d,
    /// <summary>Wait for the entrance to close at $A2:AA94.</summary>
    WaitForEntranceToClose = 0xaa94,
    /// <summary>Begin liftoff or restore Samus at $A2:AAA2.</summary>
    BeginLiftoffOrRestoreSamus = 0xaaa2,
    /// <summary>Handle the gunship save confirmation at $A2:AB1F.</summary>
    HandleSaveConfirmation = 0xab1f,
    /// <summary>Wait for the exit pad to open at $A2:AB60.</summary>
    WaitForExitPadToOpen = 0xab60,
    /// <summary>Raise Samus out of the gunship at $A2:AB6E.</summary>
    RaiseSamus = 0xab6e,
    /// <summary>Wait for the entrance to close after Samus exits at $A2:ABA5.</summary>
    FinishSamusExit = 0xaba5,
    /// <summary>Load liftoff dust-cloud tiles at $A2:ABC7.</summary>
    LoadLiftoffDustTiles = 0xabc7,
    /// <summary>Run the engine-rumble/dust phase at $A2:AC1B.</summary>
    FireUpEngines = 0xac1b,
    /// <summary>Constant-speed first liftoff phase at $A2:ACD7.</summary>
    SteadyLiftoff = 0xacd7,
    /// <summary>Accelerating liftoff and game-state handoff at $A2:AD0E.</summary>
    AcceleratingLiftoff = 0xad0e,
    /// <summary>Shared accelerating vertical integrator at $A2:AD2D.</summary>
    MoveAccelerating = 0xad2d,
}
