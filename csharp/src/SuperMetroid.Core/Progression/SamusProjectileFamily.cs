namespace SuperMetroid.Core.Progression;

/// <summary>
/// The kinds of thing Samus's weapons put into the world. Member values are arbitrary; the order the game compares
/// families in is <see cref="SuperMetroid.Core.Samus.Projectiles.SamusProjectileFamilies"/>'s, never a member's value.
/// </summary>
public enum SamusProjectileFamily
{
    /// <summary>A beam shot, charged or uncharged, including special-attack particles and echoes.</summary>
    Beam,
    /// <summary>An ordinary missile.</summary>
    Missile,
    /// <summary>A Super Missile and its invisible collision link.</summary>
    SuperMissile,
    /// <summary>A placed Power Bomb.</summary>
    PowerBomb,
    /// <summary>A Morph Ball bomb, placed or from a bomb spread.</summary>
    Bomb,
    /// <summary>The explosion a beam leaves where it struck.</summary>
    BeamExplosion,
    /// <summary>The explosion a missile or Super Missile leaves where it struck.</summary>
    MissileExplosion,
}
