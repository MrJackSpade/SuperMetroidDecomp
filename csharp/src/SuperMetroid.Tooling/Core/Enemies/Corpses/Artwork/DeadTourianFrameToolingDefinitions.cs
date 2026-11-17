using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Corpses.Artwork;

/// <summary>Development-tool members of <see cref="DeadTourianAnimations"/>; never linked by player hosts.</summary>
internal static class DeadTourianFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa9;
    /// <summary>
    /// The spritemap <paramref name="frame"/> shows: its record in this bank, whose records draw the large Sidehopper's legs
    /// the dead Sidehopper shows (#2327).
    /// </summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.WordIn(Bank, frame);
}
