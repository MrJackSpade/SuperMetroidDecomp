using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Stoke.Artwork;

/// <summary>Development-tool members of <see cref="StokeAnimations"/>; never linked by player hosts.</summary>
internal static class StokeFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa2;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.Word(frame);
}
