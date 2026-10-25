namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The main function a Bomb or Golden Torizo runs; each member names the bank-$AA routine it replaced (#1413: the values are arbitrary).</summary>
public enum TorizoFunction
{
    /// <summary><c>RTS_AAC6AB</c>: dormant.</summary>
    Idle = 1,

    /// <summary><c>Function_Torizo_SimpleMovement</c> at $AA:C6BF.</summary>
    Falling,

    /// <summary><c>Function_Torizo_WakeWhenBombTorizoChozoFinishesCrumbling</c> at $AA:C6C6.</summary>
    WaitForHandTrigger,

    /// <summary><c>Function_Torizo_NormalMovement</c> at $AA:C6FF.</summary>
    Active,

    /// <summary><c>Function_GoldenTorizo_WakeIfSamusIsBelowAndRightOfTargetPos</c> at $AA:D5C2.</summary>
    GoldenWaitForSamus,

    /// <summary><c>Function_GoldenTorizo_SimpleMovement</c> at $AA:D5DF.</summary>
    GoldenGravity,

    /// <summary><c>Function_GoldenTorizo_NormalMovement</c> at $AA:D5E6.</summary>
    GoldenPreInstruction,
}
