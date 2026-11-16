using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.FuneNamihe;

namespace SuperMetroid.Core.Enemies.FuneNamihe.Artwork;

/// <summary>Development-tool members of <see cref="FuneNamiheFireballAnimations"/>; never linked by player hosts.</summary>
internal static class FuneNamiheFireballFrameToolingDefinitions
{
    /// <summary>The frames the family's animations show, in the order they first show them.</summary>
    internal static readonly EnemyProjectileFrameId[] Frames = [.. FuneNamiheFireballAnimations.Segments.Frames];
    /// <summary>
    /// The bank-$8D spritemap the native operand of <paramref name="frame"/> selects, for artwork catalogs that resolve
    /// the frame through its spritemap rather than an installed composition.
    /// </summary>
    internal static ushort Spritemap(EnemyProjectileFrameId frame) => Spritemaps.TryGetValue(frame, out ushort spritemap)
        ? spritemap
        : throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Fune/Namihe fireball pose.");

    /// <summary>The bank-$8D spritemap each frame's native operand selects.</summary>
    private static readonly Dictionary<EnemyProjectileFrameId, ushort> Spritemaps = new()
    {
        [EnemyProjectileFrameId.FuneNamiheFireballLeftFirst] = 0xaab9,
        [EnemyProjectileFrameId.FuneNamiheFireballLeftSecond] = 0xaac0,
        [EnemyProjectileFrameId.FuneNamiheFireballLeftThird] = 0xaac7,
        [EnemyProjectileFrameId.FuneNamiheFireballRightFirst] = 0xaace,
        [EnemyProjectileFrameId.FuneNamiheFireballRightSecond] = 0xaad5,
        [EnemyProjectileFrameId.FuneNamiheFireballRightThird] = 0xaadc,
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
