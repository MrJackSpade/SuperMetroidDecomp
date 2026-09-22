namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A2 function pointers used by the Landing Site gunship.</summary>
internal static class GunshipCodePointers
{
    /// <summary>Shared no-op function at $A2:804C.</summary>
    public const ushort NoOperation = 0x804c;
    /// <summary>High-altitude post-Ceres descent at $A2:A80C.</summary>
    public const ushort DescendAfterCeres = 0xa80c;
    /// <summary>Landing brake function at $A2:A8D0.</summary>
    public const ushort ApplyLandingBrakes = 0xa8d0;
    /// <summary>Wait for the landing entrance to open at $A2:A942.</summary>
    public const ushort WaitForLandingEntranceToOpen = 0xa942;
    /// <summary>Wait for the entrance to close after landing at $A2:A987.</summary>
    public const ushort FinishLanding = 0xa987;
    /// <summary>Eject Samus after the Landing Site descent at $A2:A950.</summary>
    public const ushort EjectSamus = 0xa950;
    /// <summary>Idle entry/exit handler at $A2:A9BD.</summary>
    public const ushort Idle = 0xa9bd;
    /// <summary>Wait for the entrance to open at $A2:AA4F.</summary>
    public const ushort WaitForEntranceToOpen = 0xaa4f;
    /// <summary>Lower Samus into the gunship at $A2:AA5D.</summary>
    public const ushort LowerSamus = 0xaa5d;
    /// <summary>Wait for the entrance to close at $A2:AA94.</summary>
    public const ushort WaitForEntranceToClose = 0xaa94;
    /// <summary>Begin liftoff or restore Samus at $A2:AAA2.</summary>
    public const ushort BeginLiftoffOrRestoreSamus = 0xaaa2;
    /// <summary>Handle the gunship save confirmation at $A2:AB1F.</summary>
    public const ushort HandleSaveConfirmation = 0xab1f;
    /// <summary>Wait for the exit pad to open at $A2:AB60.</summary>
    public const ushort WaitForExitPadToOpen = 0xab60;
    /// <summary>Raise Samus out of the gunship at $A2:AB6E.</summary>
    public const ushort RaiseSamus = 0xab6e;
    /// <summary>Wait for the entrance to close after Samus exits at $A2:ABA5.</summary>
    public const ushort FinishSamusExit = 0xaba5;
    /// <summary>Load liftoff dust-cloud tiles at $A2:ABC7.</summary>
    public const ushort LoadLiftoffDustTiles = 0xabc7;
    /// <summary>Run the engine-rumble/dust phase at $A2:AC1B.</summary>
    public const ushort FireUpEngines = 0xac1b;
    /// <summary>Constant-speed first liftoff phase at $A2:ACD7.</summary>
    public const ushort SteadyLiftoff = 0xacd7;
    /// <summary>Accelerating liftoff and game-state handoff at $A2:AD0E.</summary>
    public const ushort AcceleratingLiftoff = 0xad0e;
    /// <summary>Shared accelerating vertical integrator at $A2:AD2D.</summary>
    public const ushort MoveAccelerating = 0xad2d;
}
