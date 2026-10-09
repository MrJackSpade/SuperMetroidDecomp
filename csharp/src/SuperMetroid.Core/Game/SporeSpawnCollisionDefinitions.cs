namespace SuperMetroid.Core.Game;

/// <summary>Fixed offset and bank-$A5 hitbox-list identity for Spore Spawn.</summary>
/// <param name="X">Horizontal component displacement from the owning extended spritemap origin.</param>
/// <param name="Y">Vertical component displacement from the owning extended spritemap origin.</param>
/// <param name="HitboxPointer">Bank-$A5 identity selecting the collision rectangles for this component.</param>
internal readonly record struct SporeSpawnCollisionComponent(
    short X, short Y, ushort HitboxPointer);

/// <summary>Cartridge rectangle and touch/shot callbacks, independent of OAM artwork.</summary>
/// <param name="Left">Inclusive left edge relative to the enemy origin.</param>
/// <param name="Top">Inclusive top edge relative to the enemy origin.</param>
/// <param name="Right">Inclusive right edge relative to the enemy origin.</param>
/// <param name="Bottom">Inclusive bottom edge relative to the enemy origin.</param>
/// <param name="TouchAi">Native callback selected when Samus contacts the rectangle.</param>
/// <param name="ShotAi">Native callback selected when a projectile intersects the rectangle.</param>
internal readonly record struct SporeSpawnCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>
/// The twelve bank-$A5 Spore Spawn extended OAM frames at $EE65-$EF61 and
/// their fixed gameplay hitboxes. Version 6 mislabeled their visual keys as
/// Draygon, but collision always follows the actual Spore Spawn program.
/// </summary>
internal static class SporeSpawnCollisionDefinitions
{
    /// <summary>$A5:EF73, closed-head hitbox with no active touch response.</summary>
    internal const ushort ClosedHead = 0xef73;
    /// <summary>$A5:EF8D, open-head hitbox.</summary>
    internal const ushort OpenHead = 0xef8d;
    /// <summary>$A5:EFA7, extended open-head hitbox.</summary>
    internal const ushort ExtendedHead = 0xefa7;
    /// <summary>$A5:EFC1, first four-rectangle moving head.</summary>
    internal const ushort MovingHead0 = 0xefc1;
    /// <summary>$A5:EFF3, second four-rectangle moving head.</summary>
    internal const ushort MovingHead1 = 0xeff3;
    /// <summary>$A5:F025, third four-rectangle moving head.</summary>
    internal const ushort MovingHead2 = 0xf025;
    /// <summary>$A5:F057, fourth four-rectangle moving head.</summary>
    internal const ushort MovingHead3 = 0xf057;
    /// <summary>$A5:F139, trailing vulnerable point with Spore Spawn shot AI.</summary>
    internal const ushort TrailingShotPoint = 0xf139;
    /// <summary>$A5:F147, mirrored trailing vulnerable point.</summary>
    internal const ushort MirroredTrailingShotPoint = 0xf147;
    /// <summary>$A5:F155, trailing point with dud-shot AI.</summary>
    internal const ushort TrailingDudPoint = 0xf155;
    /// <summary>$A5:F1A9, fifth four-rectangle moving head.</summary>
    internal const ushort MovingHead4 = 0xf1a9;
    /// <summary>$A5:F1DB, sixth four-rectangle moving head.</summary>
    internal const ushort MovingHead5 = 0xf1db;

    /// <summary>Native Spore Spawn callback for contact with a vulnerable head region.</summary>
    private const ushort Touch = EnemyAiCodePointers.BankA5.SporeSpawnTouch;
    /// <summary>Native Spore Spawn projectile callback that applies shot behavior.</summary>
    private const ushort Shot = EnemyAiCodePointers.BankA5.SporeSpawnShot;
    /// <summary>Native no-op callback used where head contact has no effect.</summary>
    private const ushort NoOp = EnemyAiCodePointers.BankA0.NoOp;
    /// <summary>Native dud-shot callback used by non-vulnerable shell rectangles.</summary>
    private const ushort Dud = EnemyAiCodePointers.BankA0.DudShot;

    /// <summary>$A5:EE65, ExtendedSpritemap_SporeSpawn_Dead: one inactive head component.</summary>
    private const ushort DeadFrame = 0xee65;
    /// <summary>$A5:EE6F, ExtendedSpritemap_SporeSpawn_Closed_Closing_Opening_0: one closed head.</summary>
    private const ushort ClosedFrame = 0xee6f;
    /// <summary>$A5:EE79, ExtendedSpritemap_SporeSpawn_Closed_Closing_Opening_1: first of seven opening roots.</summary>
    private const ushort FirstOpeningFrame = 0xee79;
    /// <summary>$A5:EF3D, ExtendedSpritemap_SporeSpawn_FullyOpen_0: first of three open oscillation roots.</summary>
    private const ushort FirstFullyOpenFrame = 0xef3d;

    /// <summary>Native zero-offset head followed by its inner vulnerable point when the head is open.</summary>
    /// <param name="Head">Hitbox-list identity for the head component.</param>
    /// <param name="InnerPoint">Optional inner vulnerable-point identity; zero means the sequence contains only the head.</param>
    internal readonly record struct ComponentSequence(ushort Head, ushort InnerPoint)
    {
        /// <summary>Number of components: one head, plus an inner point when its pointer is nonzero.</summary>
        internal int Length => InnerPoint == 0 ? 1 : 2;
        /// <summary>Returns the head first and the optional inner vulnerable point second.</summary>
        /// <param name="index">Zero-based component index.</param><exception cref="IndexOutOfRangeException">The index is not within <see cref="Length"/>.</exception>
        internal SporeSpawnCollisionComponent this[int index] => (uint)index < Length
            ? new(0, 0, index == 0 ? Head : InnerPoint)
            : throw new IndexOutOfRangeException();
        /// <summary>Creates a value-type enumerator over this sequence without allocating an array.</summary>
        /// <returns>An enumerator yielding the head and any selected inner point.</returns>
        public Enumerator GetEnumerator() => new(this);
        /// <summary>Iterates a component sequence in head-then-inner-point order.</summary>
        /// <param name="sequence">Immutable component pointers to traverse.</param>
        internal struct Enumerator(ComponentSequence sequence)
        {
            /// <summary>Index of the most recently yielded component; starts before the first item.</summary>
            private int index = -1;
            /// <summary>Component at the current enumerator position.</summary>
            public readonly SporeSpawnCollisionComponent Current => sequence[index];
            /// <summary>Advances to the next component when the sequence has another entry.</summary>
            /// <returns>True when <see cref="Current"/> is valid after advancing.</returns>
            public bool MoveNext() => ++index < sequence.Length;
        }
    }
    // The inner vulnerable point every moving head and trailing point carries.
    /// <summary>Creates the shared inner rectangle with the requested projectile callback and vulnerable touch callback.</summary>
    /// <param name="shot">Shot callback assigned to this inner point.</param><returns>The cartridge-sized inner hitbox.</returns>
    private static SporeSpawnCollisionHitbox InnerPoint(ushort shot) => new(-15, -24, 14, 23, Touch, shot);

    /// <summary>Head rectangles shared by the closed and open lists, which differ only in touch AI.</summary>
    private static SporeSpawnCollisionHitbox[] Head(ushort touch) =>
        [new(-41, -30, 41, 30, touch, Dud), new(-16, -45, 15, -30, touch, Dud)];

    /// <summary>Collision rectangles for the closed head; contact uses the native no-op callback.</summary>
    private static readonly SporeSpawnCollisionHitbox[] ClosedHeadList = Head(NoOp);
    /// <summary>Collision rectangles for the open head; contact uses Spore Spawn's touch callback.</summary>
    private static readonly SporeSpawnCollisionHitbox[] OpenHeadList = Head(Touch);
    /// <summary>Expanded shell rectangles for the first open-head animation pose.</summary>
    private static readonly SporeSpawnCollisionHitbox[] ExtendedHeadList =
        [new(-44, -35, 43, 33, Touch, Dud), new(-16, -49, 15, -35, Touch, Dud)];
    // Moving heads: upper and lower shell bands fitted to each drawing, the inner point, and the crown.
    /// <summary>Four rectangles for moving head pose zero, including its inner vulnerable point and crown.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead0List =
        [new(-45, -38, 44, -9, Touch, Dud), new(-45, 8, 44, 35, Touch, Dud), InnerPoint(Shot), new(-16, -54, 16, -22, Touch, Dud)];
    /// <summary>Four collision rectangles fitted to moving head pose one.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead1List =
        [new(-43, -44, 42, -13, Touch, Dud), new(-44, 12, 42, 42, Touch, Dud), InnerPoint(Shot), new(-16, -58, 16, -42, Touch, Dud)];
    /// <summary>Four collision rectangles fitted to moving head pose two.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead2List =
        [new(-45, -47, 44, -17, Touch, Dud), new(-44, 16, 43, 46, Touch, Dud), InnerPoint(Shot), new(-16, -62, 16, -45, Touch, Dud)];
    /// <summary>Four collision rectangles fitted to moving head pose three.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead3List =
        [new(-44, -50, 45, -21, Touch, Dud), new(-43, 20, 43, 50, Touch, Dud), InnerPoint(Shot), new(-16, -64, 16, -48, Touch, Dud)];
    /// <summary>Four collision rectangles fitted to moving head pose four.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead4List =
        [new(-44, -53, 44, -23, Touch, Dud), new(-44, 22, 43, 52, Touch, Dud), InnerPoint(Shot), new(-16, -68, 16, -48, Touch, Dud)];
    // The sixth head's inner point is a pixel taller at both ends, as authored.
    /// <summary>Four collision rectangles for the sixth moving pose, including its taller authored inner point.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead5List =
        [new(-44, -55, 43, -25, Touch, Dud), new(-45, 24, 43, 55, Touch, Dud), new(-15, -25, 14, 24, Touch, Shot), new(-16, -69, 16, -48, Touch, Dud)];
    /// <summary>Single vulnerable-point list for the ordinary trailing shot animation phase.</summary>
    private static readonly SporeSpawnCollisionHitbox[] ShotPointList = [InnerPoint(Shot)];
    /// <summary>Single non-vulnerable point list for the dud-shot animation phase.</summary>
    private static readonly SporeSpawnCollisionHitbox[] DudPointList = [InnerPoint(Dud)];

    /// <summary>Checks whether a pointer is one of the seven regularly spaced opening animation roots.</summary>
    /// <param name="pointer">Extended-spritemap frame pointer in bank $A5.</param><returns>True for an aligned opening root from the first opening frame through the last.</returns>
    private static bool IsOpeningFrame(ushort pointer) =>
        pointer >= FirstOpeningFrame && pointer <= FirstOpeningFrame + 6 * 18 &&
        (pointer - FirstOpeningFrame) % 18 == 0;

    /// <summary>Checks whether a pointer is one of the three fully-open oscillation roots.</summary>
    /// <param name="pointer">Extended-spritemap frame pointer in bank $A5.</param><returns>True for an aligned fully-open root.</returns>
    private static bool IsFullyOpenFrame(ushort pointer) =>
        pointer >= FirstFullyOpenFrame && pointer <= FirstFullyOpenFrame + 2 * 18 &&
        (pointer - FirstFullyOpenFrame) % 18 == 0;

    /// <summary>Determines whether the frame pointer belongs to the compiled dead, closed, opening, or fully-open collision domain.</summary>
    /// <param name="pointer">Extended-spritemap frame pointer to classify.</param><returns>True only for a root with a defined collision mapping.</returns>
    internal static bool IsFrame(ushort pointer) =>
        pointer is DeadFrame or ClosedFrame || IsOpeningFrame(pointer) || IsFullyOpenFrame(pointer);

    /// <summary>
    /// $A5:EE65..EEE5 and EF3D..EF61: dead/closed roots contain one head;
    /// opening roots advance the head geometry while the inner point oscillates B,C,D,C.
    /// Fully-open roots hold the final head and select B,C,D. All component offsets are zero.
    /// The intervening unused single-component roots are outside the installed frame domain.
    /// </summary>
    internal static ComponentSequence ComponentsAt(ushort pointer)
    {
        if (pointer == DeadFrame) return new(ClosedHead, 0);
        if (pointer == ClosedFrame) return new(OpenHead, 0);
        if (IsFullyOpenFrame(pointer))
            return new(MovingHead5, InnerPointForPhase((pointer - FirstFullyOpenFrame) / 18));
        if (IsOpeningFrame(pointer))
        {
            int opening = (pointer - FirstOpeningFrame) / 18;
            ushort head = opening == 0 ? ExtendedHead
                : opening < 5 ? (ushort)(MovingHead0 + (opening - 1) * 50)
                : (ushort)(MovingHead4 + (opening - 5) * 50);
            return new(head, InnerPointForPhase(opening % 4));
        }
        throw new InvalidDataException(
            $"Spore Spawn frame $A5:{pointer:X4} has no compiled collision identity.");
    }

    /// <summary>Selects the trailing-point collision identity for the four-phase inner-point oscillation.</summary>
    /// <param name="phase">Zero-based oscillation phase.</param><returns>Shot point for phase zero, dud point for phase two, mirrored shot point otherwise.</returns>
    private static ushort InnerPointForPhase(int phase) => phase switch
    {
        0 => TrailingShotPoint,
        2 => TrailingDudPoint,
        _ => MirroredTrailingShotPoint,
    };
    /// <summary>Returns the cartridge hitbox list associated with a compiled head or trailing-point identity.</summary>
    /// <param name="pointer">Bank-$A5 collision-list pointer selected by the component.</param><returns>Read-only view of the matching collision rectangles.</returns>
    /// <exception cref="InvalidDataException">No hitbox list is compiled for the pointer.</exception>
    internal static ReadOnlySpan<SporeSpawnCollisionHitbox> HitboxesAt(
        ushort pointer) =>
        pointer switch
        {
            ClosedHead => ClosedHeadList,
            OpenHead => OpenHeadList,
            ExtendedHead => ExtendedHeadList,
            MovingHead0 => MovingHead0List,
            MovingHead1 => MovingHead1List,
            MovingHead2 => MovingHead2List,
            MovingHead3 => MovingHead3List,
            TrailingShotPoint or MirroredTrailingShotPoint => ShotPointList,
            TrailingDudPoint => DudPointList,
            MovingHead4 => MovingHead4List,
            MovingHead5 => MovingHead5List,
            _ => throw new InvalidDataException(
                $"Spore Spawn hitbox list $A5:{pointer:X4} is not compiled."),
        };
}
