using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.Core.Game;

/// <summary>Physical component of one Golden Torizo extended frame; not editable art.</summary>
/// <param name="Offset">Component position relative to Torizo's room origin, reflected for mirrored right-facing frames.</param>
/// <param name="HitboxList">Bank-$AA pointer to ordered touch and shot rectangles, or the shared empty list.</param>
internal readonly record struct GoldenTorizoCollisionComponent(
    Offset16<RoomPixels> Offset, ushort HitboxList);

/// <summary>Native ordered touch/shot rectangle from a bank-$AA hitbox list.</summary>
/// <param name="Left">Left extent relative to the component origin.</param>
/// <param name="Top">Upper extent relative to the component origin.</param>
/// <param name="Right">Right extent relative to the component origin.</param>
/// <param name="Bottom">Lower extent relative to the component origin.</param>
/// <param name="TouchAi">Native callback pointer for Samus contact.</param>
/// <param name="ShotAi">Native callback pointer for projectile contact.</param>
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
    /// <summary>Bank $AA owns both the ordered frame components and their hitbox-list pointers.</summary>
    internal const byte Bank = TorizoInstructionProgramDefinitions.Bank;

    /// <summary>The two frames whose collision is a single empty component.</summary>
    internal enum EmptyFrame : ushort
    {
        /// <summary>$AA:87D0, the native blank frame.</summary>
        Blank = 0x87d0,
        /// <summary>$AA:A4F0, the shared facing-screen turning frame.</summary>
        Turning = 0xa4f0,
    }
    /// <summary>$AA:A4FA, first left-facing frame after the turning frame.</summary>
    internal const ushort LeftStart = 0xa4fa;
    /// <summary>$AA:AA12, first awakening frame.</summary>
    internal const ushort AwakeningStart = 0xaa12;
    /// <summary>$AA:AA98, first right-facing frame.</summary>
    internal const ushort RightStart = 0xaa98;
    /// <summary>$AA:87C7, empty hitbox list shared by both facings.</summary>
    internal const ushort EmptyList = 0x87c7;
    /// <summary>Right-facing hitbox lists follow the left-facing block by this many bytes.</summary>
    internal const ushort RightFacingListOffset = 0x015e;

    /// <summary>Single origin component used by the native blank and shared turning frames.</summary>
    private static readonly GoldenTorizoCollisionComponent[] SingleEmpty = [new(new(0, 0), EmptyList)];

    /// <summary>The 42 left-facing frames from $AA:A4FA, in native order.</summary>
    internal static readonly GoldenTorizoCollisionComponent[][] LeftFrames =
    [
        [new(new(-15, -30), 0x87c7), new(new(-5, -24), 0x8858), new(new(0, 0), 0x892a), new(new(-5, -24), 0x88dc)],
        [new(new(-15, -30), 0x87c7), new(new(-5, -25), 0x8858), new(new(0, 0), 0x884a), new(new(-5, -25), 0x88cc)],
        [new(new(-15, -31), 0x87c7), new(new(-5, -26), 0x885a), new(new(0, 0), 0x884a), new(new(-5, -26), 0x88bc)],
        [new(new(-15, -32), 0x87c7), new(new(-5, -25), 0x886a), new(new(0, 0), 0x884a), new(new(-5, -25), 0x88ba)],
        [new(new(-15, -32), 0x87c7), new(new(-5, -24), 0x887a), new(new(0, 0), 0x884a), new(new(-5, -24), 0x88ba)],
        [new(new(-15, -30), 0x87c7), new(new(-5, -24), 0x887a), new(new(0, 0), 0x892a), new(new(-5, -24), 0x88ba)],
        [new(new(-15, -31), 0x87c7), new(new(-5, -25), 0x886a), new(new(0, 0), 0x884a), new(new(-5, -25), 0x88ba)],
        [new(new(-15, -31), 0x87c7), new(new(-5, -26), 0x885a), new(new(0, 0), 0x884a), new(new(-5, -26), 0x88bc)],
        [new(new(-15, -32), 0x87c7), new(new(-5, -25), 0x8858), new(new(0, 0), 0x884a), new(new(-5, -25), 0x88cc)],
        [new(new(-15, -31), 0x87c7), new(new(-5, -24), 0x8858), new(new(0, 0), 0x884a), new(new(-5, -24), 0x88dc)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x885a), new(new(0, 0), 0x892a)],
        [new(new(-9, -31), 0x87c7), new(new(-4, -25), 0x886a), new(new(0, 0), 0x892a)],
        [new(new(-9, -31), 0x87c7), new(new(-4, -25), 0x887a), new(new(0, 0), 0x892a)],
        [new(new(-9, -31), 0x87c7), new(new(-4, -25), 0x888a), new(new(0, 0), 0x892a)],
        [new(new(-4, -25), 0x889a), new(new(-9, -31), 0x87c7), new(new(0, 0), 0x892a)],
        [new(new(-4, -25), 0x889a), new(new(-9, -31), 0x87c7), new(new(0, 0), 0x892a)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x8858), new(new(0, 0), 0x892a)],
        [new(new(-9, -31), 0x87c7), new(new(-4, -25), 0x886a), new(new(0, 0), 0x892a)],
        [new(new(-9, -31), 0x87c7), new(new(-4, -25), 0x887a), new(new(0, 0), 0x892a)],
        [new(new(-9, -31), 0x87c7), new(new(-4, -25), 0x888a), new(new(0, 0), 0x892a)],
        [new(new(-4, -25), 0x889a), new(new(-9, -31), 0x87c7), new(new(0, 0), 0x892a)],
        [new(new(-4, -25), 0x889a), new(new(-9, -31), 0x87c7), new(new(0, 0), 0x892a)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x886a), new(new(0, 0), 0x892a)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x886a), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88dc)],
        [new(new(-4, -25), 0x886a), new(new(-15, -31), 0x87c7), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88ec)],
        [new(new(-4, -25), 0x886a), new(new(-15, -31), 0x87c7), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88fc)],
        [new(new(-4, -25), 0x886a), new(new(-15, -31), 0x87c7), new(new(0, 0), 0x892a), new(new(-4, -25), 0x890c)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x886a), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88ba)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x887a), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88ba)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x888a), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88ba)],
        [new(new(-4, -25), 0x889a), new(new(-15, -31), 0x87c7), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88ba)],
        [new(new(-4, -25), 0x88aa), new(new(-15, -31), 0x87c7), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88ba)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x886a), new(new(0, 0), 0x892a)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x887a), new(new(0, 0), 0x892a)],
        [new(new(-4, -25), 0x888a), new(new(-15, -31), 0x87c7), new(new(0, 0), 0x892a)],
        [new(new(-4, -25), 0x889a), new(new(-15, -31), 0x87c7), new(new(0, 0), 0x892a)],
        [new(new(-4, -25), 0x88aa), new(new(-15, -31), 0x87c7), new(new(0, 0), 0x892a)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x885a), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88cc)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x885a), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88dc)],
        [new(new(-15, -31), 0x87c7), new(new(-4, -25), 0x885a), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88ec)],
        [new(new(-4, -25), 0x885a), new(new(-15, -31), 0x87c7), new(new(0, 0), 0x892a), new(new(-4, -25), 0x88fc)],
        [new(new(-4, -25), 0x885a), new(new(-15, -31), 0x87c7), new(new(0, 0), 0x892a), new(new(-4, -25), 0x890c)],
    ];

    /// <summary>The seven awakening frames from $AA:AA12, in native order.</summary>
    internal static readonly GoldenTorizoCollisionComponent[][] AwakeningFrames =
    [
        [new(new(0, 0), 0x87e8)],
        [new(new(0, 0), 0x87f6)],
        [new(new(0, 0), 0x8804)],
        [new(new(0, 0), 0x8812)],
        [new(new(-5, -24), 0x885a), new(new(0, 0), 0x8820)],
        [new(new(-5, -24), 0x886a), new(new(0, 0), 0x882e)],
        [new(new(-5, -24), 0x886a), new(new(0, 0), 0x883c)],
    ];

    /// <summary>The six jump-back frames ending the right-facing run (three per facing).</summary>
    internal static readonly GoldenTorizoCollisionComponent[][] JumpBackFrames =
    [
        [new(new(-16, -29), 0x87c7), new(new(-4, -22), 0x885a), new(new(0, 0), 0x891c)],
        [new(new(-16, -30), 0x87c7), new(new(-4, -24), 0x8858), new(new(0, 0), 0x891c)],
        [new(new(-16, -30), 0x87c7), new(new(-3, -24), 0x8858), new(new(0, 0), 0x891c)],
        [new(new(15, -28), 0x87c7), new(new(4, -22), 0x89b8), new(new(0, 0), 0x8a7a)],
        [new(new(15, -29), 0x87c7), new(new(4, -24), 0x89b6), new(new(0, 0), 0x8a7a)],
        [new(new(15, -29), 0x87c7), new(new(3, -24), 0x89b6), new(new(0, 0), 0x8a7a)],
    ];

    /// <summary>Number of left and awakening frames whose right-facing counterparts derive by reflection.</summary>
    internal static int MirroredCount => LeftFrames.Length + AwakeningFrames.Length;

    /// <summary>Components of <paramref name="frame"/>; right-facing frames mirror on read.</summary>
    internal static TorizoCollisionComponents ComponentsAt(ushort frame)
    {
        if (Enum.IsDefined((EmptyFrame)frame)) return new(SingleEmpty, false);
        if (TryFind(LeftFrames, LeftStart, frame, out GoldenTorizoCollisionComponent[]? left) ||
            TryFind(AwakeningFrames, AwakeningStart, frame, out left))
            return new(left, false);
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

    /// <summary>The <paramref name="index"/>th left-facing frame in mirror order.</summary>
    internal static GoldenTorizoCollisionComponent[] LeftFacing(int index) =>
        index < LeftFrames.Length ? LeftFrames[index] : AwakeningFrames[index - LeftFrames.Length];

    /// <summary>Walks variable-size native frame records from a run start and selects the exact pointer if present.</summary>
    private static bool TryFind(GoldenTorizoCollisionComponent[][] run, ushort start, ushort frame,
        [NotNullWhen(true)] out GoldenTorizoCollisionComponent[]? components)
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

    /// <summary>Advances past the native count word and all eight-byte component records.</summary>
    internal static ushort Next(ushort frame, int components) => (ushort)(frame + 2 + 8 * components);

    /// <summary>$AA:87E8: first awakening body, 32 by 54 pixels with the shared touch/shot pair.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List87E8 = [new(-16, -27, 16, 27, 0xc977, 0xc9c2)];
    /// <summary>$AA:87F6: awakening body narrows on the left while retaining full height.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List87F6 = [new(-14, -27, 13, 27, 0xc977, 0xc9c2)];
    /// <summary>$AA:8804: taller awakening rectangle before the upper reach expands.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List8804 = [new(-13, -34, 9, 33, 0xc977, 0xc9c2)];
    /// <summary>$AA:8812: centered awakening rectangle extending farther vertically.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List8812 = [new(-11, -38, 11, 39, 0xc977, 0xc9c2)];
    /// <summary>$AA:8820: left-shifted upper awakening reach, from Y -44 through 47.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List8820 = [new(-15, -44, 8, 47, 0xc977, 0xc9c2)];
    /// <summary>$AA:882E: awakening reach shortens below the center while its left side expands.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List882E = [new(-18, -43, 3, 24, 0xc977, 0xc9c2)];
    /// <summary>$AA:883C: final awakening rectangle retains the tall upper reach and narrows below.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List883C = [new(-17, -42, 5, 15, 0xc977, 0xc9c2)];
    /// <summary>$AA:884A: left-facing active body switches to the $C97C shot callback.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List884A = [new(-15, -39, 7, 21, 0xc977, 0xc97c)];
    /// <summary>$AA:891C: compact left-facing jump-back body with lower edge at Y 9.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List891C = [new(-18, -38, 7, 9, 0xc977, 0xc97c)];
    /// <summary>$AA:892A: left-facing active body extends to Y 18 during standing frames.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List892A = [new(-18, -37, 7, 18, 0xc977, 0xc97c)];
    /// <summary>$AA:8946: right-facing awakening body keeps the shared $C9C2 shot callback.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List8946 = [new(-15, -27, 13, 27, 0xc977, 0xc9c2)];
    /// <summary>$AA:8954: right-facing awakening body spans X -13 through 13.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List8954 = [new(-13, -27, 13, 27, 0xc977, 0xc9c2)];
    /// <summary>$AA:8962: right-facing awakening rectangle grows upward to Y -32.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List8962 = [new(-14, -32, 13, 33, 0xc977, 0xc9c2)];
    /// <summary>$AA:8970: right-facing awakening rectangle reaches Y -35 through 39.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List8970 = [new(-14, -35, 11, 39, 0xc977, 0xc9c2)];
    /// <summary>$AA:897E: right-facing upper reach shifts toward positive X.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List897E = [new(-6, -42, 13, 47, 0xc977, 0xc9c2)];
    /// <summary>$AA:898C: right-facing upper reach keeps a 47-pixel lower extent.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List898C = [new(-7, -41, 11, 47, 0xc977, 0xc9c2)];
    /// <summary>$AA:899A: right-facing awakening rectangle expands to X 22.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List899A = [new(-8, -41, 22, 47, 0xc977, 0xc9c2)];
    /// <summary>$AA:89A8: right-facing active body uses the $C97C shot callback.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List89A8 = [new(-10, -38, 13, 23, 0xc977, 0xc97c)];
    /// <summary>$AA:8A7A: compact right-facing jump-back body, fitted separately from its left counterpart.</summary>
    private static readonly GoldenTorizoCollisionHitbox[] List8A7A = [new(-8, -37, 15, 14, 0xc977, 0xc97c)];
    /// <summary>$AA:8A88: right-facing active body extends farther down and right than the jump-back list.</summary>
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
    /// <summary>Number of ordered native components in this frame's collision drawing.</summary>
    internal int Length => source.Length;

    /// <summary>Projects one component into the requested facing, reflecting X and selecting the paired right-facing hitbox list when needed.</summary>
    internal GoldenTorizoCollisionComponent this[int index]
    {
        get
        {
            GoldenTorizoCollisionComponent part = source[index];
            if (!mirrored) return part;
            ushort list = part.HitboxList == TorizoCollisionDefinitions.EmptyList
                ? TorizoCollisionDefinitions.EmptyList
                : (ushort)(part.HitboxList + TorizoCollisionDefinitions.RightFacingListOffset);
            return part with { Offset = part.Offset.MirroredX(), HitboxList = list };
        }
    }

    /// <summary>Enumerates components without materializing reflected frame records.</summary>
    public Enumerator GetEnumerator() => new(this);

    /// <summary>Index-based enumerator over one frame's reflected or original components.</summary>
    internal struct Enumerator(TorizoCollisionComponents view)
    {
        /// <summary>Current position before the next component, initially before index zero.</summary>
        private int index = -1;
        /// <summary>Advances to the next component while retaining native component order.</summary>
        public bool MoveNext() => ++index < view.Length;
        /// <summary>Resolves the current component through the view's facing projection.</summary>
        public readonly GoldenTorizoCollisionComponent Current => view[index];
    }
}
