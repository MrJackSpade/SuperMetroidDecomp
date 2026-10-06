namespace SuperMetroid.Core.Game;

/// <summary>Physical component of one Golden Torizo extended frame; not editable art.</summary>
internal readonly record struct GoldenTorizoCollisionComponent(
    short X, short Y, ushort HitboxList);

/// <summary>Native ordered touch/shot rectangle from a bank-$AA hitbox list.</summary>
internal readonly record struct GoldenTorizoCollisionHitbox(
    short Left, short Top, short Right, short Bottom,
    ushort TouchAi, ushort ShotAi);

/// <summary>
/// Ordered physical components and touch/shot rectangles for all 106 Torizo frames selected
/// by the compiled bank-$AA instruction lists. OAM composition and pixels are not stored here.
/// The native blank frame has an empty hitbox list, rather than invented collision.
/// </summary>
/// <remarks>
/// Reviewed for #1165; the single owner of the bank-$AA Torizo collision space. Frame records
/// are contiguous within four runs ($87D0 blank, $A4F0 turning plus 42 left-facing frames,
/// $AA12 seven awakening frames, $AA98 right-facing frames), so every pointer derives from its
/// run start and the preceding record sizes (a count word plus eight bytes per component).
/// The first 49 right-facing frames are the exact X mirrors, in order, of the 49 left-facing
/// frames: X negates and each hitbox list is its right-facing counterpart, laid out
/// <see cref="RightFacingListOffset"/> bytes after the left block (the shared empty list $87C7
/// is common). Retained as authored: the left-facing placements (torso/leg choreography of each
/// drawing), the six jump-back frames (their two facings are drawn differently), and the list
/// rectangles, which are fitted per facing to the drawings and are not mirrors of each other.
/// </remarks>
internal static class TorizoCollisionDefinitions
{
    internal const byte Bank = TorizoInstructionProgramDefinitions.Bank;

    /// <summary>$AA:87D0, the native blank frame.</summary>
    private const ushort BlankFrame = 0x87d0;
    /// <summary>$AA:A4F0, the shared facing-screen turning frame.</summary>
    private const ushort TurningFrame = 0xa4f0;
    /// <summary>$AA:A4FA, first left-facing frame after the turning frame.</summary>
    private const ushort LeftStart = 0xa4fa;
    /// <summary>$AA:AA12, first awakening frame.</summary>
    private const ushort AwakeningStart = 0xaa12;
    /// <summary>$AA:AA98, first right-facing frame.</summary>
    private const ushort RightStart = 0xaa98;
    /// <summary>$AA:87C7, empty hitbox list shared by both facings.</summary>
    internal const ushort EmptyList = 0x87c7;
    /// <summary>Right-facing hitbox lists follow the left-facing block by this many bytes.</summary>
    internal const ushort RightFacingListOffset = 0x015e;

    private static readonly GoldenTorizoCollisionComponent[] SingleEmpty = [new(0, 0, EmptyList)];

    /// <summary>The 42 left-facing frames from $AA:A4FA, in native order.</summary>
    private static readonly GoldenTorizoCollisionComponent[][] LeftFrames =
    [
        [new(-15, -30, 0x87c7), new(-5, -24, 0x8858), new(0, 0, 0x892a), new(-5, -24, 0x88dc)],
        [new(-15, -30, 0x87c7), new(-5, -25, 0x8858), new(0, 0, 0x884a), new(-5, -25, 0x88cc)],
        [new(-15, -31, 0x87c7), new(-5, -26, 0x885a), new(0, 0, 0x884a), new(-5, -26, 0x88bc)],
        [new(-15, -32, 0x87c7), new(-5, -25, 0x886a), new(0, 0, 0x884a), new(-5, -25, 0x88ba)],
        [new(-15, -32, 0x87c7), new(-5, -24, 0x887a), new(0, 0, 0x884a), new(-5, -24, 0x88ba)],
        [new(-15, -30, 0x87c7), new(-5, -24, 0x887a), new(0, 0, 0x892a), new(-5, -24, 0x88ba)],
        [new(-15, -31, 0x87c7), new(-5, -25, 0x886a), new(0, 0, 0x884a), new(-5, -25, 0x88ba)],
        [new(-15, -31, 0x87c7), new(-5, -26, 0x885a), new(0, 0, 0x884a), new(-5, -26, 0x88bc)],
        [new(-15, -32, 0x87c7), new(-5, -25, 0x8858), new(0, 0, 0x884a), new(-5, -25, 0x88cc)],
        [new(-15, -31, 0x87c7), new(-5, -24, 0x8858), new(0, 0, 0x884a), new(-5, -24, 0x88dc)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x885a), new(0, 0, 0x892a)],
        [new(-9, -31, 0x87c7), new(-4, -25, 0x886a), new(0, 0, 0x892a)],
        [new(-9, -31, 0x87c7), new(-4, -25, 0x887a), new(0, 0, 0x892a)],
        [new(-9, -31, 0x87c7), new(-4, -25, 0x888a), new(0, 0, 0x892a)],
        [new(-4, -25, 0x889a), new(-9, -31, 0x87c7), new(0, 0, 0x892a)],
        [new(-4, -25, 0x889a), new(-9, -31, 0x87c7), new(0, 0, 0x892a)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x8858), new(0, 0, 0x892a)],
        [new(-9, -31, 0x87c7), new(-4, -25, 0x886a), new(0, 0, 0x892a)],
        [new(-9, -31, 0x87c7), new(-4, -25, 0x887a), new(0, 0, 0x892a)],
        [new(-9, -31, 0x87c7), new(-4, -25, 0x888a), new(0, 0, 0x892a)],
        [new(-4, -25, 0x889a), new(-9, -31, 0x87c7), new(0, 0, 0x892a)],
        [new(-4, -25, 0x889a), new(-9, -31, 0x87c7), new(0, 0, 0x892a)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x886a), new(0, 0, 0x892a)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x886a), new(0, 0, 0x892a), new(-4, -25, 0x88dc)],
        [new(-4, -25, 0x886a), new(-15, -31, 0x87c7), new(0, 0, 0x892a), new(-4, -25, 0x88ec)],
        [new(-4, -25, 0x886a), new(-15, -31, 0x87c7), new(0, 0, 0x892a), new(-4, -25, 0x88fc)],
        [new(-4, -25, 0x886a), new(-15, -31, 0x87c7), new(0, 0, 0x892a), new(-4, -25, 0x890c)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x886a), new(0, 0, 0x892a), new(-4, -25, 0x88ba)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x887a), new(0, 0, 0x892a), new(-4, -25, 0x88ba)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x888a), new(0, 0, 0x892a), new(-4, -25, 0x88ba)],
        [new(-4, -25, 0x889a), new(-15, -31, 0x87c7), new(0, 0, 0x892a), new(-4, -25, 0x88ba)],
        [new(-4, -25, 0x88aa), new(-15, -31, 0x87c7), new(0, 0, 0x892a), new(-4, -25, 0x88ba)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x886a), new(0, 0, 0x892a)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x887a), new(0, 0, 0x892a)],
        [new(-4, -25, 0x888a), new(-15, -31, 0x87c7), new(0, 0, 0x892a)],
        [new(-4, -25, 0x889a), new(-15, -31, 0x87c7), new(0, 0, 0x892a)],
        [new(-4, -25, 0x88aa), new(-15, -31, 0x87c7), new(0, 0, 0x892a)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x885a), new(0, 0, 0x892a), new(-4, -25, 0x88cc)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x885a), new(0, 0, 0x892a), new(-4, -25, 0x88dc)],
        [new(-15, -31, 0x87c7), new(-4, -25, 0x885a), new(0, 0, 0x892a), new(-4, -25, 0x88ec)],
        [new(-4, -25, 0x885a), new(-15, -31, 0x87c7), new(0, 0, 0x892a), new(-4, -25, 0x88fc)],
        [new(-4, -25, 0x885a), new(-15, -31, 0x87c7), new(0, 0, 0x892a), new(-4, -25, 0x890c)],
    ];

    /// <summary>The seven awakening frames from $AA:AA12, in native order.</summary>
    private static readonly GoldenTorizoCollisionComponent[][] AwakeningFrames =
    [
        [new(0, 0, 0x87e8)],
        [new(0, 0, 0x87f6)],
        [new(0, 0, 0x8804)],
        [new(0, 0, 0x8812)],
        [new(-5, -24, 0x885a), new(0, 0, 0x8820)],
        [new(-5, -24, 0x886a), new(0, 0, 0x882e)],
        [new(-5, -24, 0x886a), new(0, 0, 0x883c)],
    ];

    /// <summary>The six jump-back frames ending the right-facing run (three per facing).</summary>
    private static readonly GoldenTorizoCollisionComponent[][] JumpBackFrames =
    [
        [new(-16, -29, 0x87c7), new(-4, -22, 0x885a), new(0, 0, 0x891c)],
        [new(-16, -30, 0x87c7), new(-4, -24, 0x8858), new(0, 0, 0x891c)],
        [new(-16, -30, 0x87c7), new(-3, -24, 0x8858), new(0, 0, 0x891c)],
        [new(15, -28, 0x87c7), new(4, -22, 0x89b8), new(0, 0, 0x8a7a)],
        [new(15, -29, 0x87c7), new(4, -24, 0x89b6), new(0, 0, 0x8a7a)],
        [new(15, -29, 0x87c7), new(3, -24, 0x89b6), new(0, 0, 0x8a7a)],
    ];

    private static int MirroredCount => LeftFrames.Length + AwakeningFrames.Length;

    internal static int FrameCount => 2 + MirroredCount * 2 + JumpBackFrames.Length;

    /// <summary>Every frame pointer, derived from the four runs.</summary>
    internal static IEnumerable<ushort> FramePointers
    {
        get
        {
            yield return BlankFrame;
            yield return TurningFrame;
            ushort cursor = LeftStart;
            foreach (GoldenTorizoCollisionComponent[] frame in LeftFrames)
            {
                yield return cursor;
                cursor = Next(cursor, frame.Length);
            }
            cursor = AwakeningStart;
            foreach (GoldenTorizoCollisionComponent[] frame in AwakeningFrames)
            {
                yield return cursor;
                cursor = Next(cursor, frame.Length);
            }
            cursor = RightStart;
            for (int index = 0; index < MirroredCount; index++)
            {
                yield return cursor;
                cursor = Next(cursor, LeftFacing(index).Length);
            }
            foreach (GoldenTorizoCollisionComponent[] frame in JumpBackFrames)
            {
                yield return cursor;
                cursor = Next(cursor, frame.Length);
            }
        }
    }

    internal static IEnumerable<ushort> HitboxPointers => ListPointers;

    /// <summary>Components of <paramref name="frame"/>; right-facing frames mirror on read.</summary>
    internal static TorizoCollisionComponents ComponentsAt(ushort frame)
    {
        if (frame is BlankFrame or TurningFrame) return new(SingleEmpty, false);
        if (TryFind(LeftFrames, LeftStart, frame, out GoldenTorizoCollisionComponent[]? left) ||
            TryFind(AwakeningFrames, AwakeningStart, frame, out left))
            return new(left!, false);
        ushort cursor = RightStart;
        for (int index = 0; index < MirroredCount; index++)
        {
            GoldenTorizoCollisionComponent[] source = LeftFacing(index);
            if (cursor == frame) return new(source, true);
            cursor = Next(cursor, source.Length);
        }
        foreach (GoldenTorizoCollisionComponent[] jump in JumpBackFrames)
        {
            if (cursor == frame) return new(jump, false);
            cursor = Next(cursor, jump.Length);
        }
        throw new InvalidDataException($"Torizo frame $AA:{frame:X4} is not compiled.");
    }

    internal static bool HasFrame(ushort frame) => TryGetComponents(frame, out _);

    internal static bool TryGetComponents(ushort frame, out TorizoCollisionComponents components)
    {
        foreach (ushort pointer in FramePointers)
        {
            if (pointer != frame) continue;
            components = ComponentsAt(frame);
            return true;
        }
        components = default;
        return false;
    }

    /// <summary>The <paramref name="index"/>th left-facing frame in mirror order.</summary>
    private static GoldenTorizoCollisionComponent[] LeftFacing(int index) =>
        index < LeftFrames.Length ? LeftFrames[index] : AwakeningFrames[index - LeftFrames.Length];

    private static bool TryFind(GoldenTorizoCollisionComponent[][] run, ushort start, ushort frame,
        out GoldenTorizoCollisionComponent[]? components)
    {
        ushort cursor = start;
        foreach (GoldenTorizoCollisionComponent[] candidate in run)
        {
            if (cursor == frame)
            {
                components = candidate;
                return true;
            }
            cursor = Next(cursor, candidate.Length);
        }
        components = null;
        return false;
    }

    private static ushort Next(ushort frame, int components) => (ushort)(frame + 2 + 8 * components);

    private static readonly ushort[] ListPointers =
    [
        0x87c7, 0x87e8, 0x87f6, 0x8804, 0x8812, 0x8820, 0x882e, 0x883c, 0x884a, 0x8858, 0x885a, 0x886a, 0x887a, 0x888a, 0x889a, 0x88aa, 0x88ba, 0x88bc, 0x88cc, 0x88dc, 0x88ec, 0x88fc, 0x890c, 0x891c, 0x892a, 0x8946, 0x8954, 0x8962, 0x8970, 0x897e, 0x898c, 0x899a, 0x89a8, 0x89b6, 0x89b8, 0x89c8, 0x89d8, 0x89e8, 0x89f8, 0x8a08, 0x8a18, 0x8a1a, 0x8a2a, 0x8a3a, 0x8a4a, 0x8a5a, 0x8a6a, 0x8a7a, 0x8a88,
    ];

    private static readonly GoldenTorizoCollisionHitbox[] List87E8 = [new(-16, -27, 16, 27, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List87F6 = [new(-14, -27, 13, 27, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List8804 = [new(-13, -34, 9, 33, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List8812 = [new(-11, -38, 11, 39, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List8820 = [new(-15, -44, 8, 47, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List882E = [new(-18, -43, 3, 24, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List883C = [new(-17, -42, 5, 15, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List884A = [new(-15, -39, 7, 21, 0xc977, 0xc97c)];
    private static readonly GoldenTorizoCollisionHitbox[] List891C = [new(-18, -38, 7, 9, 0xc977, 0xc97c)];
    private static readonly GoldenTorizoCollisionHitbox[] List892A = [new(-18, -37, 7, 18, 0xc977, 0xc97c)];
    private static readonly GoldenTorizoCollisionHitbox[] List8946 = [new(-15, -27, 13, 27, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List8954 = [new(-13, -27, 13, 27, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List8962 = [new(-14, -32, 13, 33, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List8970 = [new(-14, -35, 11, 39, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List897E = [new(-6, -42, 13, 47, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List898C = [new(-7, -41, 11, 47, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List899A = [new(-8, -41, 22, 47, 0xc977, 0xc9c2)];
    private static readonly GoldenTorizoCollisionHitbox[] List89A8 = [new(-10, -38, 13, 23, 0xc977, 0xc97c)];
    private static readonly GoldenTorizoCollisionHitbox[] List8A7A = [new(-8, -37, 15, 14, 0xc977, 0xc97c)];
    private static readonly GoldenTorizoCollisionHitbox[] List8A88 = [new(-9, -40, 16, 25, 0xc977, 0xc97c)];

    /// <summary>Hitbox list rectangles, fitted per facing to the drawings.</summary>
    internal static ReadOnlySpan<GoldenTorizoCollisionHitbox> HitboxesAt(ushort pointer) =>
        pointer switch
        {
            0x87c7 => [],
            0x87e8 => List87E8,
            0x87f6 => List87F6,
            0x8804 => List8804,
            0x8812 => List8812,
            0x8820 => List8820,
            0x882e => List882E,
            0x883c => List883C,
            0x884a => List884A,
            0x8858 => [],
            0x885a => [],
            0x886a => [],
            0x887a => [],
            0x888a => [],
            0x889a => [],
            0x88aa => [],
            0x88ba => [],
            0x88bc => [],
            0x88cc => [],
            0x88dc => [],
            0x88ec => [],
            0x88fc => [],
            0x890c => [],
            0x891c => List891C,
            0x892a => List892A,
            0x8946 => List8946,
            0x8954 => List8954,
            0x8962 => List8962,
            0x8970 => List8970,
            0x897e => List897E,
            0x898c => List898C,
            0x899a => List899A,
            0x89a8 => List89A8,
            0x89b6 => [],
            0x89b8 => [],
            0x89c8 => [],
            0x89d8 => [],
            0x89e8 => [],
            0x89f8 => [],
            0x8a08 => [],
            0x8a18 => [],
            0x8a1a => [],
            0x8a2a => [],
            0x8a3a => [],
            0x8a4a => [],
            0x8a5a => [],
            0x8a6a => [],
            0x8a7a => List8A7A,
            0x8a88 => List8A88,
            _ => throw new InvalidDataException($"Torizo hitbox list $AA:{pointer:X4} is not compiled."),
        };
}

/// <summary>Allocation-free view of one frame's components, mirrored for right-facing frames.</summary>
internal readonly struct TorizoCollisionComponents(GoldenTorizoCollisionComponent[] source, bool mirrored)
{
    internal int Length => source.Length;

    internal GoldenTorizoCollisionComponent this[int index]
    {
        get
        {
            GoldenTorizoCollisionComponent part = source[index];
            if (!mirrored) return part;
            ushort list = part.HitboxList == TorizoCollisionDefinitions.EmptyList
                ? TorizoCollisionDefinitions.EmptyList
                : (ushort)(part.HitboxList + TorizoCollisionDefinitions.RightFacingListOffset);
            return part with { X = (short)-part.X, HitboxList = list };
        }
    }

    public Enumerator GetEnumerator() => new(this);

    internal struct Enumerator(TorizoCollisionComponents view)
    {
        private int index = -1;
        public bool MoveNext() => ++index < view.Length;
        public readonly GoldenTorizoCollisionComponent Current => view[index];
    }
}
