using SuperMetroid.Core.Enemies;
using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.Phantoon.PhantoonCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Finds the collision components of the Phantoon frame shown.</summary>
internal static partial class PhantoonCollision
{
    /// <summary>
    /// The collision components of Phantoon frame <paramref name="frame"/>: his body's picture has the components of
    /// <paramref name="bodyHitArea"/>, the hit area his fight has live; his other parts' looks have none.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Phantoon's
    /// pictures.</exception>
    internal static ImmutableArray<EnemyCollisionComponent> ComponentsAt(EnemyFrameId? frame, PhantoonBodyHitArea bodyHitArea) =>
        frame is not { } shown ? []
            : shown == EnemyFrameId.PhantoonBody ? HitAreaComponents[bodyHitArea]
            : PartsWithoutHitboxes.Contains(shown) ? NoComponents
            : throw new InvalidDataException($"Phantoon frame {frame} has no hitbox identity.");
}
