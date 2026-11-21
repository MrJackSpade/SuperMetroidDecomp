using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>The enemy frames that draw nothing (<see cref="CommonEnemyEmptyFrames"/>).</summary>
internal static class CommonEnemyEmptyFrameDefinitions
{
    /// <summary>
    /// The frames that draw nothing: the empty frame, whose one hitbox is the single point at its origin, which an enemy
    /// colliding by its frames' hitboxes still collides at while it shows it.
    /// </summary>
    internal static readonly EnemyFrameId[] EmptyFrames = [EnemyFrameId.PointAtOrigin];
}
