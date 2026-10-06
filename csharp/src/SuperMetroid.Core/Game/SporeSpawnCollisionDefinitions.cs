namespace SuperMetroid.Core.Game;

/// <summary>Fixed offset and bank-$A5 hitbox-list identity for Spore Spawn.</summary>
internal readonly record struct SporeSpawnCollisionComponent(
    short X, short Y, ushort HitboxPointer);

/// <summary>Cartridge rectangle and touch/shot callbacks, independent of OAM artwork.</summary>
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
    private static readonly Dictionary<ushort, SporeSpawnCollisionHitbox[]> Lists =
        new()
        {
            [ClosedHead] =
            [
                new(-41, -30, 41, 30, NoOp, Dud),
                new(-16, -45, 15, -30, NoOp, Dud),
            ],
            [OpenHead] =
            [
                new(-41, -30, 41, 30, Touch, Dud),
                new(-16, -45, 15, -30, Touch, Dud),
            ],
            [ExtendedHead] =
            [
                new(-44, -35, 43, 33, Touch, Dud),
                new(-16, -49, 15, -35, Touch, Dud),
            ],
            [MovingHead0] =
            [
                new(-45, -38, 44, -9, Touch, Dud),
                new(-45, 8, 44, 35, Touch, Dud),
                new(-15, -24, 14, 23, Touch, Shot),
                new(-16, -54, 16, -22, Touch, Dud),
            ],
            [MovingHead1] =
            [
                new(-43, -44, 42, -13, Touch, Dud),
                new(-44, 12, 42, 42, Touch, Dud),
                new(-15, -24, 14, 23, Touch, Shot),
                new(-16, -58, 16, -42, Touch, Dud),
            ],
            [MovingHead2] =
            [
                new(-45, -47, 44, -17, Touch, Dud),
                new(-44, 16, 43, 46, Touch, Dud),
                new(-15, -24, 14, 23, Touch, Shot),
                new(-16, -62, 16, -45, Touch, Dud),
            ],
            [MovingHead3] =
            [
                new(-44, -50, 45, -21, Touch, Dud),
                new(-43, 20, 43, 50, Touch, Dud),
                new(-15, -24, 14, 23, Touch, Shot),
                new(-16, -64, 16, -48, Touch, Dud),
            ],
            [TrailingShotPoint] =
                [new(-15, -24, 14, 23, Touch, Shot)],
            [MirroredTrailingShotPoint] =
                [new(-15, -24, 14, 23, Touch, Shot)],
            [TrailingDudPoint] =
                [new(-15, -24, 14, 23, Touch, Dud)],
            [MovingHead4] =
            [
                new(-44, -53, 44, -23, Touch, Dud),
                new(-44, 22, 43, 52, Touch, Dud),
                new(-15, -24, 14, 23, Touch, Shot),
                new(-16, -68, 16, -48, Touch, Dud),
            ],
            [MovingHead5] =
            [
                new(-44, -55, 43, -25, Touch, Dud),
                new(-45, 24, 43, 55, Touch, Dud),
                new(-15, -25, 14, 24, Touch, Shot),
                new(-16, -69, 16, -48, Touch, Dud),
            ],
        };

    internal static int FrameCount => 12;
    internal static int ListCount => Lists.Count;

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

    private static ushort InnerPointForPhase(int phase) => phase switch
    {
        0 => TrailingShotPoint,
        2 => TrailingDudPoint,
        _ => MirroredTrailingShotPoint,
    };
    internal static ReadOnlySpan<SporeSpawnCollisionHitbox> HitboxesAt(
        ushort pointer) =>
        Lists.TryGetValue(pointer, out SporeSpawnCollisionHitbox[]? hitboxes)
            ? hitboxes
            : throw new InvalidDataException(
                $"Spore Spawn hitbox list $A5:{pointer:X4} is not compiled.");
}
