using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.WorkRobots.Artwork;

/// <summary>Development-tool members of <see cref="WorkRobotAnimations"/>; never linked by player hosts.</summary>
internal static class WorkRobotFrameToolingDefinitions
{
    /// <summary>The bank holding the spritemaps.</summary>
    internal const byte Bank = 0xa8;
    /// <summary>The spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(EnemyFrameId frame) => EnemyFrameCartridgeLookupTooling.Word(frame);
}
