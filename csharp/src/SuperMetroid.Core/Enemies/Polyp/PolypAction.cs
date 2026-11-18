namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Polyp's sequential AI states.
/// Its values are arbitrary.
/// </summary>
public enum PolypAction
{
    /// <summary>Waits for Samus to come closer than 64 pixels on both axes, then shoots a rock on the next update.</summary>
    WaitingForSamus,
    /// <summary>Consumes separate shared-RNG samples for horizontal velocity, the rock's first place on the speed curve, and cooldown; requests one lava rock and enters cooldown even if the projectile pool is full.</summary>
    ShootingRock,
    /// <summary>Waits out the cooldown its launch chose (<see cref="PolypLaunchDefinitions.Cooldown"/>), then rearms proximity checking.</summary>
    Cooldown,
}
