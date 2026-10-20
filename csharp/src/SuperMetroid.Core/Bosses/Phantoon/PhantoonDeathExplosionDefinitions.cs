using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Authored explosion choreography for Phantoon's death sequence.</summary>
public static class PhantoonDeathExplosionDefinitions
{
    /// <summary>$A7:D9FF: thirteen four-byte position/type/delay records begin at $DA1D.</summary>
    public const int Count = 13;
    /// <summary>$A7:DA04: repeat from record five after the initial pass.</summary>
    public const ushort RepeatStart = 5;
    /// <summary>$A7:DA11: finish after three trips through the end of the record table.</summary>
    public const ushort PassCount = 3;

    /// <summary>$A7:DA1D..DA50: signed X/Y offsets, misc-dust explosion animation, and delay until the next request.</summary>
    public static PhantoonDeathExplosionDefinition Read(int index) => index switch
    {
        0 => new(new(0, 0), MiscDustAnimation.BigExplosion, 16),
        1 => new(new(32, -32), MiscDustAnimation.BigExplosion, 16),
        2 => new(new(-32, 32), MiscDustAnimation.BigExplosion, 16),
        3 => new(new(-32, -32), MiscDustAnimation.BigExplosion, 16),
        4 => new(new(32, 32), MiscDustAnimation.BigExplosion, 32),
        5 => new(new(-32, -8), MiscDustAnimation.BigExplosion, 8),
        6 => new(new(0, 0), MiscDustAnimation.SmallExplosion, 8),
        7 => new(new(32, -8), MiscDustAnimation.BigExplosion, 8),
        8 => new(new(0, 0), MiscDustAnimation.SmallExplosion, 8),
        9 => new(new(0, 24), MiscDustAnimation.SmallExplosion, 8),
        10 => new(new(0, 48), MiscDustAnimation.BigExplosion, 8),
        11 => new(new(-24, 24), MiscDustAnimation.SmallExplosion, 8),
        12 => new(new(24, 24), MiscDustAnimation.SmallExplosion, 8),
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}
