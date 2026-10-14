namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// Literal bank-$A8 function word stored in Yapping Maw's native variable A. Keeping the
/// cartridge addresses makes a debugger trace line up with WRAM and with $A8:A211's jump
/// dispatcher without a second, invented host state numbering scheme.
/// </summary>
public enum YappingMawAiFunction : ushort
{
    /// <summary>$A8:A235, Function_YappingMaw_Neutral: samples Samus distance/angle, refreshes the grab cooldown inside 32 pixels, and starts an attack only below the population activation threshold.</summary>
    WaitingForSamus = 0xa235,
    /// <summary>$A8:A28C, Function_YappingMaw_Attack: clears the curl accumulator, derives radii and mirrored bend from the target, and installs the eight-way attack art and held-Samus offsets.</summary>
    BeginExtension = 0xa28c,
    /// <summary>$A8:A445, Function_YappingMaw_Attacking: solves all four curled link positions, advances the quadratic angular motion, carries any captive with the mouth, and enters cooldown when the signed curl becomes negative.</summary>
    ExtendingOrRetracting = 0xa445,
    /// <summary>$A8:A68A, Function_YappingMaw_Cooldown: retains captive placement until the timer seeded at 64 underflows on call 65, then releases input and returns to neutral unless Samus death still owns the lock.</summary>
    RetractedDelay = 0xa68a,
}
