namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One of the trail sparkles' runs of beats (#2458), which their animations play.</summary>
public enum ProjectileTrailRun
{
    /// <summary>An ice sparkle's clustered frame.</summary>
    IceClustered,

    /// <summary>An ice sparkle's twinkling frame.</summary>
    IceTwinkling,

    /// <summary>The left ice sparkle's fall of a pixel.</summary>
    LeftIceFall,

    /// <summary>The right ice sparkle's fall of a pixel.</summary>
    RightIceFall,

    /// <summary>The left ice sparkle's fall of a pixel and its scattered frame.</summary>
    LeftIceScatteredFall,

    /// <summary>The right ice sparkle's fall of a pixel and its scattered frame.</summary>
    RightIceScatteredFall,

    /// <summary>An ice sparkle's fading last frame.</summary>
    IceFading,

    /// <summary>The wave trail's ring, shared by both sides.</summary>
    Wave,

    /// <summary>The missile trail's smoke, shared by missiles and Super Missiles.</summary>
    Missile,
}
