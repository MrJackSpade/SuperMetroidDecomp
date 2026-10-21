namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A Samus projectile header: the damage and direction programs one projectile kind starts with.</summary>
internal enum SamusProjectileHeader
{
    /// <summary>$93:8431 ProjectileDataTable_Uncharged_Power.</summary>
    UnchargedPower,
    /// <summary>$93:8447 ProjectileDataTable_Uncharged_Spazer.</summary>
    UnchargedSpazer,
    /// <summary>$93:845D ProjectileDataTable_Uncharged_SpazerIce.</summary>
    UnchargedSpazerIce,
    /// <summary>$93:8473 ProjectileDataTable_Uncharged_SpazerIceWave.</summary>
    UnchargedSpazerIceWave,
    /// <summary>$93:8489 ProjectileDataTable_Uncharged_PlasmaIceWave.</summary>
    UnchargedPlasmaIceWave,
    /// <summary>$93:849F ProjectileDataTable_Uncharged_Ice.</summary>
    UnchargedIce,
    /// <summary>$93:84B5 ProjectileDataTable_Uncharged_Wave.</summary>
    UnchargedWave,
    /// <summary>$93:84CB ProjectileDataTable_Uncharged_Plasma.</summary>
    UnchargedPlasma,
    /// <summary>$93:84E1 ProjectileDataTable_Uncharged_IceWave.</summary>
    UnchargedIceWave,
    /// <summary>$93:84F7 ProjectileDataTable_Uncharged_SpazerWave.</summary>
    UnchargedSpazerWave,
    /// <summary>$93:850D ProjectileDataTable_Uncharged_PlasmaWave.</summary>
    UnchargedPlasmaWave,
    /// <summary>$93:8523 ProjectileDataTable_Uncharged_PlasmaIce.</summary>
    UnchargedPlasmaIce,
    /// <summary>$93:8539 ProjectileDataTable_Charged_Power.</summary>
    ChargedPower,
    /// <summary>$93:854F ProjectileDataTable_Charged_Spazer.</summary>
    ChargedSpazer,
    /// <summary>$93:8565 ProjectileDataTable_Charged_SpazerIce.</summary>
    ChargedSpazerIce,
    /// <summary>$93:857B ProjectileDataTable_Charged_SpazerIceWave.</summary>
    ChargedSpazerIceWave,
    /// <summary>$93:8591 ProjectileDataTable_Charged_PlasmaIceWave.</summary>
    ChargedPlasmaIceWave,
    /// <summary>$93:85A7 ProjectileDataTable_Charged_Ice.</summary>
    ChargedIce,
    /// <summary>$93:85BD ProjectileDataTable_Charged_Plasma.</summary>
    ChargedPlasma,
    /// <summary>$93:85D3 ProjectileDataTable_Charged_Wave.</summary>
    ChargedWave,
    /// <summary>$93:85E9 ProjectileDataTable_Charged_IceWave.</summary>
    ChargedIceWave,
    /// <summary>$93:85FF ProjectileDataTable_Charged_SpazerWave.</summary>
    ChargedSpazerWave,
    /// <summary>$93:8615 ProjectileDataTable_Charged_PlasmaIce.</summary>
    ChargedPlasmaIce,
    /// <summary>$93:862B ProjectileDataTable_Charged_PlasmaWave.</summary>
    ChargedPlasmaWave,
    /// <summary>$93:8641 ProjectileDataTable_NonBeam_Missile.</summary>
    Missile,
    /// <summary>$93:8657 ProjectileDataTable_NonBeam_SuperMissile.</summary>
    SuperMissile,
    /// <summary>$93:866D ProjectileDataTable_NonBeam_SuperMissileLink.</summary>
    SuperMissileLink,
    /// <summary>$93:8671 ProjectileDataTable_NonBeam_PowerBomb.</summary>
    PowerBomb,
    /// <summary>$93:8675 ProjectileDataTable_NonBeam_Bomb.</summary>
    Bomb,
    /// <summary>$93:8679 ProjectileDataTable_NonBeam_BeamExplosion.</summary>
    BeamExplosion,
    /// <summary>$93:867D ProjectileDataTable_NonBeam_MissileExplosion.</summary>
    MissileExplosion,
    /// <summary>$93:8681 ProjectileDataTable_NonBeam_BombExplosion.</summary>
    BombExplosion,
    /// <summary>$93:8685 ProjectileDataTable_NonBeam_PlasmaSBA.</summary>
    PlasmaSBA,
    /// <summary>$93:8689 ProjectileDataTable_NonBeam_WaveSBA.</summary>
    WaveSBA,
    /// <summary>$93:868D ProjectileDataTable_NonBeam_SpazerSBA.</summary>
    SpazerSBA,
    /// <summary>$93:8691 ProjectileDataTable_NonBeam_SuperMissileExplosion.</summary>
    SuperMissileExplosion,
    /// <summary>$93:8695 ProjectileDataTable_NonBeam_Projectile25.</summary>
    Projectile25,
    /// <summary>$93:86AB ProjectileDataTable_NonBeam_SpazerSBATrail.</summary>
    SpazerSBATrail,
    /// <summary>$93:86C1 ProjectileDataTable_NonBeam_ShinesparkEcho.</summary>
    ShinesparkEcho,
    /// <summary>$93:86D7 ProjectileDataTable_NonBeam_Projectile27.</summary>
    Projectile27,
}
