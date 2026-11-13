using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>Development-tool members of <see cref="EscapeEtecoonAnimations"/>; never linked by player hosts.</summary>
internal static class EscapeEtecoonFrameToolingDefinitions
{
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.Word(frame);
}
