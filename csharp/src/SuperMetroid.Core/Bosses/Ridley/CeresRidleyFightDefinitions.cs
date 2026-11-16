namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How Ceres Ridley's fight ends and shows its hits (#2232): the hits that drive him off, the energy at which he gives up
/// on Samus, the hits his health shades start at and how long each hit flashes.
/// </summary>
internal static class CeresRidleyFightDefinitions
{
    /// <summary>The accepted hits that drive Ceres Ridley into his fake retreat; he never loses ordinary health.</summary>
    internal const int HitsToDriveOff = 100;

    /// <summary>
    /// Samus's energy below which Ceres Ridley gives up on her: his hover turns to his real retreat and his fireballing
    /// is cut short.
    /// </summary>
    internal const int SamusLowEnergy = 30;

    /// <summary>The accepted hits from which his body shows his first health shade.</summary>
    internal const int FirstShadeHits = 50;

    /// <summary>The accepted hits from which his body shows his last health shade.</summary>
    internal const int LateShadeHits = 70;

    /// <summary>The updates a hit flashes him for.</summary>
    internal const int HurtFlash = 13;

    /// <summary>
    /// The updates a hit landing on an odd count of a running flash flashes him for: one longer, so the flash keeps its
    /// place in the alternation.
    /// </summary>
    internal const int HurtFlashKeepingPhase = 14;
}
