using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.Draygon.DraygonCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Finds the collision components of the Draygon frame shown.</summary>
internal static class DraygonCollision
{
    /// <summary>The collision components of Draygon frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Draygon's frames.</exception>
    internal static ImmutableArray<DraygonCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && FrameComponents.TryGetValue(shown, out ImmutableArray<DraygonCollisionComponent> components)
            ? components
            : throw new InvalidDataException($"Draygon frame {frame} has no compiled collision identity.");
}
