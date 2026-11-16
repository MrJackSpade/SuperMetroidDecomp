namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Polyp's sequential AI states.
/// Its values are arbitrary.
/// </summary>
public enum PolypAction
{
    /// <summary>Tests strict modular distances below 64 whole pixels on both axes, selecting the shoot action without spawning until the next AI update.</summary>
    WaitingForSamus,
    /// <summary>Consumes separate shared-RNG samples for horizontal velocity, the rock's first place on the speed curve, and cooldown; requests one lava rock and enters cooldown even if the projectile pool is full.</summary>
    ShootingRock,
    /// <summary>Waits out the cooldown its launch chose (<see cref="PolypLaunchDefinitions.Cooldowns"/>), then rearms proximity checking.</summary>
    Cooldown,
}
