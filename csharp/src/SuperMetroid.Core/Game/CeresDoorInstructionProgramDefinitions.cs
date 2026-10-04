namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A6 address.</summary>
internal readonly record struct CeresDoorInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control and visual-selector words for all seven Ceres
/// door/control-actor variants. Spritemap payloads remain separate artwork.
/// </summary>
internal static class CeresDoorInstructionProgramDefinitions
{
    /// <summary>Native Ceres door instruction and visual bank $A6.</summary>
    internal const byte Bank = 0xa6;
    /// <summary><c>Enemy_CeresDoor</c>, the bank-$A6 enemy definition at $A6:E23F.</summary>
    internal const ushort EnemyDefinitionPointer = 0xe23f;
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
    /// <c>InstList_CeresDoor_Normal_FacingLeft_1</c> at $A6:F5CA holds a
    /// $0002 open frame while Samus is near; the distance branch targets
    /// <see cref="CloseFacingLeft"/>.
    /// </summary>
    internal const ushort OpenFacingLeft = 0xf5ca;
    /// <summary>
    /// <c>InstList_CeresDoor_Normal_FacingLeft_2</c> at $A6:F5D6.
    /// The four closing duration words at $F5DA + 4*i are exactly
    /// $0005 for i = 0..3, then fall through to the closed state.
    /// </summary>
    internal const ushort CloseFacingLeft = 0xf5d6;
    /// <summary>
    /// <c>InstList_CeresDoor_Normal_FacingLeft_3</c> at $A6:F5EA
    /// makes the door tangible and visible before its closed wait.
    /// </summary>
    internal const ushort ClosedFacingLeft = 0xf5ea;
    /// <summary>
    /// <c>InstList_CeresDoor_Normal_FacingLeft_4</c> at $A6:F5EE
    /// holds a $0002 closed frame while Samus is distant. On approach it
    /// queues opening sound, runs four $0005 durations at $F5F8 + 4*i
    /// for i = 0..3, then returns to <see cref="OpenFacingLeft"/>.
    /// </summary>
    internal const ushort ClosedFacingLeftWait = 0xf5ee;
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
    internal static CeresDoorInstructionMechanicsWord MechanicsWord(int index)
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
                0 => new(RotatingElevatorInvisibleWall, EnemyInstructionCodePointers.Instruction_CeresDoor_GotoYIfCeresRidleyHasNotEscaped),
                1 => new(RotatingElevatorInvisibleWall + 2, NormalFacingLeft),
                2 => new(RotatingElevatorInvisibleWall + 4, CeresEnemyCodePointers.MakeCeresDoorTangible),
                3 => new(RotatingElevatorInvisibleWall + 6, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
                _ => WallLoopWord(RotatingElevatorInvisibleWallLoop - 2, local - 3),
            };
        }
        index -= 11;
        return WallLoopWord((ushort)(RidleyEscapeMode7LeftWall + 10 * (index / 4)), index % 4);
    }

    private static CeresDoorInstructionMechanicsWord WallLoopWord(ushort start, int index) => index switch
    {
        0 => new(start, CeresEnemyCodePointers.MakeCeresDoorIntangible),
        1 => new((ushort)(start + 2), 1),
        2 => new((ushort)(start + 6), CommonEnemyInstructionCodes.Goto),
        _ => new((ushort)(start + 8), (ushort)(start + 2)),
    };

    private static CeresDoorInstructionMechanicsWord NormalDoorWord(int index, int facing)
    {
        int start = NormalFacingRight + facing * (NormalFacingLeft - NormalFacingRight);
        if (index is >= 12 and < 16)
            return new((ushort)(start + 28 + 4 * (index - 12)), 5);
        if (index is >= 22 and < 26)
            return new((ushort)(start + 58 + 4 * (index - 22)), 5);
        (int offset, ushort value) = index switch
        {
            0 => (0, CeresEnemyCodePointers.MakeCeresDoorIntangible),
            1 => (2, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
            2 => (4, (ushort)2),
            3 => (8, CeresEnemyCodePointers.CeresDoorGotoIfSamusIsDistant),
            4 => (10, (ushort)(start + ClosedFacingRight - NormalFacingRight)),
            5 => (12, (ushort)2),
            6 => (16, CeresEnemyCodePointers.CeresDoorGotoIfSamusIsDistant),
            7 => (18, (ushort)(start + CloseFacingRight - NormalFacingRight)),
            8 => (20, CommonEnemyInstructionCodes.Goto),
            9 => (22, (ushort)(start + OpenFacingRight - NormalFacingRight)),
            10 => (24, CeresEnemyCodePointers.MakeCeresDoorTangible),
            11 => (26, CeresEnemyCodePointers.ShowCeresDoor),
            16 => (44, CeresEnemyCodePointers.MakeCeresDoorTangible),
            17 => (46, CeresEnemyCodePointers.ShowCeresDoor),
            18 => (48, (ushort)2),
            19 => (52, CeresEnemyCodePointers.CeresDoorGotoIfSamusIsDistant),
            20 => (54, (ushort)(start + ClosedFacingRightWait - NormalFacingRight)),
            21 => (56, EnemyInstructionCodePointers.Instruction_CeresDoor_QueueOpeningSFX),
            26 => (74, CeresEnemyCodePointers.MakeCeresDoorIntangible),
            27 => (76, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
            28 => (78, CommonEnemyInstructionCodes.Goto),
            _ => (80, (ushort)(start + OpenFacingRight - NormalFacingRight)),
        };
        return new((ushort)(start + offset), value);
    }

    private static CeresDoorInstructionMechanicsWord RidleyWord(int index)
    {
        if (index is >= 5 and < 9)
            return new((ushort)(RidleyRoomFacingRight + 12 + 4 * (index - 5)), 2);
        (int offset, ushort value) = index switch
        {
            0 => (0, CeresEnemyCodePointers.MakeCeresDoorIntangible),
            1 => (2, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
            2 => (4, (ushort)2),
            3 => (8, CeresEnemyCodePointers.MakeCeresDoorTangible),
            4 => (10, CeresEnemyCodePointers.ShowCeresDoor),
            9 => (28, EnemyInstructionCodePointers.Instruction_CeresDoor_SetDrawnByRidleyFlag),
            10 => (30, (ushort)1),
            11 => (34, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
            12 => (36, (ushort)2),
            13 => (40, EnemyInstructionCodePointers.Instruction_CeresDoor_GotoYIfAreaBossIsAlive),
            14 => (42, RidleyRoomFacingRightWait),
            15 => (44, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsVisible_ClearDrawnByRidleyFlag),
            16 => (46, CommonEnemyInstructionCodes.Goto),
            _ => (48, ClosedFacingRight),
        };
        return new((ushort)(RidleyRoomFacingRight + offset), value);
    }
    /// <summary>
    /// Stock spritemap selectors across the seven Ceres door variants.
    /// The Ridley-room right-door program uses $F540, $F548, $F54C,
    /// $F550, $F554, $F55A, and $F560, storing respectively $FAA7,
    /// $FAA7, $FA87, $FA67, $FA3D, $FA13, and $FA13 in the pinned ROM.
    /// Its repeated endpoints and mixed $20/$2A pose gaps preserve authored
    /// presentation rather than a uniform pointer progression.
    /// The ordinary right-door program uses $FA13 for its initial and
    /// closed holds and $FAA7 for its open hold. Its four close-transition
    /// operands at $F58A + 4*i are {$FAA7, $FA87, $FA67, $FA3D}; its four
    /// open-transition operands at $F5A8 + 4*i are exactly that sequence
    /// reversed, for i = 0..3.
    /// The left-door close-transition operands at $F5DC + 4*i are
    /// {$F9F3, $F9D3, $F9B3, $F989}; the open-transition operands at
    /// $F5FA + 4*i reverse them. Each of these eight pointers is the
    /// corresponding right-door transition pointer minus $00B4. The
    /// left-door holds are authored separately: initial $FA13 and
    /// open/closed $F95F, outside that offset rule.
    /// The four single-frame control-actor variants have one pointer
    /// each: variant 2 at $F614 stores $F921, variant 4 at $F624 stores
    /// $F95F, variant 5 at $F62E stores $FACE, and variant 6 at $F638
    /// stores $FB2F. The $F95F pose is shared with the left-door holds;
    /// the remaining visual identities are authored per variant.
    /// </summary>
    private static readonly (ushort Address, ushort Frame)[] PresentationWords =
    [
        (0xf540, 0xfaa7), (0xf548, 0xfaa7), (0xf54c, 0xfa87),
        (0xf550, 0xfa67), (0xf554, 0xfa3d), (0xf55a, 0xfa13),
        (0xf560, 0xfa13), (0xf572, 0xfa13), (0xf57a, 0xfaa7),
        (0xf58a, 0xfaa7), (0xf58e, 0xfa87), (0xf592, 0xfa67),
        (0xf596, 0xfa3d), (0xf59e, 0xfa13), (0xf5a8, 0xfa3d),
        (0xf5ac, 0xfa67), (0xf5b0, 0xfa87), (0xf5b4, 0xfaa7),
        (0xf5c4, 0xfa13), (0xf5cc, 0xf95f), (0xf5dc, 0xf9f3),
        (0xf5e0, 0xf9d3), (0xf5e4, 0xf9b3), (0xf5e8, 0xf989),
        (0xf5f0, 0xf95f), (0xf5fa, 0xf989), (0xf5fe, 0xf9b3),
        (0xf602, 0xf9d3), (0xf606, 0xf9f3), (0xf614, 0xf921),
        (0xf624, 0xf95f), (0xf62e, 0xface), (0xf638, 0xfb2f),
    ];

    internal static int MechanicsWordCount => 97;
    internal static int PresentationWordCount => PresentationWords.Length;

    internal static ushort PresentationWordAddress(int index) => PresentationWords[index].Address;
    internal static ushort PresentationWordFrame(int index) => PresentationWords[index].Frame;

    /// <summary>Returns the authored spritemap selector at a bank-$A6 operand.</summary>
    internal static ushort ReadPresentationFrame(ushort address)
    {
        int low = 0;
        int high = PresentationWords.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            var candidate = PresentationWords[middle];
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
            CeresDoorInstructionMechanicsWord candidate = MechanicsWord(middle);
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

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
