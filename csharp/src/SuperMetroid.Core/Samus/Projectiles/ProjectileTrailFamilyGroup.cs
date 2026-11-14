namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The group of trail families a projectile's kind chooses from; its beam combination chooses the family.</summary>
internal enum ProjectileTrailFamilyGroup
{
    /// <summary>Uncharged beams, missiles and Super Missiles.</summary>
    Uncharged,

    /// <summary>Charged beams.</summary>
    Charged,

    /// <summary>Special beam attacks.</summary>
    SpecialBeamAttack,
}
