namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A bank-$93 table that selects a projectile header by weapon or projectile kind.</summary>
internal enum SamusProjectileSelectorTable
{
    /// <summary>$93:83C1: uncharged beams, by equipment combination.</summary>
    UnchargedBeam,
    /// <summary>$93:83D9: charged beams, by equipment combination.</summary>
    ChargedBeam,
    /// <summary>$93:83F1: missiles, bombs and explosions, by projectile kind.</summary>
    NonBeam,
    /// <summary>$93:8403: the Shinespark echo and Spazer special-attack trail, by projectile kind.</summary>
    EchoAndTrail,
    /// <summary>$93:8413: special attacks, by equipment combination.</summary>
    SpecialAttack,
    /// <summary>$93:842B: the Super Missile link.</summary>
    SuperMissileLink,
}
