using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.Kraid.KraidArmCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The collision of Kraid's arm: each rectangle's reactions, and the components of the frame shown.</summary>
internal static class KraidArmCollision
{
    /// <summary>The components of Kraid-arm frame <paramref name="frame"/>, when it is one.</summary>
    internal static bool TryGetComponents(EnemyFrameId? frame, out ImmutableArray<KraidCollisionComponent> components)
    {
        if (frame is { } shown && FrameComponentHitboxes.TryGetValue(shown, out components))
            return true;
        components = [];
        return false;
    }

    /// <summary>
    /// The rectangles of one of Kraid's arm hitbox lists, in collision order; the first overlapping rectangle wins.
    /// Each reacts to touch as its list does, and to a shot as the arm.
    /// </summary>
    internal static ImmutableArray<EnemyCollisionHitbox> HitboxesAt(KraidArmHitboxList list) =>
        HitboxLists.TryGetValue(list, out ImmutableArray<EnemyCollisionHitbox> rectangles)
            ? rectangles
            : throw new InvalidDataException($"Kraid-arm hitbox list {list} is not compiled.");

    /// <summary>Each compiled hitbox list's rectangles in collision order, with their touch and shot reactions.</summary>
    private static readonly Dictionary<KraidArmHitboxList, ImmutableArray<EnemyCollisionHitbox>> HitboxLists = HitboxListShapes.ToDictionary(
        entry => entry.Key,
        entry => entry.Value.Rectangles
            .Select(shape => new EnemyCollisionHitbox(shape.Left, shape.Top, shape.Right, shape.Bottom, entry.Value.Touch, KraidReactions.ArmShot))
            .ToImmutableArray());

    /// <summary>Each arm frame's components in collision order, each with its rectangles.</summary>
    private static readonly Dictionary<EnemyFrameId, ImmutableArray<KraidCollisionComponent>> FrameComponentHitboxes = FrameComponents.ToDictionary(
        entry => entry.Key,
        entry => entry.Value
            .Select(component => new KraidCollisionComponent(component.Offset, HitboxesAt(component.Hitboxes)))
            .ToImmutableArray());
}
