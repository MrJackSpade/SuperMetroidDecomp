namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The behaviours a Rinka runs each update.
/// Its values are arbitrary.
/// </summary>
public enum RinkaAction
{
    /// <summary>Waits out the aim delay (<see cref="RinkaLifeDefinitions.AimDelay"/>), then samples Samus once to form signed 1/256-pixel velocities at a nominal speed of 288; movement begins on the following AI update.</summary>
    AimDelay,
    /// <summary>Hidden Mother Brain variant waits out its respawn delay (<see cref="RinkaLifeDefinitions.RespawnDelay"/>), restores ten health, and reinitializes at a reserved spawn point.</summary>
    DeathRespawnDelay,
    /// <summary>Stationary initial state that recycles an actor outside the expanded camera flight rectangle; the animation's fire step, not this action's timer, starts the aim delay.</summary>
    WatchForLeavingViewport,
    /// <summary>Adds the fixed signed 1/256-pixel velocities to the actor's fractional room position each update, then recycles it outside the camera-relative -16..271 X / -16..239 Y window without re-aiming.</summary>
    Flying,
}
