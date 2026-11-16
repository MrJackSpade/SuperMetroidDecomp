using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Alcoon;

namespace SuperMetroid.Core.Enemies.Alcoon.Artwork;

/// <summary>Development-tool members of <see cref="AlcoonFireballAnimations"/>; never linked by player hosts.</summary>
internal static class AlcoonFireballFrameToolingDefinitions
{
    /// <summary>The frames the family's animations show, in the order they first show them.</summary>
    internal static readonly EnemyProjectileFrameId[] Frames = [.. AlcoonFireballAnimations.Segments.Frames];

    /// <summary>
    /// The bank-$8D spritemap the native operand of <paramref name="frame"/> selects, for artwork catalogs that resolve
    /// the frame through its spritemap rather than an installed composition.
    /// </summary>
    internal static ushort Spritemap(EnemyProjectileFrameId frame) => Spritemaps.TryGetValue(frame, out ushort spritemap)
        ? spritemap
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Alcoon fireball pose.");

    /// <summary>The bank-$8D spritemap each frame's native operand selects.</summary>
    private static readonly Dictionary<EnemyProjectileFrameId, ushort> Spritemaps = new()
    {
        [EnemyProjectileFrameId.AlcoonFireballInitialFirst] = 0x8404,
        [EnemyProjectileFrameId.AlcoonFireballInitialSecond] = 0x840b,
        [EnemyProjectileFrameId.AlcoonFireballInitialThird] = 0x8412,
        [EnemyProjectileFrameId.AlcoonFireballInitialFourth] = 0x8419,
    };

    /// <summary>The frame whose program frame is stored at bank-$86 operand <paramref name="key"/>, if one is.</summary>
    internal static bool TryFrame(ushort key, out EnemyProjectileFrameId frame)
    {
        foreach (EnemyProjectileFrameId candidate in Frames)
        {
            frame = candidate;
            if (EnemyProjectileProgramFrameSources.Operand(frame) == key)
                return true;
        }
        frame = default;
        return false;
    }
}
