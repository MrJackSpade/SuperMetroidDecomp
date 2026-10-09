namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable physical components and ordered touch/shot rectangles selected by
/// the 22 Kraid-arm extended frames at $A7:8F59-92B4. The second ten frames
/// reuse the first ten physical layouts but have independent editable OAM.
/// </summary>
internal static class KraidArmCollisionDefinitions
{
    internal const byte Bank = 0xa7;

    private static readonly KraidArmComponentPosition[] Phase0 =
        [new(-36, -33), new(-28, -24),
         new(-36, -40), new(-28, -31)];
    private static readonly KraidArmComponentPosition[] Phase1 =
        [new(-38, -33), new(-30, -26),
         new(-36, -40), new(-28, -31)];
    private static readonly KraidArmComponentPosition[] Phase2 =
        [new(-48, -13), new(-38, -13),
         new(-45, -27), new(-37, -19)];
    private static readonly KraidArmComponentPosition[] Phase3 =
        [new(-46, -13), new(-37, -13),
         new(-45, -19), new(-36, -18)];
    private static readonly KraidArmComponentPosition[] Phase4 =
        [new(-45, 8), new(-38, 2),
         new(-46, 3), new(-39, -3)];
    private static readonly KraidArmComponentPosition[] Phase5 =
        [new(-44, 8), new(-37, 2),
         new(-46, 4), new(-39, -2)];
    private static readonly KraidArmComponentPosition[] Phase6 =
        [new(-39, 10), new(-38, 0),
         new(-43, 10), new(-41, -2)];
    private static readonly KraidArmComponentPosition[] Phase7 =
        [new(-39, 10), new(-38, 0),
         new(-43, 9), new(-41, -2)];
    private static readonly KraidArmComponentPosition[] Phase8 =
        [new(-39, 10), new(-38, 0),
         new(-43, 9), new(-41, -2)];
    private static readonly KraidArmComponentPosition[] Phase9 =
        [new(-39, 10), new(-38, 0),
         new(-42, 9), new(-42, -2)];

    /// <summary>$A7:8F59, ExtendedSpritemap_KraidArm_General_0; twenty five-component frames follow.</summary>
    private const ushort FirstGeneralFrame = 0x8f59;
    /// <summary>$A7:92A1, ExtendedSpritemap_KraidArm_Dying_PreparingToLungeForward_0.</summary>
    private const ushort FirstSingleComponentFrame = 0x92a1;
    /// <summary>$A7:92AB, ExtendedSpritemap_KraidArm_Dying_PreparingToLungeForward_1.</summary>
    private const ushort SecondSingleComponentFrame = 0x92ab;
    private const int GeneralFrameBytes = 2 + 5 * 8;
    private static readonly KraidArmCollisionGeometry[] Geometry =
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

    /// <summary>Selects arm pushback/lint activation for active shapes, normal touch for background/lunge shapes.</summary>
    /// <remarks>Independently reviewed for #1165 against all selected bank_A7 native
    /// rectangles. The selected lists through Hitbox_KraidArm_F use arm touch;
    /// Hitbox_KraidArm_10..12 and both dying/lunge lists use background touch.</remarks>
    internal static ushort TouchCallback(int rectangle)
    {
        if ((uint)rectangle >= RectangleCount) throw new IndexOutOfRangeException();
        return rectangle < FirstBackgroundRectangle ? EnemyAiCodePointers.BankA7.KraidArmTouch
            : EnemyAiCodePointers.BankA7.KraidBackgroundTouch;
    }

    /// <summary>Every selected arm rectangle uses $A7:94B6, EnemyShot_KraidArm.</summary>
    internal static ushort ShotCallback(int rectangle)
    {
        if ((uint)rectangle >= RectangleCount) throw new IndexOutOfRangeException();
        return EnemyAiCodePointers.BankA7.KraidArmShot;
    }

    internal static KraidArmCollisionHitbox Rectangle(int index)
    {
        if ((uint)index >= RectangleCount) throw new IndexOutOfRangeException();
        KraidArmCollisionGeometry geometry = Geometry[index];
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
        if ((uint)pose >= 12 || (uint)component >= (pose < 10 ? 5u : 1u))
            throw new IndexOutOfRangeException();
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

    /// <summary>Constructs one physical component with its calculated hitbox selector.</summary>
    /// <remarks>Component2 in the twenty general/rising frames and both final
    /// single-component records are anchored at the enemy origin. All22 native
    /// zero-coordinate pairs are independently confirmed for #1165; the remaining
    /// articulated positions have separate, unfinished review scope.</remarks>
    internal static KraidArmCollisionComponent Component(int pose, int component)
    {
        ushort hitbox = ComponentHitbox(pose, component);
        // The central body component and both single-component poses are anchored
        // at the enemy origin. Only the four articulated positions are stored.
        if (pose >= 10 || component == 2) return new(0, 0, hitbox);
        KraidArmComponentPosition[] positions = pose switch
        {
            0 => Phase0, 1 => Phase1, 2 => Phase2, 3 => Phase3, 4 => Phase4,
            5 => Phase5, 6 => Phase6, 7 => Phase7, 8 => Phase8, _ => Phase9,
        };
        KraidArmComponentPosition position = positions[component < 2 ? component : component - 1];
        return new(position.X, position.Y, hitbox);
    }
    internal static bool TryGetComponents(ushort pointer,
        out KraidArmComponentSequence components)
    {
        // Both five-component groups select the same physical pose ordinal. Their
        // artwork identities differ, so this normalization applies only to collision.
        int distance = pointer - FirstGeneralFrame;
        if (distance >= 0 && distance < 20 * GeneralFrameBytes && distance % GeneralFrameBytes == 0)
        {
            components = new(distance / GeneralFrameBytes % 10, 5);
            return true;
        }
        if (pointer is FirstSingleComponentFrame or SecondSingleComponentFrame)
        {
            components = new(pointer == FirstSingleComponentFrame ? 10 : 11, 1);
            return true;
        }
        components = default;
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
    internal static KraidArmHitboxSequence HitboxesAt(ushort pointer) =>
        pointer switch
        {
            0x92d1 => new(0, 2),
            0x92eb => new(2, 1),
            0x92f9 => new(3, 2),
            0x9313 => new(5, 1),
            0x9321 => new(6, 2),
            0x933b => new(8, 1),
            0x9349 => new(9, 2),
            0x9371 => new(11, 1),
            0x937f => new(12, 2),
            0x9399 => new(14, 1),
            0x93f7 => new(15, 2),
            0x9411 => new(17, 1),
            0x941f => new(18, 2),
            0x9439 => new(20, 2),
            0x946f => new(22, 1),
            0x947d => new(23, 1),
            _ => throw new InvalidDataException(
                $"Kraid-arm hitbox list $A7:{pointer:X4} is not compiled."),
        };
}

internal readonly record struct KraidArmCollisionComponent(short X, short Y, ushort HitboxPointer);
internal readonly record struct KraidArmCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>Ordered rectangle view that attaches calculated callbacks without storing them per record.</summary>
internal readonly record struct KraidArmHitboxSequence(int Start, int Length)
{
    internal KraidArmCollisionHitbox this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException();
            return KraidArmCollisionDefinitions.Rectangle(Start + index);
        }
    }
    public Enumerator GetEnumerator() => new(this);
    internal struct Enumerator(KraidArmHitboxSequence sequence)
    {
        private int next;
        public bool MoveNext() => next++ < sequence.Length;
        public KraidArmCollisionHitbox Current => sequence[next - 1];
    }
}

internal readonly record struct KraidArmCollisionGeometry(short Left, short Top, short Right, short Bottom);

internal readonly record struct KraidArmComponentPosition(short X, short Y);

/// <summary>Ordered physical component view with calculated hitbox selectors.</summary>
internal readonly record struct KraidArmComponentSequence(int Pose, int Length)
{
    internal KraidArmCollisionComponent this[int index]
    {
        get
        {
            if ((uint)index >= (uint)Length) throw new IndexOutOfRangeException();
            return KraidArmCollisionDefinitions.Component(Pose, index);
        }
    }
    public Enumerator GetEnumerator() => new(this);
    internal struct Enumerator(KraidArmComponentSequence sequence)
    {
        private int next;
        public bool MoveNext() => next++ < sequence.Length;
        public KraidArmCollisionComponent Current => sequence[next - 1];
    }
}
