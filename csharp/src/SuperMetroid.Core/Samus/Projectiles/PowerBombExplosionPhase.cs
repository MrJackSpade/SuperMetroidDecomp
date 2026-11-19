namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Allocation and resting phases of the power-bomb explosion, plus Crystal Flash.</summary>
public enum PowerBombExplosionPhase
{
    /// <summary>No explosion is running; an armed bomb may still be waiting for its fuse.</summary>
    Inactive,
    /// <summary>
    /// The white pre-explosion: the harmless flash grows with decreasing speed to
    /// <see cref="Abilities.PowerBombExplosionDefinitions.PreExplosionWhiteLimit"/>.
    /// </summary>
    PreExplosionWhite,
    /// <summary>The yellow pre-explosion: advances the pre-scaled lead-in shapes before initializing the damaging radius.</summary>
    PreExplosionYellow,
    /// <summary>The yellow explosion: the damaging radius accelerates while the yellow blast grows with it.</summary>
    ExplosionYellow,
    /// <summary>
    /// The white explosion: runs its pre-scaled expanding shapes, one an update for
    /// <see cref="Abilities.PowerBombExplosionDefinitions.ExplosionWhiteUpdates"/>, keeping the last for the fade.
    /// </summary>
    ExplosionWhite,
    /// <summary>
    /// The afterglow: fades the glow colour's components through its <see cref="Abilities.PowerBombExplosionDefinitions.AfterglowSteps"/>
    /// steps before cleanup and a possible Crystal Flash handoff.
    /// </summary>
    Afterglow,
    /// <summary>
    /// The Crystal Flash explosion: reuses the yellow expansion curve only up to
    /// <see cref="Abilities.PowerBombExplosionDefinitions.CrystalFlashRadiusLimit"/>, without arming another Power Bomb.
    /// </summary>
    CrystalFlashExplosion,
    /// <summary>The Crystal Flash afterglow: keeps the small final blast while colours fade, then clears the shared explosion state.</summary>
    CrystalFlashAfterglow,
    /// <summary>Started explosion awaiting its first effect update and its pre-explosion setup.</summary>
    PendingPreExplosionSetup,
}
