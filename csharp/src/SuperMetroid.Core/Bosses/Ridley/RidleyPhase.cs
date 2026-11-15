namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The phases of Ridley's AI shared by both encounters. The state stores the member; its values are arbitrary, and
/// verification's catalog relates each member to its source (#1413).
/// </summary>
/// <remarks>
/// Both encounters begin at <see cref="WaitForDoorTransition"/> and share the reveal through
/// <see cref="WaitBeforeLiftoff"/>, whose roar's liftoff starts <see cref="CeresLiftoffAccelerating"/> in Ceres and
/// <see cref="NorfairEnterArena"/> in Lower Norfair. Ceres Ridley's fake retreat, from
/// <see cref="CeresFakeRetreatMoveToPosition"/> to <see cref="CeresRetrieveBaby"/>, follows his hundredth hit: he rises,
/// drops the Baby and swoops to catch it.
/// </remarks>
public enum RidleyPhase
{
    /// <summary>Clears shared motion before returning to the flying step.</summary>
    ClearVelocity,
    /// <summary>Waits for the room-entry door transition before starting Ridley's reveal.</summary>
    WaitForDoorTransition,
    /// <summary>Counts the initial reveal delay before fading the eyes.</summary>
    InitialDelay,
    /// <summary>Advances the shared eye-colour fade before body reveal.</summary>
    FadeInEyes,
    /// <summary>Advances the body-colour fade and prepares the roar.</summary>
    FadeInBody,
    /// <summary>Waits before selecting the pre-flight roar animation.</summary>
    WaitBeforeRoar,
    /// <summary>Holds the pre-flight roar before encounter-specific liftoff.</summary>
    WaitBeforeLiftoff,
    /// <summary>Accelerates the Ceres body upward during liftoff.</summary>
    CeresLiftoffAccelerating,
    /// <summary>Decelerates Ceres liftoff into the hovering phase.</summary>
    CeresLiftoffDecelerating,
    /// <summary>Runs Ceres hovering and chooses its next scripted attack.</summary>
    CeresHovering,
    /// <summary>Moves the Ceres body to its fireball firing position.</summary>
    CeresFireballMoveToPosition,
    /// <summary>Maintains the Ceres fireball attack before returning to hover.</summary>
    CeresFireballShooting,
    /// <summary>Initializes the Ceres lunge movement and phase timer.</summary>
    CeresLungeSetup,
    /// <summary>Advances the Ceres lunge until recovery.</summary>
    CeresLungeMain,
    /// <summary>Initializes the Ceres downward-aiming swoop.</summary>
    CeresSwoopSetup,
    /// <summary>Steers Ceres Ridley to the swoop start position.</summary>
    CeresSwoopMoveToPosition,
    /// <summary>Descends through the downward-aiming portion of the Ceres swoop.</summary>
    CeresSwoopDescendingAimingDown,
    /// <summary>Turns the descending Ceres swoop toward a leftward aim.</summary>
    CeresSwoopDescendingAimingLeft,
    /// <summary>Starts the upward-aiming ascent of the Ceres swoop.</summary>
    CeresSwoopAscendingAimingUp,
    /// <summary>Continues the faster upward-aiming Ceres swoop ascent.</summary>
    CeresSwoopAscendingAimingUpFaster,
    /// <summary>Raises Ceres Ridley beyond the room for the real getaway.</summary>
    CeresRealRetreatRising,
    /// <summary>Prepares retreat colours and walls while waiting to publish the getaway.</summary>
    CeresRetreatDelay,
    /// <summary>Publishes the Ceres turning-floor getaway handoff.</summary>
    CeresPublishEscapeHandoff,
    /// <summary>The inactive Ceres body entry while its getaway presentation owns the scene.</summary>
    CeresInactive,
    /// <summary>Advances the Ceres self-destruct graphics, countdown setup, and optional Japanese typewriter phases.</summary>
    CeresActivateSelfDestruct,
    /// <summary>Keeps only the Ceres self-destruct alarm's flashing colours running after self-destruct setup.</summary>
    CeresSelfDestructAlarmOnly,

    /// <summary>Moves Lower Norfair Ridley into the arena after the shared reveal.</summary>
    NorfairEnterArena,
    /// <summary>Selects Lower Norfair Ridley's next action from combat state.</summary>
    NorfairSelectAttack,
    /// <summary>Starts the hover Ridley recovers from a missed lunge in, with its 128-update limit.</summary>
    NorfairHoverSetup,
    /// <summary>Hovers toward the point behind Ridley until he reaches it or the hover's time runs out.</summary>
    NorfairHover,
    /// <summary>Starts the swoop: its countdown and heading, then moves Ridley to the swoop's start.</summary>
    NorfairSwoopSetup,
    /// <summary>Moves Lower Norfair Ridley to its swoop start.</summary>
    NorfairSwoopMoveToStart,
    /// <summary>Descends through the downward-aiming Lower Norfair swoop phase.</summary>
    NorfairSwoopAimDown,
    /// <summary>Rotates the descending Lower Norfair swoop toward horizontal aim.</summary>
    NorfairSwoopAimSideways,
    /// <summary>Transitions the Lower Norfair swoop into upward aim.</summary>
    NorfairSwoopAimUp,
    /// <summary>Accelerates the upward-aiming Lower Norfair swoop ascent.</summary>
    NorfairSwoopClimb,
    /// <summary>Decelerates the swoop, then lunges at a grabbable Samus or chooses the next attack.</summary>
    NorfairSwoopRecover,
    /// <summary>Starts the wall sweep against a spin-jumping Samus: its tail spacing and random time limit.</summary>
    NorfairWallSweepSetup,
    /// <summary>Sweeps toward the wall behind Ridley at Samus's height, whipping his tail, while she spin jumps.</summary>
    NorfairSweepToWallBehind,
    /// <summary>Sweeps toward the wall ahead of Ridley at Samus's height, whipping his tail, while she spin jumps.</summary>
    NorfairSweepToWallAhead,
    /// <summary>Moves Ridley to the side of the arena he pogos from.</summary>
    NorfairPogoMoveToSide,
    /// <summary>Settles Ridley at the pogo height, then points his tail down and launches the first bounce.</summary>
    NorfairPogoMoveToHeight,
    /// <summary>Falls onto his tail, grabbing a grabbable Samus his claws reach, and bounces when the tail strikes the ground.</summary>
    NorfairPogoFall,
    /// <summary>Rises from a bounce until it peaks, or chooses the next attack once Samus leaves the pogo zone or time runs out.</summary>
    NorfairPogoRise,
    /// <summary>Lunges at Samus to grab her.</summary>
    NorfairGrabApproach,
    /// <summary>Initializes the Lower Norfair carry state after grabbing Samus.</summary>
    NorfairCarrySetup,
    /// <summary>Moves the grabbed Samus toward the carry anchor.</summary>
    NorfairCarryMoveToAnchor,
    /// <summary>Rises while carrying Samus until the release timer expires.</summary>
    NorfairCarryRise,
    /// <summary>Recovers after releasing Samus and restores ordinary tail spacing.</summary>
    NorfairCarryRelease,
    /// <summary>Flies clear of an armed power bomb after a missed lunge, then resumes the fight or the carry.</summary>
    NorfairPowerBombDodge,

    /// <summary>Steers Ceres Ridley to the start of his fake retreat after his hundredth hit.</summary>
    CeresFakeRetreatMoveToPosition,
    /// <summary>Raises Ceres Ridley until he lets the Baby fall.</summary>
    CeresFakeRetreatRising,
    /// <summary>Starts the Baby's fall and waits before the Ceres retrieval animation.</summary>
    CeresWaitBeforeRetrievingBaby,
    /// <summary>Steers Ridley's hand toward the falling Baby and reacquires it.</summary>
    CeresRetrieveBaby,

    /// <summary>Moves zero-health Ridley to the death spot while retaining any grabbed Samus, then starts his death roar.</summary>
    NorfairMoveToDeathSpot,
    /// <summary>Roars while still converging on the death spot, then stops and starts draining the acid.</summary>
    NorfairDeathRoar,
    /// <summary>Emits small explosions while the acid drains; expiry releases Samus and allocates the breakup actors.</summary>
    NorfairDeathExplosions,
    /// <summary>Hides the intact body and disables shared wing/tail animation after breakup allocation.</summary>
    NorfairDeathBreakup,
    /// <summary>Counts the deliberate thirty-two-frame wait before the terminal death delay.</summary>
    NorfairDeathWait,
    /// <summary>Completes the terminal wait, publishes boss persistence, scatters drops, requests music, and deletes the body.</summary>
    NorfairDeathFinish,
    /// <summary>Terminal empty phase selected after drops, music and deletion.</summary>
    NorfairDeathComplete,
}
