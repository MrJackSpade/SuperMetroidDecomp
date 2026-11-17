using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Compiled animation selectors for Mother Brain's body-relative death explosions.</summary>
internal static class MotherBrainDeathExplosionDefinitions
{
    /// <summary>The updates after a burst of smoky explosions before the next.</summary>
    internal const int SmokyIntervalUpdates = 16;

    /// <summary>The updates after a burst of mixed explosions before the next.</summary>
    internal const int MixedIntervalUpdates = 8;

    /// <summary>
    /// The groups of four body-relative explosion positions her death explosions burst at, in the order they burst
    /// (#2315), read before the random number that selects each explosion's type. The explosion's setup adds these
    /// offsets to the moving body without velocity integration; the explosion projectile has zero hitbox radii and cannot
    /// damage Samus.
    /// </summary>
    internal static IReadOnlyList<MotherBrainDeathExplosionGroup> Groups { get; } =
    [
        new(new(10, -31), new(-20, -8), new(0, 23), new(30, 61)),
        new(new(20, -41), new(4, -22), new(-20, 3), new(-27, 57)),
        new(new(-2, -33), new(10, -10), new(-14, 16), new(6, 59)),
        new(new(4, -43), new(-12, -22), new(13, -2), new(-8, 52)),
        new(new(52, -34), new(-3, -15), new(12, 19), new(25, 44)),
        new(new(17, -55), new(30, -22), new(-3, -5), new(0, 40)),
        new(new(36, -37), new(-19, -15), new(-4, 13), new(29, 25)),
    ];

    /// <summary>
    /// The shared misc-dust animation each of Mother Brain's death explosions shows: the small explosion, smoke and the big
    /// explosion.
    /// </summary>
    internal static readonly IReadOnlyDictionary<MotherBrainDeathExplosionAnimation, MiscDustAnimation> Animations =
        new Dictionary<MotherBrainDeathExplosionAnimation, MiscDustAnimation>
        {
            [MotherBrainDeathExplosionAnimation.SmallExplosion] = MiscDustAnimation.SmallExplosion,
            [MotherBrainDeathExplosionAnimation.Smoke] = MiscDustAnimation.Smoke,
            [MotherBrainDeathExplosionAnimation.BigExplosion] = MiscDustAnimation.BigExplosion,
        };
}
