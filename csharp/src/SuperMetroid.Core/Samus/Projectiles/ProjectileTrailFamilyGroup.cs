namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The group of trail families a shot's charge chooses from; its beam combination chooses the family. The Spazer special
/// attack's trailing pair, the one echo that leaves a trail, starts its trail from its own lists (#2348).
/// </summary>
internal enum ProjectileTrailFamilyGroup
{
    /// <summary>Uncharged beams, missiles and Super Missiles.</summary>
    Uncharged,

    /// <summary>Charged beams.</summary>
    Charged,
}
