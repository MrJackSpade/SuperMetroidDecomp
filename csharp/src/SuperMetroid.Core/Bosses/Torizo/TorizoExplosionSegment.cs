namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The animation segments of the Torizos' explosions and the bursts of a sonic boom that has struck a wall, which return to where they burst (#641, #1788, #2235, #2554): each animation begins at the
/// segment named for what it shows; the runs it repeats, loops on or moves on to are segments of their own
/// (<see cref="TorizoExplosionAnimations"/>).
/// </summary>
internal enum TorizoExplosionSegment
{
    /// <summary>A low-health explosion: it moves in front of Samus, harmless, then three times returns to its origin, scatters and plays its five poses, then is deleted.</summary>
    LowHealthExplosion,
    /// <summary>A low-health explosion's burst: it returns to its origin, scatters and plays its five poses, three times.</summary>
    LowHealthExplosionBurst,
    /// <summary>A death explosion: one time in four it puffs into <see cref="DeathSmoke"/> instead; otherwise it twice returns to its origin, scatters and plays its six poses, then is deleted.</summary>
    DeathExplosion,
    /// <summary>A death explosion's burst: it returns to its origin, scatters and plays its six poses, twice.</summary>
    DeathExplosionBurst,
    /// <summary>A death explosion's smoke puff: it twice returns to its origin, scatters and plays its four poses, then is deleted.</summary>
    DeathSmoke,
    /// <summary>A sonic boom bursting on a wall: it stops, turns harmless and moves in front, then five times returns to its origin, scatters and plays its five poses, then is deleted.</summary>
    SonicBoomWallImpact,
    /// <summary>A sonic boom's burst on a wall: it returns to its origin, scatters and plays its five poses, five times.</summary>
    SonicBoomWallImpactBurst,
    /// <summary>A projectile deleting itself: the end of an explosion's or a burst's repeated run.</summary>
    Deletion,
}
