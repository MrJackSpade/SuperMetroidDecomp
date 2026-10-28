using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Finds the collision components of the Spore Spawn frame shown.</summary>
internal static class SporeSpawnCollision
{
    /// <summary>Whether <paramref name="frame"/> is one of Spore Spawn's collision frames.</summary>
    internal static bool IsFrame(EnemyFrameId? frame) => frame is { } shown && FrameComponents.ContainsKey(shown);

    /// <summary>The collision components of Spore Spawn frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Spore Spawn's collision frames.</exception>
    internal static ReadOnlySpan<SporeSpawnCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && FrameComponents.TryGetValue(shown, out SporeSpawnCollisionComponent[]? components)
            ? components
            : throw new InvalidDataException($"Spore Spawn frame {frame} has no compiled collision identity.");
}
