namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A2 dispatcher values shared by the three vertical shutter definitions.</summary>
public enum VerticalShutterFunction : ushort
{
    /// <summary><c>$A2:EF09 Function_Shutter_Kamer_Initial</c>: calls the trigger-mode table entry during this same AI update until activation replaces the function.</summary>
    Initial = 0xef09,
    /// <summary><c>$A2:EF15 Function_Shutter_Kamer_WaitForTimer</c>: decrements the initial timer and activates on zero, restoring the configured wait value.</summary>
    WaitForTimer = 0xef15,
    /// <summary><c>$A2:EF28 Function_Shutter_Kamer_WaitForSamusToGetNear</c>: waits for Samus's strict horizontal proximity threshold.</summary>
    WaitForHorizontalProximity = 0xef28,
    /// <summary><c>$A2:EF39 Function_Shutter_Kamer_Activate</c>: selects primary upward/downward movement and queues the activation sound when on screen.</summary>
    Activate = 0xef39,
    /// <summary><c>$A2:EF40 Function_Shutter_Kamer_GetEnemyIndex</c>: initial inert function for the one-shot and repeatable reaction-triggered modes.</summary>
    InitialNoOp = 0xef40,
    /// <summary><c>$A2:EF68 Function_Kamer_MovingUp</c>: adds upward 16.16 velocity, carries a rider, and stops at the minimum Y boundary.</summary>
    MovingUp = 0xef68,
    /// <summary><c>$A2:EFD4 Function_Kamer_MovingDown</c>: adds downward 16.16 velocity, carries a rider, and stops at the maximum Y boundary.</summary>
    MovingDown = 0xefd4,
    /// <summary><c>$A2:F040 Function_Kamer_StoppedMovingUp</c>: waits for signed timer underflow before moving down or restoring proximity waiting.</summary>
    StoppedAfterMovingUp = 0xf040,
    /// <summary><c>$A2:F072 Function_Kamer_StoppedMovingDown</c>: waits for signed timer underflow before moving up or restoring proximity waiting.</summary>
    StoppedAfterMovingDown = 0xf072,
    /// <summary><c>$A2:F099 Function_Shutter_Kamer_GetEnemyIndex_duplicate</c>: inert endpoint selected by rest time $0FF0; eligible reaction modes can still reactivate it.</summary>
    PermanentNoOp = 0xf099,
}
