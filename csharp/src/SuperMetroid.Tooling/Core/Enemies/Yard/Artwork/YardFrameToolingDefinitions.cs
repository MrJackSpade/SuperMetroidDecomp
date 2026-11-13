using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Yard.Artwork;

/// <summary>Development-tool members of <see cref="YardAnimations"/>; never linked by player hosts.</summary>
internal static class YardFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa3;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.Word(frame);
}
