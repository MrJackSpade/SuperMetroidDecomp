using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Immutable physical components and ordered touch/shot rectangles selected by
/// the 22 Kraid-arm extended frames at $A7:8F59-92B4. The second ten frames
/// reuse the first ten physical layouts but have independent editable OAM.
/// </summary>
internal static class KraidArmCollisionDefinitions
{
    /// <summary>Collision geometry belongs to Kraid arm's bank-$A7 extended frames.</summary>
    internal const byte Bank = 0xa7;

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

    /// <summary>$A7:8F59, ExtendedSpritemap_KraidArm_General_0; twenty five-component frames follow.</summary>
    internal const ushort FirstGeneralFrame = 0x8f59;
    /// <summary>$A7:92A1, ExtendedSpritemap_KraidArm_Dying_PreparingToLungeForward_0.</summary>
    internal const ushort FirstSingleComponentFrame = 0x92a1;
    /// <summary>$A7:92AB, ExtendedSpritemap_KraidArm_Dying_PreparingToLungeForward_1.</summary>
    private const ushort SecondSingleComponentFrame = 0x92ab;
    /// <summary>Native byte stride of a five-component extended frame, including its count word.</summary>
    internal const int GeneralFrameBytes = 2 + 5 * 8;
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
        return new(geometry.Left, geometry.Top, geometry.Right, geometry.Bottom,
            TouchCallback(index), ShotCallback(index));
    }
    /// <summary>$A7:92D1, Hitbox_KraidArm_0; shapes0..6 alternate two/one rectangles.</summary>
    private const ushort FirstArticulatedShape = 0x92d1;
    /// <summary>$A7:93F7, Hitbox_KraidArm_F, connecting component's initial shape.</summary>
    private const ushort ConnectorInitialShape = 0x93f7;
    /// <summary>$A7:9371, Hitbox_KraidArm_8, connecting component's second shape.</summary>
    private const ushort ConnectorSecondShape = 0x9371;
    /// <summary>$A7:937F, Hitbox_KraidArm_9, connecting component's third shape.</summary>
    private const ushort ConnectorThirdShape = 0x937f;
    /// <summary>$A7:9399, Hitbox_KraidArm_A, connecting component's final shape.</summary>
    private const ushort ConnectorFinalShape = 0x9399;
    /// <summary>$A7:9439, Hitbox_KraidArm_12, body component in poses0/1.</summary>
    private const ushort BodyInitialShape = 0x9439;
    /// <summary>$A7:941F, Hitbox_KraidArm_11, body component in poses2/3.</summary>
    private const ushort BodyMiddleShape = 0x941f;
    /// <summary>$A7:9411, Hitbox_KraidArm_10, body component in poses4..9.</summary>
    private const ushort BodyFinalShape = 0x9411;
    /// <summary>$A7:946F, first dying/preparing-to-lunge rectangle; second list follows14 bytes later.</summary>
    private const ushort FinalPoseShape = 0x946f;

    /// <summary>Calculates the physical hitbox identity for a normalized arm pose and component.</summary>
    /// <remarks>Independently checked for #1165 against both native ten-frame groups.
    /// Components0/3 advance through seven articulated shapes with different timing;
    /// their lists alternate two and one twelve-byte rectangles plus a count word.
    /// Components1/4 choose connecting shapes and component2 chooses body shapes.
    /// Poses10/11 are the single-component dying/lunge frames. Coordinates are separate.</remarks>
    internal static ushort ComponentHitbox(int pose, int component)
    {
        if ((uint)pose >= 12)
            throw new ArgumentOutOfRangeException(nameof(pose));
        if ((uint)component >= (pose < 10 ? 5u : 1u))
            throw new ArgumentOutOfRangeException(nameof(component));
        if (pose >= 10) return (ushort)(FinalPoseShape + 14 * (pose - 10));
        if (component == 2)
            return pose < 2 ? BodyInitialShape : pose < 4 ? BodyMiddleShape : BodyFinalShape;
        if (component is 1 or 4)
        {
            int initialPoses = component == 1 ? 2 : 3;
            return pose < initialPoses ? ConnectorInitialShape : pose < 4 ? ConnectorSecondShape
                : pose < 6 ? ConnectorThirdShape : ConnectorFinalShape;
        }
        int shape = component == 0 ? (pose < 4 ? pose : 4 + (pose - 4) / 2)
            : pose < 2 ? 0 : pose < 6 ? pose - 1 : pose < 9 ? 5 : 6;
        return (ushort)(FirstArticulatedShape + 40 * (shape / 2) + 26 * (shape % 2));
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
    /// <summary>The two single-component arm frames, valued by their bank-$A7 spritemap pointer.</summary>
    private enum SingleComponentFrame : ushort
    {
        /// <summary>$A7:92A1, ExtendedSpritemap_KraidArm_Dying_PreparingToLungeForward_0.</summary>
        First = FirstSingleComponentFrame,
        /// <summary>$A7:92AB, ExtendedSpritemap_KraidArm_Dying_PreparingToLungeForward_1.</summary>
        Second = SecondSingleComponentFrame,
    }

    /// <summary>Single-component frames follow the ten shared five-component pose ordinals.</summary>
    private static int PoseOrdinal(SingleComponentFrame frame) => frame switch
    {
        SingleComponentFrame.First => 10,
        SingleComponentFrame.Second => 11,
        _ => throw new InvalidOperationException($"Undefined {nameof(SingleComponentFrame)} {(int)frame}."),
    };

    /// <summary>Normalizes either ten-frame artwork group to its shared physical pose and recognizes the two one-component dying frames.</summary>
    internal static bool TryGetComponents(ushort pointer,
        out ImmutableArray<KraidCollisionComponent> components)
    {
        // Both five-component groups select the same physical pose ordinal. Their
        // artwork identities differ, so this normalization applies only to collision.
        int distance = pointer - FirstGeneralFrame;
        if (distance >= 0 && distance < 20 * GeneralFrameBytes && distance % GeneralFrameBytes == 0)
        {
            components = PoseComponents[distance / GeneralFrameBytes % 10];
            return true;
        }
        if (Enum.IsDefined((SingleComponentFrame)pointer))
        {
            components = PoseComponents[PoseOrdinal((SingleComponentFrame)pointer)];
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
    internal static ImmutableArray<KraidCollisionHitbox> HitboxesAt(ushort pointer) =>
        HitboxLists.TryGetValue(pointer, out ImmutableArray<KraidCollisionHitbox> list)
            ? list
            : throw new InvalidDataException($"Kraid-arm hitbox list $A7:{pointer:X4} is not compiled.");

    /// <summary>Each compiled hitbox list's pointer and its slice of the rectangle geometry: first rectangle and count.</summary>
    private static readonly (ushort List, int First, int Count)[] HitboxListSlices =
    [
        (0x92d1, 0, 2),
        (0x92eb, 2, 1),
        (0x92f9, 3, 2),
        (0x9313, 5, 1),
        (0x9321, 6, 2),
        (0x933b, 8, 1),
        (0x9349, 9, 2),
        (0x9371, 11, 1),
        (0x937f, 12, 2),
        (0x9399, 14, 1),
        (0x93f7, 15, 2),
        (0x9411, 17, 1),
        (0x941f, 18, 2),
        (0x9439, 20, 2),
        (0x946f, 22, 1),
        (0x947d, 23, 1),
    ];

    /// <summary>Each compiled hitbox list's rectangles in collision order, keyed by list pointer.</summary>
    private static readonly Dictionary<ushort, ImmutableArray<KraidCollisionHitbox>> HitboxLists = HitboxListSlices.ToDictionary(
        slice => slice.List, slice => Enumerable.Range(slice.First, slice.Count).Select(Rectangle).ToImmutableArray());

    /// <summary>Each physical pose's components in native order: five for the ten shared poses, one for the two dying poses.</summary>
    private static readonly ImmutableArray<KraidCollisionComponent>[] PoseComponents =
        [.. Enumerable.Range(0, 12).Select(pose => Enumerable.Range(0, pose < 10 ? 5 : 1).Select(component => Component(pose, component)).ToImmutableArray())];
}
