using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.Phantoon.PhantoonCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Finds the collision components of the Phantoon frame shown.</summary>
internal static class PhantoonCollision
{
    /// <summary>The collision components of Phantoon frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Phantoon's BG2 frames.</exception>
    internal static ImmutableArray<PhantoonCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && FrameComponents.TryGetValue(shown, out ImmutableArray<PhantoonCollisionComponent> components)
            ? components
            : throw new InvalidDataException($"Phantoon frame {frame} has no compiled hitbox identity.");
}
