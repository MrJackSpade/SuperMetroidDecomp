namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The main function a Bomb or Golden Torizo runs; each member names the bank-$AA routine it replaced (#1413: the values are arbitrary).</summary>
public enum TorizoFunction
{
    /// <summary><c>RTS_AAC6AB</c>: dormant.</summary>
    Idle = 1,

    /// <summary><c>Function_Torizo_SimpleMovement</c>.</summary>
    Falling,

    /// <summary><c>Function_Torizo_WakeWhenBombTorizoChozoFinishesCrumbling</c>.</summary>
    WaitForHandTrigger,

    /// <summary><c>Function_Torizo_NormalMovement</c>.</summary>
    Active,

    /// <summary><c>Function_GoldenTorizo_WakeIfSamusIsBelowAndRightOfTargetPos</c>.</summary>
    GoldenWaitForSamus,

    /// <summary><c>Function_GoldenTorizo_SimpleMovement</c>.</summary>
    GoldenGravity,

    /// <summary><c>Function_GoldenTorizo_NormalMovement</c>.</summary>
    GoldenFrameBehavior,
}
