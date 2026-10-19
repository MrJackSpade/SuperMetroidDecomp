using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.Core.Bosses.Torizo;

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
/// frames: X negates and each hitbox list is replaced by its separately fitted right-facing
/// counterpart (<see cref="TorizoHitboxList.RightFacing"/>); every list without collision is
/// <see cref="Empty"/>. Retained as authored: the left-facing placements (torso/leg choreography of each
/// drawing), the six jump-back frames (their two facings are drawn differently), and the list
/// rectangles, which are fitted per facing to the drawings and are not mirrors of each other.
/// </remarks>
internal static class TorizoCollisionDefinitions
{
    /// <summary>Bank $AA owns the ordered frame components.</summary>
    internal const byte Bank = Artwork.TorizoFrameDefinitions.Bank;

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
    /// <summary>The list of every component without collision; it is its own right-facing counterpart.</summary>
    internal static readonly TorizoHitboxList Empty = new([]);

    /// <summary>Right-facing first awakening body, narrower on the left than its left-facing counterpart.</summary>
    private static readonly TorizoHitboxList AwakeningStep1Right = new([new(-15, -27, 13, 27, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]);
    /// <summary>Right-facing second awakening body, spanning X -13 through 13.</summary>
    private static readonly TorizoHitboxList AwakeningStep2Right = new([new(-13, -27, 13, 27, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]);
    /// <summary>Right-facing third awakening rectangle, grown upward to Y -32.</summary>
    private static readonly TorizoHitboxList AwakeningStep3Right = new([new(-14, -32, 13, 33, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]);
    /// <summary>Right-facing fourth awakening rectangle, reaching Y -35 through 39.</summary>
    private static readonly TorizoHitboxList AwakeningStep4Right = new([new(-14, -35, 11, 39, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]);
    /// <summary>Right-facing fifth awakening rectangle, its upper reach shifted toward positive X.</summary>
    private static readonly TorizoHitboxList AwakeningStep5Right = new([new(-6, -42, 13, 47, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]);
    /// <summary>Right-facing sixth awakening rectangle, keeping a 47-pixel lower extent.</summary>
    private static readonly TorizoHitboxList AwakeningStep6Right = new([new(-7, -41, 11, 47, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]);
    /// <summary>Right-facing final awakening rectangle, expanded to X 22.</summary>
    private static readonly TorizoHitboxList AwakeningStep7Right = new([new(-8, -41, 22, 47, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)]);
    /// <summary>Right-facing active body.</summary>
    private static readonly TorizoHitboxList ActiveBodyRight = new([new(-10, -38, 13, 23, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)]);
    /// <summary>Right-facing standing body, extending farther down and right than the right-facing jump-back body.</summary>
    private static readonly TorizoHitboxList StandingBodyRight = new([new(-9, -40, 16, 25, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)]);
    /// <summary>Compact right-facing jump-back body, fitted separately from its left counterpart; the right-facing jump-back frames use it directly.</summary>
    private static readonly TorizoHitboxList JumpBackBodyRight = new([new(-8, -37, 15, 14, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)]);

    /// <summary>First awakening body, 32 by 54 pixels, with the stand-up/sit-down shot reaction.</summary>
    private static readonly TorizoHitboxList AwakeningStep1 = new([new(-16, -27, 16, 27, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStep1Right);
    /// <summary>Second awakening body, narrowed on the left while retaining full height.</summary>
    private static readonly TorizoHitboxList AwakeningStep2 = new([new(-14, -27, 13, 27, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStep2Right);
    /// <summary>Third awakening rectangle, taller before the upper reach expands.</summary>
    private static readonly TorizoHitboxList AwakeningStep3 = new([new(-13, -34, 9, 33, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStep3Right);
    /// <summary>Fourth awakening rectangle, centered and extending farther vertically.</summary>
    private static readonly TorizoHitboxList AwakeningStep4 = new([new(-11, -38, 11, 39, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStep4Right);
    /// <summary>Fifth awakening rectangle, its upper reach shifted left, from Y -44 through 47.</summary>
    private static readonly TorizoHitboxList AwakeningStep5 = new([new(-15, -44, 8, 47, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStep5Right);
    /// <summary>Sixth awakening rectangle, shortened below the center while its left side expands.</summary>
    private static readonly TorizoHitboxList AwakeningStep6 = new([new(-18, -43, 3, 24, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStep6Right);
    /// <summary>Final awakening rectangle, retaining the tall upper reach and narrowing below.</summary>
    private static readonly TorizoHitboxList AwakeningStep7 = new([new(-17, -42, 5, 15, TorizoHitboxTouch.Body, TorizoHitboxShot.StandUpSitDown)], AwakeningStep7Right);
    /// <summary>Left-facing active body, with the ordinary body shot reaction.</summary>
    private static readonly TorizoHitboxList ActiveBody = new([new(-15, -39, 7, 21, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)], ActiveBodyRight);
    /// <summary>Compact left-facing jump-back body with its lower edge at Y 9.</summary>
    private static readonly TorizoHitboxList JumpBackBody = new([new(-18, -38, 7, 9, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)], JumpBackBodyRight);
    /// <summary>Left-facing standing body, extending to Y 18.</summary>
    private static readonly TorizoHitboxList StandingBody = new([new(-18, -37, 7, 18, TorizoHitboxTouch.Body, TorizoHitboxShot.Body)], StandingBodyRight);

    /// <summary>Single origin component used by the native blank and shared turning frames.</summary>
    private static readonly GoldenTorizoCollisionComponent[] SingleEmpty = [new(new(0, 0), Empty)];

    /// <summary>The 42 left-facing frames from $AA:A4FA, in native order.</summary>
    internal static readonly GoldenTorizoCollisionComponent[][] LeftFrames =
    [
        [new(new(-15, -30), Empty), new(new(-5, -24), Empty), new(new(0, 0), StandingBody), new(new(-5, -24), Empty)],
        [new(new(-15, -30), Empty), new(new(-5, -25), Empty), new(new(0, 0), ActiveBody), new(new(-5, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-5, -26), Empty), new(new(0, 0), ActiveBody), new(new(-5, -26), Empty)],
        [new(new(-15, -32), Empty), new(new(-5, -25), Empty), new(new(0, 0), ActiveBody), new(new(-5, -25), Empty)],
        [new(new(-15, -32), Empty), new(new(-5, -24), Empty), new(new(0, 0), ActiveBody), new(new(-5, -24), Empty)],
        [new(new(-15, -30), Empty), new(new(-5, -24), Empty), new(new(0, 0), StandingBody), new(new(-5, -24), Empty)],
        [new(new(-15, -31), Empty), new(new(-5, -25), Empty), new(new(0, 0), ActiveBody), new(new(-5, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-5, -26), Empty), new(new(0, 0), ActiveBody), new(new(-5, -26), Empty)],
        [new(new(-15, -32), Empty), new(new(-5, -25), Empty), new(new(0, 0), ActiveBody), new(new(-5, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-5, -24), Empty), new(new(0, 0), ActiveBody), new(new(-5, -24), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-9, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-9, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-9, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-9, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-9, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-15, -31), Empty), new(new(-4, -25), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
        [new(new(-4, -25), Empty), new(new(-15, -31), Empty), new(new(0, 0), StandingBody), new(new(-4, -25), Empty)],
    ];

    /// <summary>The seven awakening frames from $AA:AA12, in native order.</summary>
    internal static readonly GoldenTorizoCollisionComponent[][] AwakeningFrames =
    [
        [new(new(0, 0), AwakeningStep1)],
        [new(new(0, 0), AwakeningStep2)],
        [new(new(0, 0), AwakeningStep3)],
        [new(new(0, 0), AwakeningStep4)],
        [new(new(-5, -24), Empty), new(new(0, 0), AwakeningStep5)],
        [new(new(-5, -24), Empty), new(new(0, 0), AwakeningStep6)],
        [new(new(-5, -24), Empty), new(new(0, 0), AwakeningStep7)],
    ];

    /// <summary>The six jump-back frames ending the right-facing run (three per facing).</summary>
    internal static readonly GoldenTorizoCollisionComponent[][] JumpBackFrames =
    [
        [new(new(-16, -29), Empty), new(new(-4, -22), Empty), new(new(0, 0), JumpBackBody)],
        [new(new(-16, -30), Empty), new(new(-4, -24), Empty), new(new(0, 0), JumpBackBody)],
        [new(new(-16, -30), Empty), new(new(-3, -24), Empty), new(new(0, 0), JumpBackBody)],
        [new(new(15, -28), Empty), new(new(4, -22), Empty), new(new(0, 0), JumpBackBodyRight)],
        [new(new(15, -29), Empty), new(new(4, -24), Empty), new(new(0, 0), JumpBackBodyRight)],
        [new(new(15, -29), Empty), new(new(3, -24), Empty), new(new(0, 0), JumpBackBodyRight)],
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
}
