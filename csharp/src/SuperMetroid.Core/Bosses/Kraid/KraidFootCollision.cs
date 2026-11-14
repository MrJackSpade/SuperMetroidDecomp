using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The collision of Kraid's feet: the components of the frame shown.</summary>
internal static class KraidFootCollision
{
    /// <summary>The two collision components of Kraid-foot frame <paramref name="frame"/>, when it is one.</summary>
    internal static bool TryGetComponents(EnemyFrameId? frame, out ImmutableArray<KraidCollisionComponent> components)
    {
        if (frame is { } shown && FrameComponents.TryGetValue(shown, out components))
            return true;
        components = [];
        return false;
    }

    /// <summary>Each foot frame's components, all colliding with the shared rectangle.</summary>
    private static readonly Dictionary<EnemyFrameId, ImmutableArray<KraidCollisionComponent>> FrameComponents =
        KraidFootCollisionDefinitions.ComponentOffsets.ToDictionary(
            entry => entry.Key,
            entry => entry.Value
                .Select(offset => new KraidCollisionComponent(offset, KraidFootCollisionDefinitions.SharedHitboxes))
                .ToImmutableArray());
}
