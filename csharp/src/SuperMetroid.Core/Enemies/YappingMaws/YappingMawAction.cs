namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The behaviour Yapping Maw's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum YappingMawAction
{
    /// <summary>Samples Samus distance/angle, refreshes the grab cooldown inside 32 pixels, and starts an attack only below the population activation threshold.</summary>
    WaitingForSamus = 1,
    /// <summary>Clears the curl accumulator, derives radii and mirrored bend from the target, and installs the eight-way attack art and held-Samus offsets.</summary>
    BeginExtension,
    /// <summary>Solves all four curled link positions, advances the quadratic angular motion, carries any captive with the mouth, and enters cooldown when the signed curl becomes negative.</summary>
    ExtendingOrRetracting,
    /// <summary>Retains captive placement until the timer seeded at 64 passes zero on call 65, then releases input and returns to neutral unless Samus death still owns the lock.</summary>
    RetractedDelay,
}
