using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Polyp;

namespace SuperMetroid.Core.Enemies.Polyp.Artwork;

/// <summary>Development-tool members of <see cref="PolypRockAnimations"/>; never linked by player hosts.</summary>
internal static class PolypRockFrameToolingDefinitions
{
    /// <summary>The frames the family's animations show, in the order they first show them.</summary>
    internal static readonly EnemyProjectileFrameId[] Frames = [.. PolypRockAnimations.Segments.Frames];
    /// <summary>
    /// The bank-$8D spritemap the native operand of <paramref name="frame"/> selects, for artwork catalogs that resolve
    /// the frame through its spritemap rather than an installed composition.
    /// </summary>
    internal static ushort Spritemap(EnemyProjectileFrameId frame) => Spritemaps.TryGetValue(frame, out ushort spritemap)
        ? spritemap
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Polyp lava rock pose.");

    /// <summary>The bank-$8D spritemap each frame's native operand selects.</summary>
    private static readonly Dictionary<EnemyProjectileFrameId, ushort> Spritemaps = new()
    {
        [EnemyProjectileFrameId.PolypRock00] = 0x9340,
    };
    /// <summary>The frame whose installed artwork key is <paramref name="key"/>, if one is.</summary>
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
