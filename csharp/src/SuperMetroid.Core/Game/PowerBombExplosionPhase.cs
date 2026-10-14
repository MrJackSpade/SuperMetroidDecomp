namespace SuperMetroid.Core.Game;

/// <summary>Allocation and sleeping-list phases beginning at ROM $88:8ACE, plus Crystal Flash.</summary>
public enum PowerBombExplosionPhase
{
    /// <summary>No active bank-$88 window/color animation; an armed projectile may still be waiting for its fuse.</summary>
    Inactive,
    /// <summary>$88:90DF, PreInstruction_PowerBombExplosion_1_PreExplosion_White: expands the non-damaging 8.8 flash radius with decreasing speed, up to $9200.</summary>
    PreExplosionWhite,
    /// <summary>$88:91A8, PreInstruction_PowerBombExplosion_2_PreExplosion_Yellow: advances the pre-scaled 192-byte lead-in shapes before initializing the damaging radius.</summary>
    PreExplosionYellow,
    /// <summary>$88:8DE9, PreInstruction_PowerBombExplosion_3_Explosion_Yellow: accelerates the damaging 8.8 radius while scaling the yellow explosion window.</summary>
    ExplosionYellow,
    /// <summary>$88:8EB2, PreInstruction_PowerBombExplosion_4_Explosion_White: consumes seventeen pre-scaled expanding shapes, retaining the final window for the fade.</summary>
    ExplosionWhite,
    /// <summary>$88:8B98, PreInstruction_PowerBombExplosion_5_AfterGlow: fades fixed-color components through the 32-step countdown before cleanup and a possible Crystal Flash handoff.</summary>
    Afterglow,
    /// <summary>$88:A552, PreInstruction_CrystalFlash_1_Explosion: reuses the yellow expansion curve only up to 8.8 radius $2000, without arming another Power Bomb.</summary>
    CrystalFlashExplosion,
    /// <summary>$88:A35D, PreInstruction_CrystalFlash_2_AfterGlow: retains the small final window while colors fade, then $88:A317 clears the shared explosion state.</summary>
    CrystalFlashAfterglow,
    /// <summary>Allocated $88:8ACE list awaiting its first HDMA pass and $88:8B14 setup.</summary>
    /// <remarks>Appended to preserve existing debugger-state enum identities.</remarks>
    PendingPreExplosionSetup,
}
