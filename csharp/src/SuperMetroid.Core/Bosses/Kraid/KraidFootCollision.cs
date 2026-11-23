using System.Collections.Immutable;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The collision of Kraid's feet: the components of the pose shown, moved with its placement (#2210).</summary>
internal static partial class KraidFootCollision
{
    /// <summary>The two collision components of Kraid-foot pose <paramref name="pose"/>, when it shows a foot picture.</summary>
    internal static bool TryGetComponents(KraidPartPose? pose, out ImmutableArray<KraidCollisionComponent> components)
    {
        if (pose is { } shown && PictureComponents.TryGetValue(shown.Picture, out ImmutableArray<KraidCollisionComponent> unplaced))
        {
            components = [.. unplaced.Select(component => component with { Offset = component.Offset + shown.Placement })];
            return true;
        }
        components = [];
        return false;
    }

    /// <summary>Each foot picture's components where its walk first shows it, all colliding with the shared rectangle.</summary>
    private static readonly Dictionary<EnemyFrameId, ImmutableArray<KraidCollisionComponent>> PictureComponents =
        KraidFootCollisionDefinitions.ComponentOffsets.ToDictionary(
            entry => entry.Key,
            entry => entry.Value
                .Select(offset => new KraidCollisionComponent(offset, KraidFootCollisionDefinitions.SharedHitboxes))
                .ToImmutableArray());
}