namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of Torizo's body animations does: start or stop its body's motion, show or hide the body, fade its colours, start its music, record its footsteps, queue its sounds, spawn its explosions, drool, orbs, eggs and missiles, lock its animation, resume the walk or the animation an attack or the gut explosion interrupted, or mark the moment a held pose has shown, on which its owner starts the next animation once the Torizo's state has changed.</summary>
internal enum TorizoCueKind
{
    /// <summary>Starts the body walking (#1931).</summary>
    StartWalking,
    /// <summary>Starts the body standing its ground (#1931).</summary>
    StartStanding,
    /// <summary>Starts the body leaping (#1931).</summary>
    StartLeaping,
    /// <summary>Stops the body's motion: it holds still (#1931).</summary>
    StopMoving,
    /// <summary>Marks Bomb Torizo's gut blown up and spawns its drool pieces (<see cref="TorizoDeathDefinitions.GutDroolPieces"/>).</summary>
    BlowUpGut,
    /// <summary>Marks Bomb Torizo's face blown up.</summary>
    MarkFaceBlownUp,
    /// <summary>Makes the body visible.</summary>
    MakeVisible,
    /// <summary>Makes the body invisible.</summary>
    MakeInvisible,
    /// <summary>Starts the colour fade to black.</summary>
    FadeToBlack,
    /// <summary>Records the boss's defeat, queues the elevator music and spawns the drops.</summary>
    FinishDeath,
    /// <summary>Advances the gradual colour fade.</summary>
    AdvanceColourFade,
    /// <summary>Starts the colour fade to the normal colors.</summary>
    FadeToNormalColors,
    /// <summary>Starts Bomb Torizo's fight music and belly glow.</summary>
    StartBombTorizoFight,
    /// <summary>Sets the animation lock.</summary>
    LockAnimation,
    /// <summary>Clears the animation lock.</summary>
    UnlockAnimation,
    /// <summary>Ends an attack or a stagger: the walk it interrupted resumes where it stopped (#2028).</summary>
    ResumeWalk,
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
    /// <summary>Spawns Bomb Torizo's Chozo orbs.</summary>
    SpawnBombTorizoChozoOrbs,
    /// <summary>Spawns a Bomb Torizo sonic boom.</summary>
    SpawnBombTorizoSonicBoom,
    /// <summary>Spawns a Golden Torizo sonic boom.</summary>
    SpawnGoldenTorizoSonicBoom,
    /// <summary>Spawns Golden Torizo's eye beam.</summary>
    SpawnEyeBeam,
    /// <summary>Queues the shot sound.</summary>
    PlayShotSound,
    /// <summary>Queues the footstep sound.</summary>
    PlayFootstepSound,
    /// <summary>Starts the colour fade to Golden Torizo's colours.</summary>
    FadeToGoldenColors,
    /// <summary>Starts Golden Torizo's fight music and belly glow.</summary>
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
    /// <summary>
    /// A jumping Torizo has shown its leaping pose (#2411): once it no longer rises, its owner starts the fall its jump
    /// named; while it rises, its jump shows the pose again.
    /// </summary>
    LeapingPoseShown,
    /// <summary>
    /// A waking Golden Torizo has shown its half-risen pose as it drops (#2411): once it is on its floor, its owner starts its
    /// settling; until then it shows the pose again.
    /// </summary>
    DroppingPoseShown,
    /// <summary>
    /// Golden Torizo has held its chest open a moment (#2411): once its eggs are gone, its owner starts its chest's closing;
    /// while any remains, it holds it open again.
    /// </summary>
    ChestHeldOpen,
}
