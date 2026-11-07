namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Per-projectile phase word. Only the
/// Spazer special beam attack uses it, as the offset into the phase table.
/// </summary>
public enum SpazerComboPhase
{
    /// <summary>Phase 0: circling Samus; also the word's cleared value.</summary>
    Circling = 0,

    /// <summary>Phase 2: flying up towards the convergence point.</summary>
    FlyingUpTowardsPoint = 2,

    /// <summary>Phase 4: flying up away from the convergence point.</summary>
    FlyingUpAwayFromPoint = 4,
}
