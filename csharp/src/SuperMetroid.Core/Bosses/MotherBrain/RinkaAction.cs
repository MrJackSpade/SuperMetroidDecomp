namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The behaviours a Rinka runs each update.
/// Its values are arbitrary.
/// </summary>
public enum RinkaAction
{
    /// <summary>Waits out the aim delay (<see cref="RinkaLifeDefinitions.AimDelayUpdates"/>), then aims at Samus once at <see cref="RinkaLifeDefinitions.Speed"/>; it starts moving on the following update.</summary>
    AimDelay,
    /// <summary>Hidden Mother Brain variant waits out its respawn delay (<see cref="RinkaLifeDefinitions.RespawnDelayUpdates"/>), restores its health (<see cref="RinkaLifeDefinitions.RespawnHealth"/>), and reinitializes at a reserved spawn point.</summary>
    DeathRespawnDelay,
    /// <summary>Stationary initial state that recycles an actor outside the expanded camera flight rectangle; the animation's fire step, not this action's timer, starts the aim delay.</summary>
    WatchForLeavingViewport,
    /// <summary>Moves by its fixed velocity each update, then recycles it outside the camera-relative flight window, a margin past the screen, without re-aiming.</summary>
    Flying,
}
