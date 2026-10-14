namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Per-projectile phase word at WRAM <c>$0CA4,X</c> (<c>SamusProjectile_Phases</c>). Only the
/// Spazer special beam attack uses it, as the offset into the phase table at <c>$90:DB49</c>.
/// </summary>
public enum SpazerComboPhase : ushort
{
    /// <summary>Phase 0: circling Samus (<c>$90:DB93</c>); also the word's cleared value (<c>$90:CE38</c>).</summary>
    Circling = 0,

    /// <summary>Phase 2: flying up towards the convergence point (<c>$90:DBCF</c>).</summary>
    FlyingUpTowardsPoint = 2,

    /// <summary>Phase 4: flying up away from the convergence point (<c>$90:DC30</c>).</summary>
    FlyingUpAwayFromPoint = 4,
}
