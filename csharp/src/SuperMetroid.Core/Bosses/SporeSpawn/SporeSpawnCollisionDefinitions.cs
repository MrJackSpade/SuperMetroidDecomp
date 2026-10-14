using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// The twelve bank-$A5 Spore Spawn extended OAM frames at $EE65-$EF61 and
/// their fixed gameplay hitboxes. Version 6 mislabeled their visual keys as
/// Draygon, but collision always follows the actual Spore Spawn program.
/// </summary>
internal static class SporeSpawnCollisionDefinitions
{
    /// <summary>Bank $A5 touch callback for active Spore Spawn shell and vulnerable point.</summary>
    private const ushort Touch = EnemyAiCodePointers.BankA5.SporeSpawnTouch;
    /// <summary>Bank $A5 shot callback for the exposed vulnerable point.</summary>
    private const ushort Shot = EnemyAiCodePointers.BankA5.SporeSpawnShot;
    /// <summary>Inactive head touch callback used before Spore Spawn opens.</summary>
    private const ushort NoOp = EnemyAiCodePointers.BankA0.NoOp;
    /// <summary>
    /// Samus projectile family bits 8-10: the vulnerable core accepts any projectile with one set, besides a charged
    /// beam (<c>SporeSpawn_Shot</c>).
    /// </summary>
    internal const ushort AcceptedFamilyBits = 0x0700;
    /// <summary>Dud shot callback for shell rectangles and the inactive trailing point.</summary>
    private const ushort Dud = EnemyAiCodePointers.BankA0.DudShot;

    /// <summary>The two single-head root frames that precede the opening sequence.</summary>
    private enum HeadRootFrame : ushort
    {
        /// <summary>$A5:EE65, ExtendedSpritemap_SporeSpawn_Dead: one inactive head component.</summary>
        Dead = 0xee65,
        /// <summary>$A5:EE6F, ExtendedSpritemap_SporeSpawn_Closed_Closing_Opening_0: one closed head.</summary>
        Closed = 0xee6f,
    }
    /// <summary>$A5:EE79, ExtendedSpritemap_SporeSpawn_Closed_Closing_Opening_1: first of seven opening roots.</summary>
    private const ushort FirstOpeningFrame = 0xee79;
    /// <summary>$A5:EF3D, ExtendedSpritemap_SporeSpawn_FullyOpen_0: first of three open oscillation roots.</summary>
    private const ushort FirstFullyOpenFrame = 0xef3d;

    /// <summary>Native zero-offset head followed by its inner vulnerable point when the head is open.</summary>
    internal readonly record struct ComponentSequence(ushort Head, ushort InnerPoint)
    {
        /// <summary>Gets one head component or a head plus trailing vulnerable point.</summary>
        internal int Length => InnerPoint == 0 ? 1 : 2;
        /// <summary>Gets a zero-offset component with its native hitbox-list pointer.</summary>
        internal SporeSpawnCollisionComponent this[int index] => (uint)index < Length
            ? new(Offset16<RoomPixels>.Zero, index == 0 ? Head : InnerPoint)
            : throw new IndexOutOfRangeException();
        /// <summary>Enumerates the frame's head and optional inner point in native order.</summary>
        public Enumerator GetEnumerator() => new(this);
        /// <summary>Allocation-free traversal of a frame's one or two collision components.</summary>
        internal struct Enumerator(ComponentSequence sequence)
        {
            /// <summary>Current component index, starting before the first element.</summary>
            private int index = -1;
            /// <summary>Gets the current native collision component.</summary>
            public readonly SporeSpawnCollisionComponent Current => sequence[index];
            /// <summary>Advances to the next component, if any.</summary>
            public bool MoveNext() => ++index < sequence.Length;
        }
    }
    /// <summary>Builds the trailing vulnerable rectangle with active or dud shot response.</summary>
    private static SporeSpawnCollisionHitbox InnerPoint(ushort shot) => new(-15, -24, 14, 23, Touch, shot);

    /// <summary>Head rectangles shared by the closed and open lists, which differ only in touch AI.</summary>
    private static SporeSpawnCollisionHitbox[] Head(ushort touch) =>
        [new(-41, -30, 41, 30, touch, Dud), new(-16, -45, 15, -30, touch, Dud)];

    /// <summary>Two closed-head shell rectangles without an active touch response.</summary>
    private static readonly SporeSpawnCollisionHitbox[] ClosedHeadList = Head(NoOp);
    /// <summary>Two open-head shell rectangles with active touch response.</summary>
    private static readonly SporeSpawnCollisionHitbox[] OpenHeadList = Head(Touch);
    /// <summary>Wider head rectangles used by the first opening pose.</summary>
    private static readonly SporeSpawnCollisionHitbox[] ExtendedHeadList =
        [new(-44, -35, 43, 33, Touch, Dud), new(-16, -49, 15, -35, Touch, Dud)];
    /// <summary>First moving head: upper and lower shell bands, inner point, and crown.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead0List =
        [new(-45, -38, 44, -9, Touch, Dud), new(-45, 8, 44, 35, Touch, Dud), InnerPoint(Shot), new(-16, -54, 16, -22, Touch, Dud)];
    /// <summary>Second moving head with widened vertical shell spacing.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead1List =
        [new(-43, -44, 42, -13, Touch, Dud), new(-44, 12, 42, 42, Touch, Dud), InnerPoint(Shot), new(-16, -58, 16, -42, Touch, Dud)];
    /// <summary>Third moving head with its raised crown and exposed inner point.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead2List =
        [new(-45, -47, 44, -17, Touch, Dud), new(-44, 16, 43, 46, Touch, Dud), InnerPoint(Shot), new(-16, -62, 16, -45, Touch, Dud)];
    /// <summary>Fourth moving head with shell bands extended farther above and below.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead3List =
        [new(-44, -50, 45, -21, Touch, Dud), new(-43, 20, 43, 50, Touch, Dud), InnerPoint(Shot), new(-16, -64, 16, -48, Touch, Dud)];
    /// <summary>Fifth moving head and its trailing vulnerable point.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead4List =
        [new(-44, -53, 44, -23, Touch, Dud), new(-44, 22, 43, 52, Touch, Dud), InnerPoint(Shot), new(-16, -68, 16, -48, Touch, Dud)];
    /// <summary>Sixth moving head; its inner point extends one pixel farther at both vertical ends.</summary>
    private static readonly SporeSpawnCollisionHitbox[] MovingHead5List =
        [new(-44, -55, 43, -25, Touch, Dud), new(-45, 24, 43, 55, Touch, Dud), new(-15, -25, 14, 24, Touch, Shot), new(-16, -69, 16, -48, Touch, Dud)];
    /// <summary>Single trailing point that accepts the Spore Spawn shot callback.</summary>
    private static readonly SporeSpawnCollisionHitbox[] ShotPointList = [InnerPoint(Shot)];
    /// <summary>Single trailing point that rejects shots through the dud callback.</summary>
    private static readonly SporeSpawnCollisionHitbox[] DudPointList = [InnerPoint(Dud)];

    /// <summary>Checks seven fixed-stride opening frame roots in bank $A5.</summary>
    private static bool IsOpeningFrame(ushort pointer) =>
        pointer >= FirstOpeningFrame && pointer <= FirstOpeningFrame + 6 * 18 &&
        (pointer - FirstOpeningFrame) % 18 == 0;

    /// <summary>Checks the three fixed-stride fully open oscillation frame roots.</summary>
    private static bool IsFullyOpenFrame(ushort pointer) =>
        pointer >= FirstFullyOpenFrame && pointer <= FirstFullyOpenFrame + 2 * 18 &&
        (pointer - FirstFullyOpenFrame) % 18 == 0;

    /// <summary>Checks whether a pointer belongs to an installed Spore Spawn collision frame.</summary>
    internal static bool IsFrame(ushort pointer) =>
        Enum.IsDefined((HeadRootFrame)pointer) || IsOpeningFrame(pointer) || IsFullyOpenFrame(pointer);

    /// <summary>
    /// $A5:EE65..EEE5 and EF3D..EF61: dead/closed roots contain one head;
    /// opening roots advance the head geometry while the inner point oscillates B,C,D,C.
    /// Fully-open roots hold the final head and select B,C,D. All component offsets are zero.
    /// The intervening unused single-component roots are outside the installed frame domain.
    /// </summary>
    internal static ComponentSequence ComponentsAt(ushort pointer)
    {
        if (Enum.IsDefined((HeadRootFrame)pointer))
        {
            var root = (HeadRootFrame)pointer;
            return root switch
            {
                HeadRootFrame.Dead => new((ushort)SporeSpawnHitboxList.ClosedHead, 0),
                HeadRootFrame.Closed => new((ushort)SporeSpawnHitboxList.OpenHead, 0),
                _ => throw new InvalidOperationException($"Undefined {nameof(HeadRootFrame)} {(int)root}."),
            };
        }
        if (IsFullyOpenFrame(pointer))
            return new((ushort)SporeSpawnHitboxList.MovingHead5, InnerPointForPhase((pointer - FirstFullyOpenFrame) / 18));
        if (IsOpeningFrame(pointer))
        {
            int opening = (pointer - FirstOpeningFrame) / 18;
            ushort head = opening == 0 ? (ushort)SporeSpawnHitboxList.ExtendedHead
                : opening < 5 ? (ushort)((ushort)SporeSpawnHitboxList.MovingHead0 + (opening - 1) * 50)
                : (ushort)((ushort)SporeSpawnHitboxList.MovingHead4 + (opening - 5) * 50);
            return new(head, InnerPointForPhase(opening % 4));
        }
        throw new InvalidDataException(
            $"Spore Spawn frame $A5:{pointer:X4} has no compiled collision identity.");
    }

    /// <summary>Selects active, dud, or mirrored trailing-point hitboxes by opening phase.</summary>
    private static ushort InnerPointForPhase(int phase) => phase switch
    {
        0 => (ushort)SporeSpawnHitboxList.TrailingShotPoint,
        2 => (ushort)SporeSpawnHitboxList.TrailingDudPoint,
        _ => (ushort)SporeSpawnHitboxList.MirroredTrailingShotPoint,
    };
    /// <summary>Gets the fixed physical rectangles and callbacks for a native hitbox-list pointer.</summary>
    internal static ReadOnlySpan<SporeSpawnCollisionHitbox> HitboxesAt(
        ushort pointer) =>
        ClosedNativeWords.Decode<SporeSpawnHitboxList>(pointer, "Spore Spawn hitbox list") switch
        {
            SporeSpawnHitboxList.ClosedHead => ClosedHeadList,
            SporeSpawnHitboxList.OpenHead => OpenHeadList,
            SporeSpawnHitboxList.ExtendedHead => ExtendedHeadList,
            SporeSpawnHitboxList.MovingHead0 => MovingHead0List,
            SporeSpawnHitboxList.MovingHead1 => MovingHead1List,
            SporeSpawnHitboxList.MovingHead2 => MovingHead2List,
            SporeSpawnHitboxList.MovingHead3 => MovingHead3List,
            SporeSpawnHitboxList.TrailingShotPoint or SporeSpawnHitboxList.MirroredTrailingShotPoint => ShotPointList,
            SporeSpawnHitboxList.TrailingDudPoint => DudPointList,
            SporeSpawnHitboxList.MovingHead4 => MovingHead4List,
            SporeSpawnHitboxList.MovingHead5 => MovingHead5List,
            _ => throw new InvalidOperationException($"Undefined {nameof(SporeSpawnHitboxList)} {pointer:X4}."),
        };
}
