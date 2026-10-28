using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Finds the collision components of the Mother Brain frame shown.</summary>
internal static class MotherBrainCollision
{
    /// <summary>The collision components of Mother Brain body frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Mother Brain's body frames.</exception>
    internal static ReadOnlySpan<MotherBrainCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && FrameComponents.TryGetValue(shown, out MotherBrainCollisionComponent[]? components)
            ? components
            : throw new InvalidDataException($"Mother Brain has no extended frame {frame}.");
}
