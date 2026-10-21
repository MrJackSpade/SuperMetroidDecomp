namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A trail selector table: a projectile's kind picks one, and its beam combination picks the row.</summary>
internal enum ProjectileTrailFamilyGroup
{
    /// <summary>Uncharged beams, missiles and Super Missiles.</summary>
    Uncharged,

    /// <summary>Charged beams.</summary>
    Charged,

    /// <summary>Special beam attacks.</summary>
    SpecialBeamAttack,
}
