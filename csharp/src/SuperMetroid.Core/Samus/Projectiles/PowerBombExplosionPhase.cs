namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Allocation and sleeping phases of the power-bomb explosion, plus Crystal Flash.</summary>
public enum PowerBombExplosionPhase
{
    /// <summary>No active window/color animation; an armed projectile may still be waiting for its fuse.</summary>
    Inactive,
    /// <summary>The white pre-explosion: expands the non-damaging 1/256-pixel flash radius with decreasing speed, up to 37376.</summary>
    PreExplosionWhite,
    /// <summary>The yellow pre-explosion: advances the pre-scaled lead-in shapes before initializing the damaging radius.</summary>
    PreExplosionYellow,
    /// <summary>The yellow explosion: accelerates the damaging 1/256-pixel radius while scaling the yellow explosion window.</summary>
    ExplosionYellow,
    /// <summary>The white explosion: consumes seventeen pre-scaled expanding shapes, retaining the final window for the fade.</summary>
    ExplosionWhite,
    /// <summary>The afterglow: fades the glow colour's components through the 32-step countdown before cleanup and a possible Crystal Flash handoff.</summary>
    Afterglow,
    /// <summary>The Crystal Flash explosion: reuses the yellow expansion curve only up to 1/256-pixel radius 8192, without arming another Power Bomb.</summary>
    CrystalFlashExplosion,
    /// <summary>The Crystal Flash afterglow: retains the small final window while colors fade, then clears the shared explosion state.</summary>
    CrystalFlashAfterglow,
    /// <summary>Started explosion awaiting its first effect update and its pre-explosion setup.</summary>
    PendingPreExplosionSetup,
}
