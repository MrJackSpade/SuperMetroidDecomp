namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The routines a dead sidehopper's main AI dispatches.
/// Its values are arbitrary.
/// </summary>
public enum DeadSidehopperAiFunction
{
    /// <summary>Activates the living victim when the signed camera-X comparison is below room pixel 513, falling through into movement.</summary>
    AliveWaitForCamera = 1,
    /// <summary>Applies signed 8.8 hop velocity and gravity; landing advances the four-phase launch sequence and starts the hopping animation.</summary>
    ActivatedMovement,
    /// <summary>Holds main AI idle while the landing animation runs until the end-hop cue selects its successor.</summary>
    NoOperation,
    /// <summary>Selects the post-landing idle state and initializes the function timer to 64.</summary>
    BeginPostLandingDelay,
    /// <summary>Decrements the timer through zero into signed-negative expiry, then resumes hopping or begins the drained palette transformation.</summary>
    PostLandingDelay,
    /// <summary>Advances the fifteen-color drained palette every eight AI calls, then installs solid corpse graphics and a 12-pixel Y radius.</summary>
    TransformPalette,
    /// <summary>Waits for Samus's solid-enemy collision to select the pre-rot delay; also the return state after rotting completes.</summary>
    WaitForSamusCollision,
    /// <summary>Increments the retained contact-delay counter to sixteen before starting rotting and rejecting normal interaction.</summary>
    PreRotDelay,
    /// <summary>Runs the shared staggered pixel-row scheduler and submits the configured tile transfers, including on the completion call.</summary>
    Rotting,
}
