using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Authored explosion choreography for Phantoon's death sequence.</summary>
public static class PhantoonDeathExplosionDefinitions
{
    /// <summary>$A7:D9FF: thirteen four-byte position/type/delay records begin at $DA1D.</summary>
    public const int Count = 13;
    /// <summary>$A7:DA04: repeat from record five after the initial pass.</summary>
    public const int RepeatStart = 5;
    /// <summary>$A7:DA11: finish after three trips through the end of the record table.</summary>
    public const int PassCount = 3;

    /// <summary>$A7:DA1D..DA50: signed X/Y offsets, misc-dust explosion animation, and delay until the next request.</summary>
    internal static readonly PhantoonDeathExplosionDefinition[] Records =
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
