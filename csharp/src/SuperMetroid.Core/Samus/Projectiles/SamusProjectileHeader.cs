namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A Samus projectile header: the damage and direction programs one projectile kind starts with.</summary>
internal enum SamusProjectileHeader
{
    /// <summary>The uncharged Power beam's projectile header.</summary>
    UnchargedPower,
    /// <summary>The uncharged Spazer beam's projectile header.</summary>
    UnchargedSpazer,
    /// <summary>The uncharged Spazer-Ice beam's projectile header.</summary>
    UnchargedSpazerIce,
    /// <summary>The uncharged Spazer-Ice-Wave beam's projectile header.</summary>
    UnchargedSpazerIceWave,
    /// <summary>The uncharged Plasma-Ice-Wave beam's projectile header.</summary>
    UnchargedPlasmaIceWave,
    /// <summary>The uncharged Ice beam's projectile header.</summary>
    UnchargedIce,
    /// <summary>The uncharged Wave beam's projectile header.</summary>
    UnchargedWave,
    /// <summary>The uncharged Plasma beam's projectile header.</summary>
    UnchargedPlasma,
    /// <summary>The uncharged Ice-Wave beam's projectile header.</summary>
    UnchargedIceWave,
    /// <summary>The uncharged Spazer-Wave beam's projectile header.</summary>
    UnchargedSpazerWave,
    /// <summary>The uncharged Plasma-Wave beam's projectile header.</summary>
    UnchargedPlasmaWave,
    /// <summary>The uncharged Plasma-Ice beam's projectile header.</summary>
    UnchargedPlasmaIce,
    /// <summary>The charged Power beam's projectile header.</summary>
    ChargedPower,
    /// <summary>The charged Spazer beam's projectile header.</summary>
    ChargedSpazer,
    /// <summary>The charged Spazer-Ice beam's projectile header.</summary>
    ChargedSpazerIce,
    /// <summary>The charged Spazer-Ice-Wave beam's projectile header.</summary>
    ChargedSpazerIceWave,
    /// <summary>The charged Plasma-Ice-Wave beam's projectile header.</summary>
    ChargedPlasmaIceWave,
    /// <summary>The charged Ice beam's projectile header.</summary>
    ChargedIce,
    /// <summary>The charged Plasma beam's projectile header.</summary>
    ChargedPlasma,
    /// <summary>The charged Wave beam's projectile header.</summary>
    ChargedWave,
    /// <summary>The charged Ice-Wave beam's projectile header.</summary>
    ChargedIceWave,
    /// <summary>The charged Spazer-Wave beam's projectile header.</summary>
    ChargedSpazerWave,
    /// <summary>The charged Plasma-Ice beam's projectile header.</summary>
    ChargedPlasmaIce,
    /// <summary>The charged Plasma-Wave beam's projectile header.</summary>
    ChargedPlasmaWave,
    /// <summary>The missile's projectile header.</summary>
    Missile,
    /// <summary>The Super Missile's projectile header.</summary>
    SuperMissile,
    /// <summary>The Super Missile collision link's projectile header.</summary>
    SuperMissileLink,
    /// <summary>The Power Bomb's projectile header.</summary>
    PowerBomb,
    /// <summary>The bomb's projectile header.</summary>
    Bomb,
    /// <summary>The Plasma special beam attack's projectile header.</summary>
    PlasmaSBA,
    /// <summary>The Wave special beam attack's projectile header.</summary>
    WaveSBA,
    /// <summary>The Spazer special beam attack's projectile header.</summary>
    SpazerSBA,
    /// <summary>The Spazer special beam attack trail's projectile header.</summary>
    SpazerSBATrail,
    /// <summary>The shinespark echo's projectile header.</summary>
    ShinesparkEcho,
}
