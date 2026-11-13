namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The behaviours Rinka's main AI dispatches.
/// Its values are arbitrary; verification's catalog relates each member to its source (#1413).
/// </summary>
public enum RinkaAction
{
    /// <summary>Counts the initially 26-update timer down until it passes zero, then samples Samus once to form signed 8.8 velocities at a nominal speed of 288; movement begins on the following AI update.</summary>
    AimDelay = 1,
    /// <summary>Hidden Mother Brain variant waits for its timer to pass zero (1, 0), restores ten health, and reinitializes at a reserved spawn point.</summary>
    DeathRespawnDelay,
    /// <summary>Stationary initial state that recycles an actor outside the expanded camera flight rectangle; the animation's fire step, not this action's timer, starts the aim delay.</summary>
    WatchForLeavingViewport,
    /// <summary>Adds the fixed signed 8.8 velocities to the actor's fractional room position each update, then recycles it outside the camera-relative -16..271 X / -16..239 Y window without re-aiming.</summary>
    Flying,
}
