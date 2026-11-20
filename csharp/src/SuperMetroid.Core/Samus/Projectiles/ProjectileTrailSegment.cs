namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A projectile-trail animation one side of a trail runs.</summary>
public enum ProjectileTrailSegment
{
    /// <summary>The left ice sparkle, also drawn by a charged Power Beam.</summary>
    LeftIce,

    /// <summary>The right ice sparkle.</summary>
    RightIce,

    /// <summary>The wave trail, shared by both sides.</summary>
    Wave,

    /// <summary>The missile trail, shared by missiles and Super Missiles.</summary>
    Missile,
}
