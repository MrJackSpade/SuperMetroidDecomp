namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// What a Samus projectile is: a beam shot, missile, bomb, special attack particle or echo. The kind sets the damage the
/// projectile starts with and the program it starts in each direction it is fired in.
/// </summary>
internal enum SamusProjectileKind
{
    /// <summary>An uncharged Power beam shot.</summary>
    UnchargedPower,
    /// <summary>An uncharged Spazer beam shot.</summary>
    UnchargedSpazer,
    /// <summary>An uncharged Spazer-Ice beam shot.</summary>
    UnchargedSpazerIce,
    /// <summary>An uncharged Spazer-Ice-Wave beam shot.</summary>
    UnchargedSpazerIceWave,
    /// <summary>An uncharged Plasma-Ice-Wave beam shot.</summary>
    UnchargedPlasmaIceWave,
    /// <summary>An uncharged Ice beam shot.</summary>
    UnchargedIce,
    /// <summary>An uncharged Wave beam shot.</summary>
    UnchargedWave,
    /// <summary>An uncharged Plasma beam shot.</summary>
    UnchargedPlasma,
    /// <summary>An uncharged Ice-Wave beam shot.</summary>
    UnchargedIceWave,
    /// <summary>An uncharged Spazer-Wave beam shot.</summary>
    UnchargedSpazerWave,
    /// <summary>An uncharged Plasma-Wave beam shot.</summary>
    UnchargedPlasmaWave,
    /// <summary>An uncharged Plasma-Ice beam shot.</summary>
    UnchargedPlasmaIce,
    /// <summary>A charged Power beam shot.</summary>
    ChargedPower,
    /// <summary>A charged Spazer beam shot.</summary>
    ChargedSpazer,
    /// <summary>A charged Spazer-Ice beam shot.</summary>
    ChargedSpazerIce,
    /// <summary>A charged Spazer-Ice-Wave beam shot.</summary>
    ChargedSpazerIceWave,
    /// <summary>A charged Plasma-Ice-Wave beam shot.</summary>
    ChargedPlasmaIceWave,
    /// <summary>A charged Ice beam shot.</summary>
    ChargedIce,
    /// <summary>A charged Plasma beam shot.</summary>
    ChargedPlasma,
    /// <summary>A charged Wave beam shot.</summary>
    ChargedWave,
    /// <summary>A charged Ice-Wave beam shot.</summary>
    ChargedIceWave,
    /// <summary>A charged Spazer-Wave beam shot.</summary>
    ChargedSpazerWave,
    /// <summary>A charged Plasma-Ice beam shot.</summary>
    ChargedPlasmaIce,
    /// <summary>A charged Plasma-Wave beam shot.</summary>
    ChargedPlasmaWave,
    /// <summary>A missile.</summary>
    Missile,
    /// <summary>A Super Missile.</summary>
    SuperMissile,
    /// <summary>The collision link a Super Missile drags behind it.</summary>
    SuperMissileLink,
    /// <summary>A Power Bomb.</summary>
    PowerBomb,
    /// <summary>A morph ball bomb.</summary>
    Bomb,
    /// <summary>A particle of the Plasma special beam attack.</summary>
    PlasmaSBA,
    /// <summary>A particle of the Wave special beam attack.</summary>
    WaveSBA,
    /// <summary>A particle of the Spazer special beam attack.</summary>
    SpazerSBA,
    /// <summary>A trail of the Spazer special beam attack.</summary>
    SpazerSBATrail,
    /// <summary>A shinespark echo.</summary>
    ShinesparkEcho,
}
