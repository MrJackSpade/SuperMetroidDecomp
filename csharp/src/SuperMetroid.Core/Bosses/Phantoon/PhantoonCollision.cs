using SuperMetroid.Core.Enemies;
using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.Phantoon.PhantoonCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Finds the collision components of the Phantoon frame shown.</summary>
internal static class PhantoonCollision
{
    /// <summary>
    /// The collision components of Phantoon frame <paramref name="frame"/>: his body's picture has the components of
    /// <paramref name="bodyHitArea"/>, the hit area his fight has live; his other parts' looks have none.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Phantoon's
    /// pictures.</exception>
    internal static ImmutableArray<EnemyCollisionComponent> ComponentsAt(EnemyFrameId? frame, PhantoonBodyHitArea bodyHitArea) =>
        frame == EnemyFrameId.PhantoonBody
            ? HitAreaComponents[bodyHitArea]
            : frame is { } part && PartsWithoutHitboxes.Contains(part)
                ? NoComponents
                : throw new InvalidDataException($"Phantoon frame {frame} has no hitbox identity.");
}
