using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Hibashi.Artwork;

/// <summary>Development-tool members of <see cref="HibashiAnimations"/>; never linked by player hosts.</summary>
internal static class HibashiFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.Word(frame);
}
