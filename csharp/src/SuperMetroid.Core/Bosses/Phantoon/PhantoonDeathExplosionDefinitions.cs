using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Authored explosion choreography for Phantoon's death: the opening explosions, burst once, then the repeated explosions,
/// burst <see cref="PassCount"/> times over (#2258).
/// </summary>
public static class PhantoonDeathExplosionDefinitions
{
    /// <summary>How many times the repeated explosions burst: once after the opening explosions, then twice more.</summary>
    public const int PassCount = 3;

    /// <summary>The explosions that open his death, burst once: offsets from his body, dust animation and delay until the next.</summary>
    internal static readonly PhantoonDeathExplosionDefinition[] Opening =
    [
        new(new(0, 0), MiscDustAnimation.BigExplosion, 16),
        new(new(32, -32), MiscDustAnimation.BigExplosion, 16),
        new(new(-32, 32), MiscDustAnimation.BigExplosion, 16),
        new(new(-32, -32), MiscDustAnimation.BigExplosion, 16),
        new(new(32, 32), MiscDustAnimation.BigExplosion, 32),
    ];

    /// <summary>The explosions that follow the opening ones, burst <see cref="PassCount"/> times over, in order.</summary>
    internal static readonly PhantoonDeathExplosionDefinition[] Repeated =
    [
        new(new(-32, -8), MiscDustAnimation.BigExplosion, 8),
        new(new(0, 0), MiscDustAnimation.SmallExplosion, 8),
        new(new(32, -8), MiscDustAnimation.BigExplosion, 8),
        new(new(0, 0), MiscDustAnimation.SmallExplosion, 8),
        new(new(0, 24), MiscDustAnimation.SmallExplosion, 8),
        new(new(0, 48), MiscDustAnimation.BigExplosion, 8),
        new(new(-24, 24), MiscDustAnimation.SmallExplosion, 8),
        new(new(24, 24), MiscDustAnimation.SmallExplosion, 8),
    ];
}
