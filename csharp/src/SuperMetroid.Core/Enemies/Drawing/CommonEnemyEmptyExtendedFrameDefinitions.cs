using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Shared visually empty extended frame at bank-local $804F. The supported
/// enemy banks carry the same one-component, point-hitbox record. Its native
/// callbacks are the common $A0:8023 touch and $A0:802D shot routines.
/// </summary>
internal static class CommonEnemyEmptyExtendedFrameDefinitions
{
    /// <summary>
    /// The frames that show the shared empty spritemap: the common empty frame, and the Norfair lava jumper frame
    /// catalogued at the same record.
    /// </summary>
    internal static readonly HashSet<EnemyFrameId> EmptySpritemapFrames = [EnemyFrameId.Empty, EnemyFrameId.NorfairLavaJumperPose0];
}
