using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Immutable physical components and ordered touch/shot rectangles selected by
/// the 22 Kraid-arm extended frames at $A7:8F59-92B4. The second ten frames
/// reuse the first ten physical layouts but have independent editable OAM.
/// </summary>
internal static class KraidArmCollisionDefinitions
{

    /// <summary>
    /// The four articulated component offsets of each of the ten shared five-component poses, indexed by pose; the body
    /// component stays at the enemy origin. Pose 0 is the initial pose and pose 1 the next before the arm rises; poses 2
    /// and 3 raise it outward, the second shifting hand and connector; poses 4 and 5 lower it beside Kraid's body;
    /// poses 6 through 9 hold the low pose, 7 lifting the outer connector a pixel above 6, 8 repeating 7's positions under
    /// distinct artwork, and 9 shifting the two outer components toward one another.
    /// </summary>
    private static readonly Offset16<RoomPixels>[][] PosePositions =
    [
        [new(-36, -33), new(-28, -24), new(-36, -40), new(-28, -31)],
        [new(-38, -33), new(-30, -26), new(-36, -40), new(-28, -31)],
        [new(-48, -13), new(-38, -13), new(-45, -27), new(-37, -19)],
        [new(-46, -13), new(-37, -13), new(-45, -19), new(-36, -18)],
        [new(-45, 8), new(-38, 2), new(-46, 3), new(-39, -3)],
        [new(-44, 8), new(-37, 2), new(-46, 4), new(-39, -2)],
        [new(-39, 10), new(-38, 0), new(-43, 10), new(-41, -2)],
        [new(-39, 10), new(-38, 0), new(-43, 9), new(-41, -2)],
        [new(-39, 10), new(-38, 0), new(-43, 9), new(-41, -2)],
        [new(-39, 10), new(-38, 0), new(-42, 9), new(-42, -2)],
    ];

    /// <summary>Ordered physical rectangles; callbacks are calculated separately from the index.</summary>
    private static readonly EnemyHitboxRectangle[] Geometry =
    [
        new(-13, -11, -3, -5),
        new(-9, -5, 1, 2),
        new(-16, -5, 1, 2),
        new(-9, -2, 1, 7),
        new(-12, 3, -6, 12),
        new(-6, -1, 1, 14),
        new(-3, -2, 6, 9),
        new(2, 7, 11, 11),
        new(-1, -4, 14, 4),
        new(-3, -7, 6, 2),
        new(4, -12, 10, -1),
        new(-15, -5, 2, 4),
        new(-11, 2, -4, 10),
        new(-6, -3, 3, 5),
        new(-4, -2, 3, 13),
        new(-12, -12, -3, -3),
        new(-6, -6, 3, 2),
        new(-45, -9, 4, 8),
        new(-28, -17, -12, 0),
        new(-42, -23, -28, -6),
        new(-22, -25, -8, -5),
        new(-35, -35, -19, -17),
        new(-64, -48, -32, -16),
        new(-64, -4, 0, 4),
    ];

    /// <summary>Twenty-four rectangles selected by the compiled arm frame families.</summary>
    internal const int RectangleCount = 24;
    /// <summary>First compiled rectangle from $A7:9411, Hitbox_KraidArm_10, using normal background touch.</summary>
    private const int FirstBackgroundRectangle = 17;

    /// <summary>Selects the arm pushback/lint reaction for active shapes, ordinary touch for background/lunge shapes.</summary>
    /// <remarks>Independently reviewed for #1165 against all selected bank_A7 native
    /// rectangles. The selected lists through Hitbox_KraidArm_F use arm touch;
    /// Hitbox_KraidArm_10..12 and both dying/lunge lists use background touch.</remarks>
    internal static KraidHitboxTouch TouchCallback(int rectangle)
    {
        if ((uint)rectangle >= RectangleCount) throw new ArgumentOutOfRangeException(nameof(rectangle));
        return rectangle < FirstBackgroundRectangle ? KraidHitboxTouch.Arm : KraidHitboxTouch.Background;
    }

    /// <summary>Every selected arm rectangle uses the arm's shot reaction (EnemyShot_KraidArm).</summary>
    internal static KraidHitboxShot ShotCallback(int rectangle)
    {
        if ((uint)rectangle >= RectangleCount) throw new ArgumentOutOfRangeException(nameof(rectangle));
        return KraidHitboxShot.Arm;
    }

    /// <summary>Combines one indexed rectangle's bounds with its arm/background touch and arm-shot reactions.</summary>
    internal static KraidCollisionHitbox Rectangle(int index)
    {
        if ((uint)index >= RectangleCount) throw new ArgumentOutOfRangeException(nameof(index));
        EnemyHitboxRectangle geometry = Geometry[index];
        return new(checked((short)geometry.Left), checked((short)geometry.Top), checked((short)geometry.Right), checked((short)geometry.Bottom),
            TouchCallback(index), ShotCallback(index));
    }
    /// <summary>The seven articulated shapes components 0 and 3 advance through.</summary>
    private static readonly KraidArmHitboxList[] ArticulatedShapes =
    [
        KraidArmHitboxList.Articulated0, KraidArmHitboxList.Articulated1, KraidArmHitboxList.Articulated2,
        KraidArmHitboxList.Articulated3, KraidArmHitboxList.Articulated4, KraidArmHitboxList.Articulated5,
        KraidArmHitboxList.Articulated6,
    ];

    /// <summary>Calculates the physical hitbox identity for a normalized arm pose and component.</summary>
    /// <remarks>Independently checked for #1165 against both native ten-frame groups.
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
    /// single-component records are anchored at the enemy origin. All22 native
    /// zero-coordinate pairs are independently confirmed for #1165; the remaining
    /// articulated positions have separate, unfinished review scope.</remarks>
    internal static KraidCollisionComponent Component(int pose, int component)
    {
        ImmutableArray<KraidCollisionHitbox> hitbox = HitboxesAt(ComponentHitbox(pose, component));
        // The central body component and both single-component poses are anchored
        // at the enemy origin. Only the four articulated positions are stored.
        if (pose >= 10 || component == 2) return new(Offset16<RoomPixels>.Zero, hitbox);
        Offset16<RoomPixels>[] positions = PosePositions[pose];
        return new(positions[component < 2 ? component : component - 1], hitbox);
    }
    /// <summary>
    /// The physical pose of each Kraid-arm frame: the two ten-frame artwork groups share the ten five-component poses,
    /// and the two dying frames take the single-component poses 10 and 11.
    /// </summary>
    private static readonly Dictionary<EnemyFrameId, int> FramePoses = new()
    {
        [EnemyFrameId.KraidArm0] = 0,
        [EnemyFrameId.KraidArm1] = 1,
        [EnemyFrameId.KraidArm2] = 2,
        [EnemyFrameId.KraidArm3] = 3,
        [EnemyFrameId.KraidArm4] = 4,
        [EnemyFrameId.KraidArm5] = 5,
        [EnemyFrameId.KraidArm6] = 6,
        [EnemyFrameId.KraidArm7] = 7,
        [EnemyFrameId.KraidArm8] = 8,
        [EnemyFrameId.KraidArm9] = 9,
        [EnemyFrameId.KraidArm10] = 0,
        [EnemyFrameId.KraidArm11] = 1,
        [EnemyFrameId.KraidArm12] = 2,
        [EnemyFrameId.KraidArm13] = 3,
        [EnemyFrameId.KraidArm14] = 4,
        [EnemyFrameId.KraidArm15] = 5,
        [EnemyFrameId.KraidArm16] = 6,
        [EnemyFrameId.KraidArm17] = 7,
        [EnemyFrameId.KraidArm18] = 8,
        [EnemyFrameId.KraidArm19] = 9,
        [EnemyFrameId.KraidArm20] = 10,
        [EnemyFrameId.KraidArm21] = 11,
    };

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
    /// Bank-$A7 hitbox lists $92D1-$948A; the first overlapping rectangle wins.
    /// The native callbacks are Kraid-arm touch $9490 or background touch $948B,
    /// and Kraid-arm shot $94B6.
    /// </summary>
    /// <remarks>Independently reviewed for #1165: these cases select ordered
    /// rectangle slices for native arm shapes 0..6, 8..A, F..12 and the two
    /// dying/lunge shapes. Starts are ordinals in the selected 24-record geometry
    /// domain, not native byte offsets; lengths match each original count word.
    /// Shape7, B..E, foot/lint lists, record interiors and every other ushort
    /// identity remain rejected. Rectangle-edge data has a separate disposition.</remarks>
    internal static ImmutableArray<KraidCollisionHitbox> HitboxesAt(KraidArmHitboxList list) =>
        HitboxLists.TryGetValue(list, out ImmutableArray<KraidCollisionHitbox> rectangles)
            ? rectangles
            : throw new InvalidDataException($"Kraid-arm hitbox list {list} is not compiled.");

    /// <summary>Each compiled hitbox list and its slice of the rectangle geometry: first rectangle and count.</summary>
    private static readonly KraidHitboxListDefinition[] HitboxListSlices =
    [
        new(KraidArmHitboxList.Articulated0, 0, 2),
        new(KraidArmHitboxList.Articulated1, 2, 1),
        new(KraidArmHitboxList.Articulated2, 3, 2),
        new(KraidArmHitboxList.Articulated3, 5, 1),
        new(KraidArmHitboxList.Articulated4, 6, 2),
        new(KraidArmHitboxList.Articulated5, 8, 1),
        new(KraidArmHitboxList.Articulated6, 9, 2),
        new(KraidArmHitboxList.ConnectorSecond, 11, 1),
        new(KraidArmHitboxList.ConnectorThird, 12, 2),
        new(KraidArmHitboxList.ConnectorFinal, 14, 1),
        new(KraidArmHitboxList.ConnectorInitial, 15, 2),
        new(KraidArmHitboxList.BodyFinal, 17, 1),
        new(KraidArmHitboxList.BodyMiddle, 18, 2),
        new(KraidArmHitboxList.BodyInitial, 20, 2),
        new(KraidArmHitboxList.FinalPoseFirst, 22, 1),
        new(KraidArmHitboxList.FinalPoseSecond, 23, 1),
    ];

    /// <summary>Each compiled hitbox list's rectangles in collision order.</summary>
    private static readonly Dictionary<KraidArmHitboxList, ImmutableArray<KraidCollisionHitbox>> HitboxLists = HitboxListSlices.ToDictionary(
        slice => slice.List, slice => Enumerable.Range(slice.First, slice.Count).Select(Rectangle).ToImmutableArray());

    /// <summary>Each physical pose's components in native order: five for the ten shared poses, one for the two dying poses.</summary>
    private static readonly ImmutableArray<KraidCollisionComponent>[] PoseComponents =
        [.. Enumerable.Range(0, 12).Select(pose => Enumerable.Range(0, pose < 10 ? 5 : 1).Select(component => Component(pose, component)).ToImmutableArray())];
}
