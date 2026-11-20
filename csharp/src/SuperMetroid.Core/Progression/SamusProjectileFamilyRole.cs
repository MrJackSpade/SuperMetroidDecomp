namespace SuperMetroid.Core.Progression;

/// <summary>What a family of Samus's projectiles is to the rules that test it (#2556).</summary>
internal enum SamusProjectileFamilyRole
{
    /// <summary>A fired shot: a beam, missile or Super Missile.</summary>
    FiredShot,

    /// <summary>A bomb or a Power Bomb.</summary>
    Bomb,

    /// <summary>An explosion a shot left behind.</summary>
    Explosion,
}
