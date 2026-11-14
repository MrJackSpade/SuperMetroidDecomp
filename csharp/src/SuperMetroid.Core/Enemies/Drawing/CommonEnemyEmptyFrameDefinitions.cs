using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// The empty frame every enemy family shares. An enemy showing it draws nothing; one that collides by its frames' hitboxes
/// has a single point hitbox at its origin while it shows it, whose reactions are the common touch and shot reactions.
/// </summary>
internal static class CommonEnemyEmptyFrameDefinitions
{
    /// <summary>The frames that show the empty frame: the common empty frame.</summary>
    internal static readonly HashSet<EnemyFrameId> EmptyFrames = [EnemyFrameId.Empty];
}
