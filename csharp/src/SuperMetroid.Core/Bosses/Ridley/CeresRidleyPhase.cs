namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The phases Ceres Ridley runs once his lift-off ends the reveal (#2227): his rise, his hover and its attacks, his fake
/// retreat after his hundredth hit (he rises, drops the Baby and swoops to catch it), his real retreat with the Baby and
/// the getaway and self-destruct that follow. The state stores the member; its values are arbitrary.
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
    /// <summary>Publishes the turning-floor getaway handoff.</summary>
    PublishEscapeHandoff,
    /// <summary>The inactive body while its getaway presentation owns the scene.</summary>
    Inactive,
    /// <summary>Advances the self-destruct graphics, countdown setup, and optional Japanese typewriter phases.</summary>
    ActivateSelfDestruct,
    /// <summary>Keeps only the self-destruct alarm's flashing colours running after self-destruct setup.</summary>
    SelfDestructAlarmOnly,
}
