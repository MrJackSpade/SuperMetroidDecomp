namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Polyp's sequential AI states.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum PolypEnemyFunction
{
    /// <summary>Function_Polyp_WaitForSamusToGetNear, $A2:B596: tests strict modular distances below 64 whole pixels on both axes, selecting the shoot function without spawning until the next AI update.</summary>
    WaitingForSamus = 1,
    /// <summary>Function_Polyp_ShootRock, $A2:B5B2: consumes separate shared-RNG samples for horizontal velocity, initial quadratic Y-speed selector, and cooldown; requests one bank-$86 lava rock and enters cooldown even if the projectile pool is full.</summary>
    ShootingRock,
    /// <summary>Function_Polyp_Cooldown, $A2:B5EA: decrements the native word once per AI update and rearms proximity checking only when the signed result is negative, so an initial nonnegative count N waits N+1 updates.</summary>
    Cooldown,
}
