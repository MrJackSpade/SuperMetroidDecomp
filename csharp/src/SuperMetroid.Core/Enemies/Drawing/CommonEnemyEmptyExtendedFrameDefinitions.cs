using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Shared visually empty extended frame. The supported
/// enemy families carry the same one-component, point-hitbox record. Its
/// callbacks are the common touch and shot reactions.
/// </summary>
internal static class CommonEnemyEmptyExtendedFrameDefinitions
{
    /// <summary>
    /// The frames that show the shared empty spritemap: the common empty frame, and the Norfair lava jumper frame
    /// catalogued at the same record.
    /// </summary>
    internal static readonly HashSet<EnemyFrameId> EmptySpritemapFrames = [EnemyFrameId.Empty, EnemyFrameId.NorfairLavaJumperPose0];
}
