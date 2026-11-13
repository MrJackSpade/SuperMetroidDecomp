namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of Torizo's body animations does: show or hide the body, fade its colours, start its music, record its footsteps, queue its sounds, spawn its explosions, drool, orbs, eggs and missiles, lock its animation, or resume where it was interrupted.</summary>
internal enum TorizoCueKind
{
    /// <summary>Clears the body's frame behaviour.</summary>
    ClearFrameBehavior,
    /// <summary>Marks Bomb Torizo's gut blown up and spawns six drool pieces.</summary>
    BlowUpGut,
    /// <summary>Marks Bomb Torizo's face blown up.</summary>
    MarkFaceBlownUp,
    /// <summary>Makes the body visible.</summary>
    MakeVisible,
    /// <summary>Makes the body invisible.</summary>
    MakeInvisible,
    /// <summary>Starts the palette fade to black.</summary>
    FadeToBlack,
    /// <summary>Sets the boss bit, queues the elevator music and spawns the drops.</summary>
    FinishDeath,
    /// <summary>Advances the gradual colour fade.</summary>
    AdvanceColourFade,
    /// <summary>Starts the palette fade to the normal colors.</summary>
    FadeToNormalColors,
    /// <summary>Starts Bomb Torizo's fight music and belly palette effect.</summary>
    StartBombTorizoFight,
    /// <summary>Does nothing.</summary>
    NoOperation,
    /// <summary>Sets the animation lock.</summary>
    LockAnimation,
    /// <summary>Clears the animation lock.</summary>
    UnlockAnimation,
    /// <summary>Resumes at the saved return point.</summary>
    Return,
    /// <summary>Resumes the animation the gut explosion interrupted.</summary>
    ResumeInterrupted,
    /// <summary>Spawns a death explosion; the animation then waits one update.</summary>
    SpawnDeathExplosion,
    /// <summary>Spawns the landing dust clouds.</summary>
    SpawnLandingDust,
    /// <summary>Spawns drool when health is low.</summary>
    SpawnLowHealthDrool,
    /// <summary>Marks the body turning around.</summary>
    SetTurningAround,
    /// <summary>Records a left step on the left foot.</summary>
    SteppedLeftWithLeftFoot,
    /// <summary>Records a right step on the right foot.</summary>
    SteppedRightWithRightFoot,
    /// <summary>Records a left step on the right foot.</summary>
    SteppedLeftWithRightFoot,
    /// <summary>Records a right step on the left foot.</summary>
    SteppedRightWithLeftFoot,
    /// <summary>Spawns Bomb Torizo's Chozo orbs.</summary>
    SpawnBombTorizoChozoOrbs,
    /// <summary>Queues the shot sound.</summary>
    PlayShotSound,
    /// <summary>Queues the footstep sound.</summary>
    PlayFootstepSound,
    /// <summary>Loads Golden Torizo's palettes.</summary>
    LoadGoldenTorizoPalettes,
    /// <summary>Starts Golden Torizo's fight music and belly palette effect.</summary>
    StartGoldenTorizoFight,
    /// <summary>Clears the caught-super-missile flag.</summary>
    ClearCaughtSuperMissile,
    /// <summary>Spawns a Golden Torizo egg.</summary>
    SpawnEgg,
    /// <summary>Disables eye-beam explosions.</summary>
    DisableEyeBeamExplosions,
    /// <summary>Enables eye-beam explosions.</summary>
    EnableEyeBeamExplosions,
    /// <summary>Clears the stunned mark.</summary>
    UnmarkStunned,
    /// <summary>Queues the egg-released sound.</summary>
    PlayEggReleasedSound,
    /// <summary>Queues the laser sound.</summary>
    PlayLaserSound,
    /// <summary>Queues the sonic-boom sound.</summary>
    PlaySonicBoomSound,
    /// <summary>Spawns a super missile.</summary>
    SpawnSuperMissile,
    /// <summary>Spawns Golden Torizo's Chozo orbs.</summary>
    SpawnGoldenTorizoChozoOrbs,
}
