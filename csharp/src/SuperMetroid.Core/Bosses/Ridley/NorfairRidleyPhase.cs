namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The phases Lower Norfair Ridley runs once his lift-off ends the reveal (#2227): his entry into the arena, his attacks
/// and grabs, and his death. The state stores the member; its values are arbitrary.
/// </summary>
public enum NorfairRidleyPhase
{
    /// <summary>Moves Ridley into the arena after the reveal.</summary>
    EnterArena,
    /// <summary>Selects Ridley's next action from combat state.</summary>
    SelectAttack,
    /// <summary>Starts the hover Ridley recovers from a missed lunge in, with its limit (<see cref="NorfairRidleyMotionDefinitions.HoverUpdates"/>).</summary>
    HoverSetup,
    /// <summary>Hovers toward the point behind Ridley until he reaches it or the hover's time runs out.</summary>
    Hover,
    /// <summary>Starts the swoop: its countdown and heading, then moves Ridley to the swoop's start.</summary>
    SwoopSetup,
    /// <summary>Moves Ridley to its swoop start.</summary>
    SwoopMoveToStart,
    /// <summary>Descends through the downward-aiming swoop phase.</summary>
    SwoopAimDown,
    /// <summary>Rotates the descending swoop toward horizontal aim.</summary>
    SwoopAimSideways,
    /// <summary>Transitions the swoop into upward aim.</summary>
    SwoopAimUp,
    /// <summary>Accelerates the upward-aiming swoop ascent.</summary>
    SwoopClimb,
    /// <summary>Decelerates the swoop, then lunges at a grabbable Samus or chooses the next attack.</summary>
    SwoopRecover,
    /// <summary>Starts the wall sweep against a spin-jumping Samus: its tail spacing and random time limit.</summary>
    WallSweepSetup,
    /// <summary>Sweeps toward the wall behind Ridley at Samus's height, whipping his tail, while she spin jumps.</summary>
    SweepToWallBehind,
    /// <summary>Sweeps toward the wall ahead of Ridley at Samus's height, whipping his tail, while she spin jumps.</summary>
    SweepToWallAhead,
    /// <summary>Moves Ridley to the side of the arena he pogos from.</summary>
    PogoMoveToSide,
    /// <summary>Settles Ridley at the pogo height, then points his tail down and launches the first bounce.</summary>
    PogoMoveToHeight,
    /// <summary>Falls onto his tail, grabbing a grabbable Samus his claws reach, and bounces when the tail strikes the ground.</summary>
    PogoFall,
    /// <summary>Rises from a bounce until it peaks, or chooses the next attack once Samus leaves the pogo zone or time runs out.</summary>
    PogoRise,
    /// <summary>Lunges at Samus to grab her.</summary>
    GrabApproach,
    /// <summary>Initializes the carry state after grabbing Samus.</summary>
    CarrySetup,
    /// <summary>Moves the grabbed Samus toward the carry anchor.</summary>
    CarryMoveToAnchor,
    /// <summary>Rises while carrying Samus until the release timer expires.</summary>
    CarryRise,
    /// <summary>Recovers after releasing Samus and restores ordinary tail spacing.</summary>
    CarryRelease,
    /// <summary>Flies clear of an armed power bomb after a missed lunge, then resumes the fight or the carry.</summary>
    PowerBombDodge,
    /// <summary>Moves zero-health Ridley to the death spot while retaining any grabbed Samus, then starts his death roar.</summary>
    MoveToDeathSpot,
    /// <summary>Roars while still converging on the death spot, then stops and starts draining the acid.</summary>
    DeathRoar,
    /// <summary>Emits small explosions while the acid drains; expiry releases Samus and allocates the breakup actors.</summary>
    DeathExplosions,
    /// <summary>Hides the intact body and disables shared wing/tail animation after breakup allocation.</summary>
    DeathBreakup,
    /// <summary>Counts the deliberate thirty-two-frame wait before the terminal death delay.</summary>
    DeathWait,
    /// <summary>Completes the terminal wait, publishes boss persistence, scatters drops, requests music, and deletes the body.</summary>
    DeathFinish,
    /// <summary>Terminal empty phase selected after drops, music and deletion.</summary>
    DeathComplete,
}
