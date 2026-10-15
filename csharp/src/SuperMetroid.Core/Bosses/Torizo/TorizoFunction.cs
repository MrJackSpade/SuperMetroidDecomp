namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bank-$AA main functions a Bomb or Golden Torizo installs in <c>toriz_var_E</c>.</summary>
public enum TorizoFunction : ushort
{
    /// <summary><c>RTS_AAC6AB</c>: dormant.</summary>
    Idle = 0xc6ab,

    /// <summary><c>Function_Torizo_SimpleMovement</c> at $AA:C6BF.</summary>
    Falling = 0xc6bf,

    /// <summary><c>Function_Torizo_WakeWhenBombTorizoChozoFinishesCrumbling</c> at $AA:C6C6.</summary>
    WaitForHandTrigger = 0xc6c6,

    /// <summary><c>Function_Torizo_NormalMovement</c> at $AA:C6FF.</summary>
    Active = 0xc6ff,

    /// <summary><c>Function_GoldenTorizo_WakeIfSamusIsBelowAndRightOfTargetPos</c> at $AA:D5C2.</summary>
    GoldenWaitForSamus = 0xd5c2,

    /// <summary><c>Function_GoldenTorizo_SimpleMovement</c> at $AA:D5DF.</summary>
    GoldenGravity = 0xd5df,

    /// <summary><c>Function_GoldenTorizo_NormalMovement</c> at $AA:D5E6.</summary>
    GoldenPreInstruction = 0xd5e6,
}
