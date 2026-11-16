namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The phases Ceres Ridley runs once his lift-off ends the reveal (#2227): his rise, his hover and its attacks, his fake
/// retreat after his hundredth hit (he rises, drops the Baby and swoops to catch it), his real retreat with the Baby and
/// his handoff to the station's escape (<see cref="CeresEscape"/>). The state stores the member; its values are arbitrary.
/// </summary>
public enum CeresRidleyPhase
{
    /// <summary>Accelerates the body upward during liftoff.</summary>
    LiftoffAccelerating,
    /// <summary>Decelerates liftoff into the hovering phase.</summary>
    LiftoffDecelerating,
    /// <summary>Runs hovering and chooses its next scripted attack.</summary>
    Hovering,
    /// <summary>Moves the body to its fireball firing position.</summary>
    FireballMoveToPosition,
    /// <summary>Maintains the fireball attack before returning to hover.</summary>
    FireballShooting,
    /// <summary>Initializes the lunge movement and phase timer.</summary>
    LungeSetup,
    /// <summary>Advances the lunge until recovery.</summary>
    LungeMain,
    /// <summary>Initializes the downward-aiming swoop.</summary>
    SwoopSetup,
    /// <summary>Steers Ridley to the swoop start position.</summary>
    SwoopMoveToPosition,
    /// <summary>Descends through the downward-aiming portion of the swoop.</summary>
    SwoopDescendingAimingDown,
    /// <summary>Turns the descending swoop toward a leftward aim.</summary>
    SwoopDescendingAimingLeft,
    /// <summary>Starts the upward-aiming ascent of the swoop.</summary>
    SwoopAscendingAimingUp,
    /// <summary>Continues the faster upward-aiming swoop ascent.</summary>
    SwoopAscendingAimingUpFaster,
    /// <summary>Steers Ridley to the start of his fake retreat after his hundredth hit.</summary>
    FakeRetreatMoveToPosition,
    /// <summary>Raises Ridley until he lets the Baby fall.</summary>
    FakeRetreatRising,
    /// <summary>Starts the Baby's fall and waits before the retrieval animation.</summary>
    WaitBeforeRetrievingBaby,
    /// <summary>Steers Ridley's hand toward the falling Baby and reacquires it.</summary>
    RetrieveBaby,
    /// <summary>Raises Ridley beyond the room for the real getaway.</summary>
    RealRetreatRising,
    /// <summary>Prepares retreat colours and walls while waiting to publish the getaway.</summary>
    RetreatDelay,
    /// <summary>Hides Ridley and begins the station's escape with his turning-floor getaway.</summary>
    PublishEscapeHandoff,
    /// <summary>
    /// He has got away (#2228): the station's escape owns the room, and his AI runs the escape's step in his place in the
    /// update (nothing during the getaway, then the self-destruct warning and its alarm).
    /// </summary>
    Escaped,
}
