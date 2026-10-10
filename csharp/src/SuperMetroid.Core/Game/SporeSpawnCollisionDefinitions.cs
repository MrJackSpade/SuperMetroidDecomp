namespace SuperMetroid.Core.Game;

/// <summary>Fixed offset and bank-$A5 hitbox-list identity for Spore Spawn.</summary>
internal readonly record struct SporeSpawnCollisionComponent(
    short X, short Y, ushort HitboxPointer);

/// <summary>Cartridge rectangle and touch/shot callbacks, independent of OAM artwork.</summary>
internal readonly record struct SporeSpawnCollisionHitbox(
    short Left, short Top, short Right, short Bottom, ushort TouchAi, ushort ShotAi);

/// <summary>The twelve bank-$A5 Spore Spawn hitbox lists selected by its collision components.</summary>
internal enum SporeSpawnHitboxList : ushort
{
    /// <summary>$A5:EF73, closed-head hitbox with no active touch response.</summary>
    ClosedHead = 0xef73,
    /// <summary>$A5:EF8D, open-head hitbox.</summary>
    OpenHead = 0xef8d,
    /// <summary>$A5:EFA7, extended open-head hitbox.</summary>
    ExtendedHead = 0xefa7,
    /// <summary>$A5:EFC1, first four-rectangle moving head.</summary>
    MovingHead0 = 0xefc1,
    /// <summary>$A5:EFF3, second four-rectangle moving head.</summary>
    MovingHead1 = 0xeff3,
    /// <summary>$A5:F025, third four-rectangle moving head.</summary>
    MovingHead2 = 0xf025,
    /// <summary>$A5:F057, fourth four-rectangle moving head.</summary>
    MovingHead3 = 0xf057,
    /// <summary>$A5:F139, trailing vulnerable point with Spore Spawn shot AI.</summary>
    TrailingShotPoint = 0xf139,
    /// <summary>$A5:F147, mirrored trailing vulnerable point.</summary>
    MirroredTrailingShotPoint = 0xf147,
    /// <summary>$A5:F155, trailing point with dud-shot AI.</summary>
    TrailingDudPoint = 0xf155,
    /// <summary>$A5:F1A9, fifth four-rectangle moving head.</summary>
    MovingHead4 = 0xf1a9,
    /// <summary>$A5:F1DB, sixth four-rectangle moving head.</summary>
    MovingHead5 = 0xf1db,
}

/// <summary>
/// The twelve bank-$A5 Spore Spawn extended OAM frames at $EE65-$EF61 and
/// their fixed gameplay hitboxes. Version 6 mislabeled their visual keys as
/// Draygon, but collision always follows the actual Spore Spawn program.
/// </summary>
internal static class SporeSpawnCollisionDefinitions
{
    private const ushort Touch = EnemyAiCodePointers.BankA5.SporeSpawnTouch;
    private const ushort Shot = EnemyAiCodePointers.BankA5.SporeSpawnShot;
    private const ushort NoOp = EnemyAiCodePointers.BankA0.NoOp;
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
    internal readonly record struct ComponentSequence(ushort Head, ushort InnerPoint)
    {
        internal int Length => InnerPoint == 0 ? 1 : 2;
        internal SporeSpawnCollisionComponent this[int index] => (uint)index < Length
            ? new(0, 0, index == 0 ? Head : InnerPoint)
            : throw new IndexOutOfRangeException();
        public Enumerator GetEnumerator() => new(this);
        internal struct Enumerator(ComponentSequence sequence)
        {
            private int index = -1;
            public readonly SporeSpawnCollisionComponent Current => sequence[index];
            public bool MoveNext() => ++index < sequence.Length;
        }
    }
    // The inner vulnerable point every moving head and trailing point carries.
    private static SporeSpawnCollisionHitbox InnerPoint(ushort shot) => new(-15, -24, 14, 23, Touch, shot);

    /// <summary>Head rectangles shared by the closed and open lists, which differ only in touch AI.</summary>
    private static SporeSpawnCollisionHitbox[] Head(ushort touch) =>
        [new(-41, -30, 41, 30, touch, Dud), new(-16, -45, 15, -30, touch, Dud)];

    private static readonly SporeSpawnCollisionHitbox[] ClosedHeadList = Head(NoOp);
    private static readonly SporeSpawnCollisionHitbox[] OpenHeadList = Head(Touch);
    private static readonly SporeSpawnCollisionHitbox[] ExtendedHeadList =
        [new(-44, -35, 43, 33, Touch, Dud), new(-16, -49, 15, -35, Touch, Dud)];
    // Moving heads: upper and lower shell bands fitted to each drawing, the inner point, and the crown.
    private static readonly SporeSpawnCollisionHitbox[] MovingHead0List =
        [new(-45, -38, 44, -9, Touch, Dud), new(-45, 8, 44, 35, Touch, Dud), InnerPoint(Shot), new(-16, -54, 16, -22, Touch, Dud)];
    private static readonly SporeSpawnCollisionHitbox[] MovingHead1List =
        [new(-43, -44, 42, -13, Touch, Dud), new(-44, 12, 42, 42, Touch, Dud), InnerPoint(Shot), new(-16, -58, 16, -42, Touch, Dud)];
    private static readonly SporeSpawnCollisionHitbox[] MovingHead2List =
        [new(-45, -47, 44, -17, Touch, Dud), new(-44, 16, 43, 46, Touch, Dud), InnerPoint(Shot), new(-16, -62, 16, -45, Touch, Dud)];
    private static readonly SporeSpawnCollisionHitbox[] MovingHead3List =
        [new(-44, -50, 45, -21, Touch, Dud), new(-43, 20, 43, 50, Touch, Dud), InnerPoint(Shot), new(-16, -64, 16, -48, Touch, Dud)];
    private static readonly SporeSpawnCollisionHitbox[] MovingHead4List =
        [new(-44, -53, 44, -23, Touch, Dud), new(-44, 22, 43, 52, Touch, Dud), InnerPoint(Shot), new(-16, -68, 16, -48, Touch, Dud)];
    // The sixth head's inner point is a pixel taller at both ends, as authored.
    private static readonly SporeSpawnCollisionHitbox[] MovingHead5List =
        [new(-44, -55, 43, -25, Touch, Dud), new(-45, 24, 43, 55, Touch, Dud), new(-15, -25, 14, 24, Touch, Shot), new(-16, -69, 16, -48, Touch, Dud)];
    private static readonly SporeSpawnCollisionHitbox[] ShotPointList = [InnerPoint(Shot)];
    private static readonly SporeSpawnCollisionHitbox[] DudPointList = [InnerPoint(Dud)];

    private static bool IsOpeningFrame(ushort pointer) =>
        pointer >= FirstOpeningFrame && pointer <= FirstOpeningFrame + 6 * 18 &&
        (pointer - FirstOpeningFrame) % 18 == 0;

    private static bool IsFullyOpenFrame(ushort pointer) =>
        pointer >= FirstFullyOpenFrame && pointer <= FirstFullyOpenFrame + 2 * 18 &&
        (pointer - FirstFullyOpenFrame) % 18 == 0;

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
        if (pointer == DeadFrame) return new((ushort)SporeSpawnHitboxList.ClosedHead, 0);
        if (pointer == ClosedFrame) return new((ushort)SporeSpawnHitboxList.OpenHead, 0);
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

    private static ushort InnerPointForPhase(int phase) => phase switch
    {
        0 => (ushort)SporeSpawnHitboxList.TrailingShotPoint,
        2 => (ushort)SporeSpawnHitboxList.TrailingDudPoint,
        _ => (ushort)SporeSpawnHitboxList.MirroredTrailingShotPoint,
    };
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
