using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Authored explosion choreography for Phantoon's death sequence.</summary>
public static class PhantoonDeathExplosionDefinitions
{
    /// <summary>Thirteen explosions, each with a position, a type and a delay.</summary>
    public const int Count = 13;
    /// <summary>After the first pass, each pass starts again from the sixth explosion.</summary>
    public const int RepeatStart = 5;
    /// <summary>Finish after three trips through the end of the explosion list.</summary>
    public const int PassCount = 3;

    /// <summary>Each explosion's offset from Phantoon, its animation, and the delay until the next.</summary>
    internal static readonly PhantoonDeathExplosionDefinition[] Explosions =
    [
        new(new(0, 0), MiscDustAnimation.BigExplosion, 16),
        new(new(32, -32), MiscDustAnimation.BigExplosion, 16),
        new(new(-32, 32), MiscDustAnimation.BigExplosion, 16),
        new(new(-32, -32), MiscDustAnimation.BigExplosion, 16),
        new(new(32, 32), MiscDustAnimation.BigExplosion, 32),
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
