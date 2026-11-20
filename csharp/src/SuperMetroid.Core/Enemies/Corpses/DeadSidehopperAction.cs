namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>
/// The behaviours a dead sidehopper's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum DeadSidehopperAction
{
    /// <summary>Activates the living victim when the camera's X is left of <see cref="DeadSidehopperVictimDefinitions.ActivationCameraX"/>, moving it in the same update.</summary>
    AliveWaitForCamera,
    /// <summary>Applies the hop velocity and gravity; landing advances the four-phase launch sequence and starts the hopping animation.</summary>
    ActivatedMovement,
    /// <summary>Lands: main AI waits while the landing animation runs, until its end-hop cue selects what follows.</summary>
    Landing,
    /// <summary>Selects the post-landing idle state and starts its pause (<see cref="DeadSidehopperVictimDefinitions.PostLandingPause"/>).</summary>
    BeginPostLandingDelay,
    /// <summary>Waits out the pause, then resumes hopping or begins the drain.</summary>
    PostLandingDelay,
    /// <summary>Steps Baby Metroid's drain each drain stage (<see cref="DeadSidehopperVictimDefinitions.DrainUpdatesPerStage"/>); once the victim is fully drained, shows the solid corpse at its drained height.</summary>
    Draining,
    /// <summary>Waits for Samus's solid-enemy collision to select the pre-rot delay; also the return state after rotting completes.</summary>
    WaitForSamusCollision,
    /// <summary>Counts the pre-rot delay from Samus's first touch (<see cref="CorpseRotScheduleDefinitions.PreRotDelayUpdates"/>), then starts rotting, Samus passing through the corpse from then on.</summary>
    PreRotDelay,
    /// <summary>Rots the corpse row by row by its <see cref="CorpseRotSchedule"/> and shows the rotted rows, including on the update the rot ends.</summary>
    Rotting,
}
