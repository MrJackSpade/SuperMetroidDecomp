namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Allocation and sleeping-list phases of the power-bomb explosion, plus Crystal Flash.</summary>
public enum PowerBombExplosionPhase
{
    /// <summary>No active window/color animation; an armed projectile may still be waiting for its fuse.</summary>
    Inactive,
    /// <summary>FrameBehavior_PowerBombExplosion_1_PreExplosion_White: expands the non-damaging 8.8 flash radius with decreasing speed, up to 37376.</summary>
    PreExplosionWhite,
    /// <summary>FrameBehavior_PowerBombExplosion_2_PreExplosion_Yellow: advances the pre-scaled 192-byte lead-in shapes before initializing the damaging radius.</summary>
    PreExplosionYellow,
    /// <summary>FrameBehavior_PowerBombExplosion_3_Explosion_Yellow: accelerates the damaging 8.8 radius while scaling the yellow explosion window.</summary>
    ExplosionYellow,
    /// <summary>FrameBehavior_PowerBombExplosion_4_Explosion_White: consumes seventeen pre-scaled expanding shapes, retaining the final window for the fade.</summary>
    ExplosionWhite,
    /// <summary>FrameBehavior_PowerBombExplosion_5_AfterGlow: fades fixed-color components through the 32-step countdown before cleanup and a possible Crystal Flash handoff.</summary>
    Afterglow,
    /// <summary>FrameBehavior_CrystalFlash_1_Explosion: reuses the yellow expansion curve only up to 8.8 radius 8192, without arming another Power Bomb.</summary>
    CrystalFlashExplosion,
    /// <summary>FrameBehavior_CrystalFlash_2_AfterGlow: retains the small final window while colors fade, then clears the shared explosion state.</summary>
    CrystalFlashAfterglow,
    /// <summary>Started explosion awaiting its first effect update and its pre-explosion setup.</summary>
    PendingPreExplosionSetup,
}
