namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How Ceres Ridley's fight opens, ends and shows its hits (#2232): how long his reveal waits and holds his roar, the hits
/// that drive him off, the energy at which he gives up on Samus, the hits his health shades start at and how long each hit
/// flashes.
/// </summary>
internal static class CeresRidleyFightDefinitions
{
    /// <summary>The accepted hits that drive Ceres Ridley into his fake retreat; he never loses ordinary health.</summary>
    internal const int HitsToDriveOff = 100;

    /// <summary>The updates Ceres Ridley waits once Samus's door has finished; on the last his eyes begin to fade in.</summary>
    internal const int RevealDelayUpdates = 513;

    /// <summary>The updates Ceres Ridley holds his opening roar; on the last he takes off.</summary>
    internal const int RoarHoldUpdates = 253;

    /// <summary>
    /// Samus's energy below which Ceres Ridley gives up on her: his hover turns to his real retreat and his fireballing
    /// is cut short.
    /// </summary>
    internal const int SamusLowEnergy = 30;

    /// <summary>The updates a hit flashes him for.</summary>
    internal const int HurtFlash = 13;

    /// <summary>
    /// The updates a hit landing on an odd count of a running flash flashes him for: one longer, so the flash keeps its
    /// place in the alternation.
    /// </summary>
    internal const int HurtFlashKeepingPhase = 14;
}
