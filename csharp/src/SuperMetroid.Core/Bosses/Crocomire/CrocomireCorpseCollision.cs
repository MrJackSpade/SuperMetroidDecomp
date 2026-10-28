using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Finds the physical components of Crocomire's corpse for the frame it shows.</summary>
internal static class CrocomireCorpseCollision
{
    /// <summary>Whether <paramref name="frame"/> is one of the corpse frames.</summary>
    internal static bool HasFrame(EnemyFrameId? frame) => frame is { } shown && CrocomireCorpseCollisionDefinitions.Frames.ContainsKey(shown);

    /// <summary>Returns physical component placement for a compiled corpse frame.</summary>
    internal static ReadOnlySpan<CrocomireBodyCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && CrocomireCorpseCollisionDefinitions.Frames.TryGetValue(shown, out var parts)
            ? parts
            : throw new InvalidDataException($"Unknown Crocomire corpse frame {frame}.");
}
