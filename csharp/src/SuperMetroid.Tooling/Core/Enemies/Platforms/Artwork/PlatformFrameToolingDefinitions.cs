using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Platforms.Artwork;

/// <summary>Development-tool members of <see cref="PlatformAnimations"/>; never linked by player hosts.</summary>
internal static class PlatformFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>The spritemap <paramref name="frame"/> shows: its record in this bank (#2307).</summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.WordIn(Bank, frame);
}
