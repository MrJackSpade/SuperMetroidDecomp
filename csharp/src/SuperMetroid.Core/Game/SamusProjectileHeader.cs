namespace SuperMetroid.Core.Game;

/// <summary>The sixteen bank-$93 non-beam projectile data headers, valued by their native long address.</summary>
internal enum SamusProjectileHeader
{
    /// <summary>$93:8641 ProjectileDataTable_NonBeam_Missile: damage header identity.</summary>
    Missile = 0x938641,
    /// <summary>$93:8657 ProjectileDataTable_NonBeam_SuperMissile: damage header identity.</summary>
    SuperMissile = 0x938657,
    /// <summary>$93:866D ProjectileDataTable_NonBeam_SuperMissileLink: damage header identity.</summary>
    SuperMissileLink = 0x93866d,
    /// <summary>$93:8671 ProjectileDataTable_NonBeam_PowerBomb: damage header identity.</summary>
    PowerBomb = 0x938671,
    /// <summary>$93:8675 ProjectileDataTable_NonBeam_Bomb: damage header identity.</summary>
    Bomb = 0x938675,
    /// <summary>$93:8679 ProjectileDataTable_NonBeam_BeamExplosion: damage header identity.</summary>
    BeamExplosion = 0x938679,
    /// <summary>$93:867D ProjectileDataTable_NonBeam_MissileExplosion: damage header identity.</summary>
    MissileExplosion = 0x93867d,
    /// <summary>$93:8681 ProjectileDataTable_NonBeam_BombExplosion: damage header identity.</summary>
    BombExplosion = 0x938681,
    /// <summary>$93:8685 ProjectileDataTable_NonBeam_PlasmaSBA: damage header identity.</summary>
    PlasmaSBA = 0x938685,
    /// <summary>$93:8689 ProjectileDataTable_NonBeam_WaveSBA: damage header identity.</summary>
    WaveSBA = 0x938689,
    /// <summary>$93:868D ProjectileDataTable_NonBeam_SpazerSBA: damage header identity.</summary>
    SpazerSBA = 0x93868d,
    /// <summary>$93:8691 ProjectileDataTable_NonBeam_SuperMissileExplosion: damage header identity.</summary>
    SuperMissileExplosion = 0x938691,
    /// <summary>$93:8695 ProjectileDataTable_NonBeam_Projectile25: damage header identity.</summary>
    Projectile25 = 0x938695,
    /// <summary>$93:86AB ProjectileDataTable_NonBeam_SpazerSBATrail: damage header identity.</summary>
    SpazerSBATrail = 0x9386ab,
    /// <summary>$93:86C1 ProjectileDataTable_NonBeam_ShinesparkEcho: damage header identity.</summary>
    ShinesparkEcho = 0x9386c1,
    /// <summary>$93:86D7 ProjectileDataTable_NonBeam_Projectile27: damage header identity.</summary>
    Projectile27 = 0x9386d7,
}
