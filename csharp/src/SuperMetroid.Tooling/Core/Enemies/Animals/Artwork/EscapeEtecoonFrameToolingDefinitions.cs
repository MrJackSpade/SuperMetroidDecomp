using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Animals.Artwork;

/// <summary>Development-tool members of <see cref="EscapeEtecoonAnimations"/>; never linked by player hosts.</summary>
internal static class EscapeEtecoonFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xb3;
    /// <summary>The spritemap <paramref name="frame"/> shows: its record in this bank, which stores the Etecoon's strides again (#2327).</summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.WordIn(Bank, frame);
}
