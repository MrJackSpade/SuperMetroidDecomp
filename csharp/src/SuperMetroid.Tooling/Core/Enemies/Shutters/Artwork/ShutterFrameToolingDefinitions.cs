using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Shutters.Artwork;

/// <summary>Development-tool members of <see cref="ShutterAnimations"/>; never linked by player hosts.</summary>
internal static class ShutterFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows: its record in this bank (#2307).</summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.WordIn(Bank, frame);
}
