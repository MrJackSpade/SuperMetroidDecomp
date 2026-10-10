namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control and visual-selector words for all seven Ceres
/// door/control-actor variants. Spritemap payloads remain separate artwork.
/// </summary>
internal abstract class CeresDoorInstructionProgramDefinitions
{
    /// <summary>Native Ceres door instruction and visual bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The initial Ceres door spritemap at $A6:FAC7.</summary>
    internal const ushort InitialSpritemap = 0xfac7;
    /// <summary>The Ridley-room private door overlay spritemap at $A6:A329.</summary>
    internal const ushort RidleyPrivateOverlaySpritemap = 0xa329;
    /// <summary>
    /// <c>InstList_CeresDoor_RidleysRoom_FacingRight_0</c> at
    /// $A6:F53A-$A6:F56B. An intangible/invisible $0002 frame at $F53E
    /// precedes four visible $0002 frames at $F546 + 4*i for i = 0..3.
    /// A $0001 frame at $F558 sets the Ridley-drawn handoff, then hides
    /// the door before <see cref="RidleyRoomFacingRightWait"/>.
    /// </summary>
    internal const ushort RidleyRoomFacingRight = 0xf53a;
    /// <summary>
    /// <c>InstList_CeresDoor_RidleysRoom_FacingRight_1</c> at $A6:F55E.
    /// Its $0002 frame repeats through the $F66A boss-alive branch at
    /// $F562. After defeat, $F6B0 restores visibility and clears the
    /// Ridley-drawn flag; $80ED at $F568 goes to
    /// <see cref="ClosedFacingRight"/> at $F598.
    /// </summary>
    internal const ushort RidleyRoomFacingRightWait = 0xf55e;
    /// <summary>
    /// <c>InstList_CeresDoor_Normal_FacingRight</c> at $A6:F56C-$A6:F5BD.
    /// The intangible/invisible entry holds one $0002 frame; $F63E at
    /// $F574 branches to <see cref="ClosedFacingRight"/> when Samus is at
    /// least $0030 pixels away on either axis. Otherwise it enters
    /// <see cref="OpenFacingRight"/>. This is the variant-zero entry.
    /// </summary>
    internal const ushort NormalFacingRight = 0xf56c;
    /// <summary>
    /// <c>InstList_CeresDoor_Open_FacingRight_0</c> at $A6:F578.
    /// Its $0002 frame loops through $80ED while Samus is near; the
    /// $F63E distance branch targets <see cref="CloseFacingRight"/>.
    /// </summary>
    internal const ushort OpenFacingRight = 0xf578;
    /// <summary>
    /// <c>InstList_CeresDoor_Open_FacingRight_1</c> at $A6:F584.
    /// After making the door tangible and visible, duration word
    /// $F588 + 4*i is exactly $0005 for i = 0..3; the four frames
    /// fall through to <see cref="ClosedFacingRight"/>.
    /// </summary>
    internal const ushort CloseFacingRight = 0xf584;
    /// <summary>
    /// <c>InstList_CeresDoor_Closed_FacingRight_0</c> at $A6:F598 makes
    /// the door tangible and visible before the closed wait segment.
    /// </summary>
    internal const ushort ClosedFacingRight = 0xf598;
    /// <summary>
    /// <c>InstList_CeresDoor_Closed_FacingRight_1</c> at $A6:F59C.
    /// The $0002 closed frame repeats while Samus is distant. When near,
    /// the door queues its opening sound and duration word $F5A6 + 4*i
    /// is exactly $0005 for i = 0..3. After these four frames it becomes
    /// intangible/invisible and jumps to <see cref="OpenFacingRight"/>.
    /// </summary>
    internal const ushort ClosedFacingRightWait = 0xf59c;
    /// <summary>
    /// <c>InstList_CeresDoor_Normal_FacingLeft_0</c> at $A6:F5BE-$A6:F60F.
    /// Its thirty mechanics word addresses are the right-facing program's
    /// addresses plus $0052. Non-target values match exactly; all five
    /// local branch/goto targets are also relocated by $0052. The live
    /// spritemap operands have distinct facing-left values.
    /// </summary>
    internal const ushort NormalFacingLeft = 0xf5be;
    /// <summary>
    /// <c>InstList_CeresDoor_RotatingElevRoom_PreExploDoorOverlay_0</c>
    /// at $A6:F610-$A6:F619. Variant two executes $F68B once to make
    /// the overlay intangible before its one-tick frame loop.
    /// </summary>
    internal const ushort RotatingElevatorPreExplosionOverlay = 0xf610;
    /// <summary>
    /// <c>InstList_CeresDoor_RotatingElevRoom_PreExploDoorOverlay_1</c>
    /// at $A6:F612 contains duration $0001 and a live spritemap operand.
    /// The $80ED goto at $F616 targets $F612, skipping the one-time setup.
    /// </summary>
    internal const ushort RotatingElevatorPreExplosionOverlayLoop = 0xf612;
    /// <summary>
    /// <c>InstList_CeresDoor_RotatingElevatorRoom_InvisibleWall_0</c>
    /// at $A6:F61A-$A6:F629. Variant four's $F678 callback branches to
    /// <see cref="NormalFacingLeft"/> if Ridley has not escaped.
    /// Otherwise the wall becomes tangible but invisible before its loop.
    /// </summary>
    internal const ushort RotatingElevatorInvisibleWall = 0xf61a;
    /// <summary>
    /// <c>InstList_CeresDoor_RotatingElevatorRoom_InvisibleWall_1</c>
    /// at $A6:F622 holds a $0001 frame with a live spritemap operand.
    /// The $80ED goto at $F626 returns to $F622 without repeating setup.
    /// </summary>
    internal const ushort RotatingElevatorInvisibleWallLoop = 0xf622;
    /// <summary>
    /// <c>InstList_CeresDoor_RidleyEscapeMode7LeftWall_0</c> at
    /// $A6:F62A-$A6:F633. Variant five makes the wall intangible once;
    /// its frame loop starts at <see cref="RidleyEscapeMode7LeftWallLoop"/>.
    /// </summary>
    internal const ushort RidleyEscapeMode7LeftWall = 0xf62a;
    /// <summary>
    /// <c>InstList_CeresDoor_RidleyEscapeMode7LeftWall_1</c> at $A6:F62C
    /// holds duration $0001 and a live spritemap operand. The $80ED
    /// goto at $F630 returns here, skipping one-time setup.
    /// </summary>
    internal const ushort RidleyEscapeMode7LeftWallLoop = 0xf62c;
    /// <summary>
    /// <c>InstList_CeresDoor_RidleyEscapeMode7RightWall_0</c> at
    /// $A6:F634-$A6:F63D is the variant-six counterpart. Its four
    /// mechanics word addresses equal the left program's plus $000A;
    /// its local goto target is relocated by the same amount.
    /// </summary>
    internal const ushort RidleyEscapeMode7RightWall = 0xf634;
    /// <summary>
    /// <c>InstList_CeresDoor_RidleyEscapeMode7RightWall_1</c> at $A6:F636
    /// holds duration $0001 and a live spritemap operand; $80ED at
    /// $F63A returns here without repeating intangible setup.
    /// </summary>
    internal const ushort RidleyEscapeMode7RightWallLoop = 0xf636;

    /// <summary>Mechanics follow the Ridley handoff, two relocated normal doors, and four wall actors.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < 18)
            return RidleyWord(index);
        if (index < 78)
            return NormalDoorWord((index - 18) % 30, (index - 18) / 30);
        index -= 78;
        if (index < 4)
            return WallLoopWord(RotatingElevatorPreExplosionOverlay, index);
        if (index < 11)
        {
            int local = index - 4;
            return local switch
            {
                0 => new(RotatingElevatorInvisibleWall, (ushort)CeresDoorInstruction.GotoYIfCeresRidleyHasNotEscaped),
                1 => new(RotatingElevatorInvisibleWall + 2, NormalFacingLeft),
                2 => new(RotatingElevatorInvisibleWall + 4, (ushort)CeresDoorInstruction.SetAsTangible),
                3 => new(RotatingElevatorInvisibleWall + 6, (ushort)CeresDoorInstruction.SetAsInvisible),
                _ => WallLoopWord(RotatingElevatorInvisibleWallLoop - 2, local - 3),
            };
        }
        index -= 11;
        return WallLoopWord((ushort)(RidleyEscapeMode7LeftWall + 10 * (index / 4)), index % 4);
    }

    private static InstructionMechanicsWord WallLoopWord(ushort start, int index) => index switch
    {
        0 => new(start, (ushort)CeresDoorInstruction.SetAsIntangible),
        1 => new((ushort)(start + 2), 1),
        2 => new((ushort)(start + 6), (ushort)CommonEnemyInstruction.Goto),
        _ => new((ushort)(start + 8), (ushort)(start + 2)),
    };

    private static InstructionMechanicsWord NormalDoorWord(int index, int facing)
    {
        int start = NormalFacingRight + facing * (NormalFacingLeft - NormalFacingRight);
        if (index is >= 12 and < 16)
            return new((ushort)(start + 28 + 4 * (index - 12)), 5);
        if (index is >= 22 and < 26)
            return new((ushort)(start + 58 + 4 * (index - 22)), 5);
        (int offset, ushort value) = index switch
        {
            0 => (0, (ushort)CeresDoorInstruction.SetAsIntangible),
            1 => (2, (ushort)CeresDoorInstruction.SetAsInvisible),
            2 => (4, (ushort)2),
            3 => (8, (ushort)CeresDoorInstruction.GotoYIfSamusIsDistant),
            4 => (10, (ushort)(start + ClosedFacingRight - NormalFacingRight)),
            5 => (12, (ushort)2),
            6 => (16, (ushort)CeresDoorInstruction.GotoYIfSamusIsDistant),
            7 => (18, (ushort)(start + CloseFacingRight - NormalFacingRight)),
            8 => (20, (ushort)CommonEnemyInstruction.Goto),
            9 => (22, (ushort)(start + OpenFacingRight - NormalFacingRight)),
            10 => (24, (ushort)CeresDoorInstruction.SetAsTangible),
            11 => (26, (ushort)CeresDoorInstruction.SetAsVisible),
            16 => (44, (ushort)CeresDoorInstruction.SetAsTangible),
            17 => (46, (ushort)CeresDoorInstruction.SetAsVisible),
            18 => (48, (ushort)2),
            19 => (52, (ushort)CeresDoorInstruction.GotoYIfSamusIsDistant),
            20 => (54, (ushort)(start + ClosedFacingRightWait - NormalFacingRight)),
            21 => (56, (ushort)CeresDoorInstruction.QueueOpeningSFX),
            26 => (74, (ushort)CeresDoorInstruction.SetAsIntangible),
            27 => (76, (ushort)CeresDoorInstruction.SetAsInvisible),
            28 => (78, (ushort)CommonEnemyInstruction.Goto),
            _ => (80, (ushort)(start + OpenFacingRight - NormalFacingRight)),
        };
        return new((ushort)(start + offset), value);
    }

    private static InstructionMechanicsWord RidleyWord(int index)
    {
        if (index is >= 5 and < 9)
            return new((ushort)(RidleyRoomFacingRight + 12 + 4 * (index - 5)), 2);
        (int offset, ushort value) = index switch
        {
            0 => (0, (ushort)CeresDoorInstruction.SetAsIntangible),
            1 => (2, (ushort)CeresDoorInstruction.SetAsInvisible),
            2 => (4, (ushort)2),
            3 => (8, (ushort)CeresDoorInstruction.SetAsTangible),
            4 => (10, (ushort)CeresDoorInstruction.SetAsVisible),
            9 => (28, (ushort)CeresDoorInstruction.SetDrawnByRidleyFlag),
            10 => (30, (ushort)1),
            11 => (34, (ushort)CeresDoorInstruction.SetAsInvisible),
            12 => (36, (ushort)2),
            13 => (40, (ushort)CeresDoorInstruction.GotoYIfAreaBossIsAlive),
            14 => (42, RidleyRoomFacingRightWait),
            15 => (44, (ushort)CeresDoorInstruction.SetAsVisibleClearDrawnByRidleyFlag),
            16 => (46, (ushort)CommonEnemyInstruction.Goto),
            _ => (48, ClosedFacingRight),
        };
        return new((ushort)(RidleyRoomFacingRight + offset), value);
    }
    /// <summary>Spritemap_CeresDoor_RotatingElevRoomPreExplosionDoorOverlay at $A6:F921.</summary>
    private const ushort ElevatorOverlaySpritemap = 0xf921;
    /// <summary>Spritemap_CeresDoor_FacingLeft_Closed at $A6:F95F, followed by both facing pose sets.</summary>
    private const ushort LeftClosedSpritemap = 0xf95f;
    /// <summary>Spritemap_CeresDoor_RidleyEscapeMode7LeftWall at $A6:FACE.</summary>
    private const ushort LeftWallSpritemap = 0xface;
    /// <summary>Spritemap_CeresDoor_RidleyEscapeMode7RightWall at $A6:FB2F.</summary>
    private const ushort RightWallSpritemap = 0xfb2f;

    public static int MechanicsWordCount => 97;
    public static int PresentationWordCount => 33;

    /// <summary>
    /// $A6:F95F-FAC6 contains five opening poses per facing. Four inner OAM parts
    /// accompany four outer parts for the closed/first-opening pose, then two outer
    /// parts for the remaining poses. A record is a two-byte count plus five bytes
    /// per part: the first two widths are 42 and the final three 32. This derives
    /// selector identity only; independently authored part geometry remains artwork.
    /// </summary>
    private static ushort DoorPose(bool facingRight, int openingPhase)
    {
        const int wideRecord = 2 + 5 * (4 + 4);
        const int narrowRecord = 2 + 5 * (4 + 2);
        int facingBytes = 2 * wideRecord + 3 * narrowRecord;
        return (ushort)(LeftClosedSpritemap + (facingRight ? facingBytes : 0) +
            Math.Min(openingPhase, 2) * wideRecord + Math.Max(openingPhase - 2, 0) * narrowRecord);
    }

    internal static (ushort Address, ushort Frame) PresentationWord(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        if (index < 7)
        {
            if (index is >= 1 and <= 4)
                return ((ushort)(RidleyRoomFacingRight + 14 + 4 * (index - 1)), DoorPose(true, 5 - index));
            return index switch
            {
                0 => ((ushort)(RidleyRoomFacingRight + 6), DoorPose(true, 4)),
                5 => ((ushort)(RidleyRoomFacingRight + 32), DoorPose(true, 0)),
                _ => ((ushort)(RidleyRoomFacingRightWait + 2), DoorPose(true, 0)),
            };
        }
        if (index < 29)
        {
            int local = (index - 7) % 11;
            bool facingRight = index < 18;
            int start = facingRight ? NormalFacingRight : NormalFacingLeft;
            if (local is >= 2 and <= 5)
                return ((ushort)(start + 30 + 4 * (local - 2)), DoorPose(facingRight, 6 - local));
            if (local >= 7)
                return ((ushort)(start + 60 + 4 * (local - 7)), DoorPose(facingRight, local - 6));
            return local switch
            {
                // Both invisible initial entries select the right-facing closed map.
                0 => ((ushort)(start + 6), DoorPose(true, 0)),
                // The native left hold uses its closed map; its actor is invisible here.
                1 => ((ushort)(start + 14), DoorPose(facingRight, facingRight ? 4 : 0)),
                _ => ((ushort)(start + 50), DoorPose(facingRight, 0)),
            };
        }
        return index switch
        {
            29 => ((ushort)(RotatingElevatorPreExplosionOverlayLoop + 2), ElevatorOverlaySpritemap),
            30 => ((ushort)(RotatingElevatorInvisibleWallLoop + 2), DoorPose(false, 0)),
            31 => ((ushort)(RidleyEscapeMode7LeftWallLoop + 2), LeftWallSpritemap),
            _ => ((ushort)(RidleyEscapeMode7RightWallLoop + 2), RightWallSpritemap),
        };
    }
    /// <summary>Returns the authored spritemap selector at a bank-$A6 operand.</summary>
    internal static ushort ReadPresentationFrame(ushort address)
    {
        int low = 0;
        int high = PresentationWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            var candidate = PresentationWord(middle);
            if (candidate.Address == address)
                return candidate.Frame;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Ceres door visual operand $A6:{address:X4} is not compiled.");
    }

    /// <summary>Returns fixed Ceres door control or rejects pointers outside the authored programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Ceres door instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }
}
