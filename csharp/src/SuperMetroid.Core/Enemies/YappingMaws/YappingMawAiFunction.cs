namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The routine Yapping Maw's main AI runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum YappingMawAiFunction
{
    /// <summary>$A8:A235, Function_YappingMaw_Neutral: samples Samus distance/angle, refreshes the grab cooldown inside 32 pixels, and starts an attack only below the population activation threshold.</summary>
    WaitingForSamus = 1,
    /// <summary>$A8:A28C, Function_YappingMaw_Attack: clears the curl accumulator, derives radii and mirrored bend from the target, and installs the eight-way attack art and held-Samus offsets.</summary>
    BeginExtension,
    /// <summary>$A8:A445, Function_YappingMaw_Attacking: solves all four curled link positions, advances the quadratic angular motion, carries any captive with the mouth, and enters cooldown when the signed curl becomes negative.</summary>
    ExtendingOrRetracting,
    /// <summary>$A8:A68A, Function_YappingMaw_Cooldown: retains captive placement until the timer seeded at 64 underflows on call 65, then releases input and returns to neutral unless Samus death still owns the lock.</summary>
    RetractedDelay,
}
