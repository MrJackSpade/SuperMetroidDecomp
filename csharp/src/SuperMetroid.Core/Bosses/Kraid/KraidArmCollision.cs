using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.Kraid.KraidArmCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The collision of Kraid's arm: each rectangle's reactions, each pose component's hitbox list and position, and the components of the frame shown.</summary>
internal static class KraidArmCollision
{
    /// <summary>Selects the arm pushback/lint reaction for active shapes, ordinary touch for background/lunge shapes.</summary>
    /// <remarks>Independently reviewed for #1165 against all selected authored
    /// rectangles. The selected lists through the last active arm frame use arm touch;
    /// the three background frames after it and both dying/lunge lists use background touch.</remarks>
    internal static EnemyReaction TouchReaction(int rectangle)
    {
        if ((uint)rectangle >= RectangleCount) throw new ArgumentOutOfRangeException(nameof(rectangle));
        return rectangle < FirstBackgroundRectangle ? EnemyReaction.KraidArmTouch : EnemyReaction.KraidBackgroundTouch;
    }

    /// <summary>Every selected arm rectangle uses the arm's shot reaction.</summary>
    internal static EnemyReaction ShotReaction(int rectangle)
    {
        if ((uint)rectangle >= RectangleCount) throw new ArgumentOutOfRangeException(nameof(rectangle));
        return EnemyReaction.KraidArmShot;
    }

    /// <summary>Combines one indexed rectangle's bounds with its arm/background touch and arm-shot reactions.</summary>
    internal static EnemyCollisionHitbox Rectangle(int index)
    {
        if ((uint)index >= RectangleCount) throw new ArgumentOutOfRangeException(nameof(index));
        EnemyHitboxRectangle geometry = Geometry[index];
        return new(geometry.Left, geometry.Top, geometry.Right, geometry.Bottom,
            TouchReaction(index), ShotReaction(index));
    }

    /// <summary>Calculates the physical hitbox identity for a normalized arm pose and component.</summary>
    /// <remarks>Independently checked for #1165 against both ten-frame groups.
    /// Components0/3 advance through seven articulated shapes with different timing.
    /// Components1/4 choose connecting shapes and component2 chooses body shapes.
    /// Poses10/11 are the single-component dying/lunge frames. Coordinates are separate.</remarks>
    internal static KraidArmHitboxList ComponentHitbox(int pose, int component)
    {
        if ((uint)pose >= 12)
            throw new ArgumentOutOfRangeException(nameof(pose));
        if ((uint)component >= (pose < 10 ? 5u : 1u))
            throw new ArgumentOutOfRangeException(nameof(component));
        if (pose >= 10) return pose == 10 ? KraidArmHitboxList.FinalPoseFirst : KraidArmHitboxList.FinalPoseSecond;
        if (component == 2)
            return pose < 2 ? KraidArmHitboxList.BodyInitial : pose < 4 ? KraidArmHitboxList.BodyMiddle : KraidArmHitboxList.BodyFinal;
        if (component is 1 or 4)
        {
            int initialPoses = component == 1 ? 2 : 3;
            return pose < initialPoses ? KraidArmHitboxList.ConnectorInitial : pose < 4 ? KraidArmHitboxList.ConnectorSecond
                : pose < 6 ? KraidArmHitboxList.ConnectorThird : KraidArmHitboxList.ConnectorFinal;
        }
        int shape = component == 0 ? (pose < 4 ? pose : 4 + (pose - 4) / 2)
            : pose < 2 ? 0 : pose < 6 ? pose - 1 : pose < 9 ? 5 : 6;
        return ArticulatedShapes[shape];
    }

    /// <summary>Constructs one physical component with the rectangles its calculated hitbox selector names.</summary>
    /// <remarks>Component2 in the twenty general/rising frames and both final
    /// single-component records are anchored at the enemy origin. All22 authored
    /// zero-coordinate pairs are independently confirmed for #1165; the remaining
    /// articulated positions have separate, unfinished review scope.</remarks>
    internal static KraidCollisionComponent Component(int pose, int component)
    {
        ImmutableArray<EnemyCollisionHitbox> hitbox = HitboxesAt(ComponentHitbox(pose, component));
        // The central body component and both single-component poses are anchored
        // at the enemy origin. Only the four articulated positions are stored.
        if (pose >= 10 || component == 2) return new(Offset16<RoomPixels>.Zero, hitbox);
        Offset16<RoomPixels>[] positions = PosePositions[pose];
        return new(positions[component < 2 ? component : component - 1], hitbox);
    }

    /// <summary>The components of Kraid-arm frame <paramref name="frame"/>, when it is one.</summary>
    internal static bool TryGetComponents(EnemyFrameId? frame, out ImmutableArray<KraidCollisionComponent> components)
    {
        if (frame is { } shown && FramePoses.TryGetValue(shown, out int pose))
        {
            components = PoseComponents[pose];
            return true;
        }
        components = [];
        return false;
    }

    /// <summary>
    /// Kraid's arm hitbox lists; the first overlapping rectangle wins.
    /// The reactions are the arm's touch or the background touch, and the arm's shot.
    /// </summary>
    /// <remarks>Independently reviewed for #1165: these cases select ordered
    /// rectangle slices for arm shapes 0..6, 8..A, F..12 and the two
    /// dying/lunge shapes. Starts are ordinals in the selected 24-record geometry
    /// domain, not byte offsets; lengths match each original count word.
    /// Shape7, B..E, foot/lint lists, record interiors and every other ushort
    /// identity remain rejected. Rectangle-edge data has a separate disposition.</remarks>
    internal static ImmutableArray<EnemyCollisionHitbox> HitboxesAt(KraidArmHitboxList list) =>
        HitboxLists.TryGetValue(list, out ImmutableArray<EnemyCollisionHitbox> rectangles)
            ? rectangles
            : throw new InvalidDataException($"Kraid-arm hitbox list {list} is not compiled.");

    /// <summary>Each compiled hitbox list's rectangles in collision order.</summary>
    private static readonly Dictionary<KraidArmHitboxList, ImmutableArray<EnemyCollisionHitbox>> HitboxLists = HitboxListSlices.ToDictionary(
        slice => slice.List, slice => Enumerable.Range(slice.First, slice.Count).Select(Rectangle).ToImmutableArray());

    /// <summary>Each physical pose's components in authored order: five for the ten shared poses, one for the two dying poses.</summary>
    private static readonly ImmutableArray<KraidCollisionComponent>[] PoseComponents =
        [.. Enumerable.Range(0, 12).Select(pose => Enumerable.Range(0, pose < 10 ? 5 : 1).Select(component => Component(pose, component)).ToImmutableArray())];
}
