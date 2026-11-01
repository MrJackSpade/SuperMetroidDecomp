namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A table that selects a projectile header by weapon or projectile kind.</summary>
internal enum SamusProjectileSelectorTable
{
    /// <summary>Uncharged beams, by equipment combination.</summary>
    UnchargedBeam,
    /// <summary>Charged beams, by equipment combination.</summary>
    ChargedBeam,
    /// <summary>Missiles, bombs and explosions, by projectile kind.</summary>
    NonBeam,
    /// <summary>The Shinespark echo and Spazer special-attack trail, by projectile kind.</summary>
    EchoAndTrail,
    /// <summary>Special attacks, by equipment combination.</summary>
    SpecialAttack,
    /// <summary>The Super Missile link.</summary>
    SuperMissileLink,
}
