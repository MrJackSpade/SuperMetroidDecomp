namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Polyp's sequential AI states.
/// Its values are arbitrary.
/// </summary>
public enum PolypAction
{
    /// <summary>Tests strict modular distances below 64 whole pixels on both axes, selecting the shoot action without spawning until the next AI update.</summary>
    WaitingForSamus,
    /// <summary>Consumes separate shared-RNG samples for horizontal velocity, initial quadratic Y-speed selector, and cooldown; requests one lava rock and enters cooldown even if the projectile pool is full.</summary>
    ShootingRock,
    /// <summary>Decrements the cooldown once per AI update and rearms proximity checking only when the signed result is negative, so an initial nonnegative count N waits N+1 updates.</summary>
    Cooldown,
}
