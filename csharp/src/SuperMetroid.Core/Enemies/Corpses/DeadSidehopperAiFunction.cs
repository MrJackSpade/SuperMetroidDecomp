namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The routines a dead sidehopper's main AI dispatches.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum DeadSidehopperAiFunction
{
    /// <summary>$A9:D8E2, Function_CorpseSidehopper_Alive_WaitingForActivation: activates the living victim when the signed camera-X comparison is below room pixel 513, falling through into movement.</summary>
    AliveWaitForCamera = 1,
    /// <summary>$A9:D8F1, Function_CorpseSidehopper_Hopping: applies signed 8.8 hop velocity and gravity; landing advances the four-phase launch sequence and starts the hopping animation.</summary>
    ActivatedMovement,
    /// <summary>$A9:D90F, the hopping routine's RTS: holds main AI idle while the landing animation runs until instruction $A9:ECD0 selects its successor.</summary>
    NoOperation,
    /// <summary>$A9:D910, Function_CorpseSidehopper_StartIdling: selects the post-landing idle state and initializes the native function timer to $0040.</summary>
    BeginPostLandingDelay,
    /// <summary>$A9:D91D, Function_CorpseSidehopper_Idling: decrements the timer through zero into signed-negative expiry, then resumes hopping or begins the drained palette transformation.</summary>
    PostLandingDelay,
    /// <summary>$A9:DA08, Function_CorpseSidehopper_BeingDrained: advances the fifteen-color drained palette every eight AI calls, then installs solid corpse graphics and a 12-pixel Y radius.</summary>
    TransformPalette,
    /// <summary>$A9:DA64, Function_CorpseSidehopper_Dead_WaitForSamusCollision: waits for Samus's solid-enemy collision to select the pre-rot delay; also the native return state after rotting completes.</summary>
    WaitForSamusCollision,
    /// <summary>$A9:DA8F, Function_CorpseSidehopper_PreRotDelay: increments the retained contact-delay counter to sixteen before starting rotting and rejecting normal interaction.</summary>
    PreRotDelay,
    /// <summary>$A9:DABA, Function_CorpseSidehopper_Rotting: runs the shared staggered pixel-row scheduler and submits the configured VRAM transfers, including on the completion call.</summary>
    Rotting,
}
