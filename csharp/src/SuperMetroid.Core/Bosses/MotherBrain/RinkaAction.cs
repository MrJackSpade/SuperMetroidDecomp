namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The behaviours a Rinka runs each update.
/// Its values are arbitrary.
/// </summary>
public enum RinkaAction
{
    /// <summary>Waits out the aim delay (<see cref="RinkaLifeDefinitions.AimDelayUpdates"/>), then aims at Samus once at a speed of a pixel and an eighth an update; it starts moving on the following update.</summary>
    AimDelay,
    /// <summary>Hidden Mother Brain variant waits out its respawn delay (<see cref="RinkaLifeDefinitions.RespawnDelayUpdates"/>), restores ten health, and reinitializes at a reserved spawn point.</summary>
    DeathRespawnDelay,
    /// <summary>Stationary initial state that recycles an actor outside the expanded camera flight rectangle; the animation's fire step, not this action's timer, starts the aim delay.</summary>
    WatchForLeavingViewport,
    /// <summary>Moves by its fixed velocity each update, then recycles it outside the camera-relative -16..271 X / -16..239 Y window without re-aiming.</summary>
    Flying,
}
