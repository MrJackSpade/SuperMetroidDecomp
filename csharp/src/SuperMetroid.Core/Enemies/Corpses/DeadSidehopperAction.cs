namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The behaviours a dead sidehopper's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum DeadSidehopperAction
{
    /// <summary>Activates the living victim when the camera's X is left of room pixel 513, moving it in the same update.</summary>
    AliveWaitForCamera,
    /// <summary>Applies signed 1/256-pixel hop velocity and gravity; landing advances the four-phase launch sequence and starts the hopping animation.</summary>
    ActivatedMovement,
    /// <summary>Lands: main AI waits while the landing animation runs, until its end-hop cue selects what follows.</summary>
    Landing,
    /// <summary>Selects the post-landing idle state and starts its pause (<see cref="DeadSidehopperVictimDefinitions.PostLandingPause"/>).</summary>
    BeginPostLandingDelay,
    /// <summary>Waits out the pause, then resumes hopping or begins the drain.</summary>
    PostLandingDelay,
    /// <summary>Steps Shitroid's drain every eight AI updates; once the victim is fully drained, shows the solid corpse with a 12-pixel Y radius.</summary>
    Draining,
    /// <summary>Waits for Samus's solid-enemy collision to select the pre-rot delay; also the return state after rotting completes.</summary>
    WaitForSamusCollision,
    /// <summary>Increments the retained contact-delay counter to sixteen before starting rotting and rejecting normal interaction.</summary>
    PreRotDelay,
    /// <summary>Runs the shared staggered pixel-row scheduler and shows the rotted rows, including on the completion call.</summary>
    Rotting,
}
