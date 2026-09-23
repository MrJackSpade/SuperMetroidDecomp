namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A6 address.</summary>
internal readonly record struct CeresDoorInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for all seven Ceres door/control-actor variants.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class CeresDoorInstructionProgramDefinitions
{
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
    /// <summary><c>InstList_CeresDoor_RidleyEscapeMode7LeftWall_0</c> at $A6:F62A.</summary>
    internal const ushort RidleyEscapeMode7LeftWall = 0xf62a;
    /// <summary><c>InstList_CeresDoor_RidleyEscapeMode7LeftWall_1</c> at $A6:F62C.</summary>
    internal const ushort RidleyEscapeMode7LeftWallLoop = 0xf62c;
    /// <summary><c>InstList_CeresDoor_RidleyEscapeMode7RightWall_0</c> at $A6:F634.</summary>
    internal const ushort RidleyEscapeMode7RightWall = 0xf634;
    /// <summary><c>InstList_CeresDoor_RidleyEscapeMode7RightWall_1</c> at $A6:F636.</summary>
    internal const ushort RidleyEscapeMode7RightWallLoop = 0xf636;

    private static readonly CeresDoorInstructionMechanicsWord[] Words =
    [
        new(0xf53a, CeresEnemyCodePointers.MakeCeresDoorIntangible),
        new(0xf53c, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
        new(0xf53e, 2),
        new(0xf542, CeresEnemyCodePointers.MakeCeresDoorTangible),
        new(0xf544, CeresEnemyCodePointers.ShowCeresDoor),
        new(0xf546, 2), new(0xf54a, 2), new(0xf54e, 2), new(0xf552, 2),
        new(0xf556, EnemyInstructionCodePointers.Instruction_CeresDoor_SetDrawnByRidleyFlag),
        new(0xf558, 1),
        new(0xf55c, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
        new(0xf55e, 2),
        new(0xf562, EnemyInstructionCodePointers.Instruction_CeresDoor_GotoYIfAreaBossIsAlive),
        new(0xf564, RidleyRoomFacingRightWait),
        new(0xf566, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsVisible_ClearDrawnByRidleyFlag),
        new(0xf568, CommonEnemyInstructionCodes.Goto),
        new(0xf56a, ClosedFacingRight),

        new(0xf56c, CeresEnemyCodePointers.MakeCeresDoorIntangible),
        new(0xf56e, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
        new(0xf570, 2),
        new(0xf574, CeresEnemyCodePointers.CeresDoorGotoIfSamusIsDistant),
        new(0xf576, ClosedFacingRight),
        new(0xf578, 2),
        new(0xf57c, CeresEnemyCodePointers.CeresDoorGotoIfSamusIsDistant),
        new(0xf57e, CloseFacingRight),
        new(0xf580, CommonEnemyInstructionCodes.Goto),
        new(0xf582, OpenFacingRight),
        new(0xf584, CeresEnemyCodePointers.MakeCeresDoorTangible),
        new(0xf586, CeresEnemyCodePointers.ShowCeresDoor),
        new(0xf588, 5), new(0xf58c, 5), new(0xf590, 5), new(0xf594, 5),
        new(0xf598, CeresEnemyCodePointers.MakeCeresDoorTangible),
        new(0xf59a, CeresEnemyCodePointers.ShowCeresDoor),
        new(0xf59c, 2),
        new(0xf5a0, CeresEnemyCodePointers.CeresDoorGotoIfSamusIsDistant),
        new(0xf5a2, ClosedFacingRightWait),
        new(0xf5a4, EnemyInstructionCodePointers.Instruction_CeresDoor_QueueOpeningSFX),
        new(0xf5a6, 5), new(0xf5aa, 5), new(0xf5ae, 5), new(0xf5b2, 5),
        new(0xf5b6, CeresEnemyCodePointers.MakeCeresDoorIntangible),
        new(0xf5b8, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
        new(0xf5ba, CommonEnemyInstructionCodes.Goto),
        new(0xf5bc, OpenFacingRight),

        new(0xf5be, CeresEnemyCodePointers.MakeCeresDoorIntangible),
        new(0xf5c0, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
        new(0xf5c2, 2),
        new(0xf5c6, CeresEnemyCodePointers.CeresDoorGotoIfSamusIsDistant),
        new(0xf5c8, ClosedFacingLeft),
        new(0xf5ca, 2),
        new(0xf5ce, CeresEnemyCodePointers.CeresDoorGotoIfSamusIsDistant),
        new(0xf5d0, CloseFacingLeft),
        new(0xf5d2, CommonEnemyInstructionCodes.Goto),
        new(0xf5d4, OpenFacingLeft),
        new(0xf5d6, CeresEnemyCodePointers.MakeCeresDoorTangible),
        new(0xf5d8, CeresEnemyCodePointers.ShowCeresDoor),
        new(0xf5da, 5), new(0xf5de, 5), new(0xf5e2, 5), new(0xf5e6, 5),
        new(0xf5ea, CeresEnemyCodePointers.MakeCeresDoorTangible),
        new(0xf5ec, CeresEnemyCodePointers.ShowCeresDoor),
        new(0xf5ee, 2),
        new(0xf5f2, CeresEnemyCodePointers.CeresDoorGotoIfSamusIsDistant),
        new(0xf5f4, ClosedFacingLeftWait),
        new(0xf5f6, EnemyInstructionCodePointers.Instruction_CeresDoor_QueueOpeningSFX),
        new(0xf5f8, 5), new(0xf5fc, 5), new(0xf600, 5), new(0xf604, 5),
        new(0xf608, CeresEnemyCodePointers.MakeCeresDoorIntangible),
        new(0xf60a, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
        new(0xf60c, CommonEnemyInstructionCodes.Goto),
        new(0xf60e, OpenFacingLeft),

        new(0xf610, CeresEnemyCodePointers.MakeCeresDoorIntangible),
        new(0xf612, 1),
        new(0xf616, CommonEnemyInstructionCodes.Goto),
        new(0xf618, RotatingElevatorPreExplosionOverlayLoop),
        new(0xf61a, EnemyInstructionCodePointers.Instruction_CeresDoor_GotoYIfCeresRidleyHasNotEscaped),
        new(0xf61c, NormalFacingLeft),
        new(0xf61e, CeresEnemyCodePointers.MakeCeresDoorTangible),
        new(0xf620, EnemyInstructionCodePointers.Instruction_CeresDoor_SetAsInvisible),
        new(0xf622, 1),
        new(0xf626, CommonEnemyInstructionCodes.Goto),
        new(0xf628, RotatingElevatorInvisibleWallLoop),
        new(0xf62a, CeresEnemyCodePointers.MakeCeresDoorIntangible),
        new(0xf62c, 1),
        new(0xf630, CommonEnemyInstructionCodes.Goto),
        new(0xf632, RidleyEscapeMode7LeftWallLoop),
        new(0xf634, CeresEnemyCodePointers.MakeCeresDoorIntangible),
        new(0xf636, 1),
        new(0xf63a, CommonEnemyInstructionCodes.Goto),
        new(0xf63c, RidleyEscapeMode7RightWallLoop),
    ];

    /// <summary>
    /// Live spritemap operand addresses across the seven Ceres door variants.
    /// The Ridley-room right-door program uses $F540, $F548, $F54C,
    /// $F550, $F554, $F55A, and $F560, storing respectively $FAA7,
    /// $FAA7, $FA87, $FA67, $FA3D, $FA13, and $FA13 in the pinned ROM.
    /// Its repeated endpoints and mixed $20/$2A pose gaps preserve authored
    /// presentation rather than a uniform pointer progression.
    /// The ordinary right-door program uses $FA13 for its initial and
    /// closed holds and $FAA7 for its open hold. Its four close-transition
    /// operands at $F58A + 4*i are {$FAA7, $FA87, $FA67, $FA3D}; its four
    /// open-transition operands at $F5A8 + 4*i are exactly that sequence
    /// reversed, for i = 0..3. Both transitions keep live ROM pointers.
    /// The left-door close-transition operands at $F5DC + 4*i are
    /// {$F9F3, $F9D3, $F9B3, $F989}; the open-transition operands at
    /// $F5FA + 4*i reverse them. Each of these eight pointers is the
    /// corresponding right-door transition pointer minus $00B4. The
    /// left-door holds are authored separately: initial $FA13 and
    /// open/closed $F95F, outside that offset rule.
    /// </summary>
    private static readonly ushort[] PresentationWords =
    [
        0xf540, 0xf548, 0xf54c, 0xf550, 0xf554, 0xf55a, 0xf560,
        0xf572, 0xf57a, 0xf58a, 0xf58e, 0xf592, 0xf596, 0xf59e,
        0xf5a8, 0xf5ac, 0xf5b0, 0xf5b4,
        0xf5c4, 0xf5cc, 0xf5dc, 0xf5e0, 0xf5e4, 0xf5e8, 0xf5f0,
        0xf5fa, 0xf5fe, 0xf602, 0xf606,
        0xf614, 0xf624, 0xf62e, 0xf638,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static CeresDoorInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Ceres door control or rejects pointers outside the authored programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CeresDoorInstructionMechanicsWord candidate = Words[middle];
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
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
