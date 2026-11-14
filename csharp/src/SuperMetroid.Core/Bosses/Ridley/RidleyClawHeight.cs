namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// How far Ridley's legs reach below him (#1747), which his body animations select and which sets how low his claws hold a
/// grabbed Samus. Its values are arbitrary.
/// </summary>
public enum RidleyClawHeight
{
    /// <summary>Standing legs.</summary>
    Standing,
    /// <summary>Half-extended legs.</summary>
    LegsHalfExtended,
    /// <summary>Extended legs.</summary>
    LegsExtended,
}
