using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Converts projectile behaviours and depth to the property-word bits the cartridge stores (<see cref="EnemyProjectilePropertyBitDefinitions"/>).</summary>
internal static class EnemyProjectilePropertyWords
{
    /// <summary>The property-word bits <paramref name="flags"/> occupy.</summary>
    internal static ushort ToNativeWord(EnemyProjectilePropertyFlags flags)
    {
        ushort word = 0;
        foreach ((EnemyProjectilePropertyFlags flag, ushort bit) in EnemyProjectilePropertyBitDefinitions.Bits)
        {
            if (flags.HasFlag(flag))
                word |= bit;
        }
        return word;
    }

    /// <summary>The property-word bit of <paramref name="depth"/>.</summary>
    internal static ushort ToNativeWord(EnemyProjectileDepth depth) => EnemyProjectilePropertyBitDefinitions.DepthBits[depth];
}
