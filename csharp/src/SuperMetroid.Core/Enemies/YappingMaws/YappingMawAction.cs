namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The behaviour Yapping Maw's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum YappingMawAction
{
    /// <summary>Samples Samus distance/angle, refreshes the grab cooldown inside 32 pixels, and starts an attack only below the population activation threshold.</summary>
    WaitingForSamus,
    /// <summary>Starts the curl from rest, takes its reach and bend from where Samus is, and starts the attack animation for its eight-way aim with where it holds a caught Samus.</summary>
    BeginExtension,
    /// <summary>Solves all four curled link positions, advances the quadratic angular motion, carries any captive with the mouth, and enters cooldown when the signed curl becomes negative.</summary>
    ExtendingOrRetracting,
    /// <summary>Retains captive placement until the timer seeded at 64 passes zero on call 65, then releases input and returns to neutral unless Samus death still owns the lock.</summary>
    RetractedDelay,
}
