using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Finds the physical components of Crocomire's corpse for the frame it shows.</summary>
internal static partial class CrocomireCorpseCollision
{
    /// <summary>Whether <paramref name="frame"/> is one of the corpse frames.</summary>
    internal static bool HasFrame(EnemyFrameId? frame) => frame is { } shown && CrocomireCorpseCollisionDefinitions.Frames.ContainsKey(shown);

    /// <summary>Returns where a corpse frame places its collision components.</summary>
    internal static ReadOnlySpan<EnemyCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is not { } shown ? []
            : CrocomireCorpseCollisionDefinitions.Frames.TryGetValue(shown, out var parts) ? parts
            : throw new InvalidDataException($"Unknown Crocomire corpse frame {frame}.");
}
