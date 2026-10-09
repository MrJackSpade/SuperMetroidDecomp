namespace SuperMetroid.Core.Game;

/// <summary>
/// The bounded continuation of <c>InstList_GoldenTorizo_Initial_0</c> at
/// $AA:C9E2-CACD: fall, sitting-down tile swaps, standing-up animation,
/// palette change, and the handoff to the ordinary walking program. The
/// interleaved extended-spritemap selectors are presentation data; the eight
/// $814B upload descriptors and their pixels have separate installed owners.
/// </summary>
internal abstract class GoldenTorizoAwakeningInstructionProgramDefinitions
{
    /// <summary>First instruction after the Samus-position sleep at $AA:C9E2.</summary>
    internal const ushort Start = 0xc9e2;

    /// <summary>$AA:C6BF, Function_Torizo_SimpleMovement, active during the initial fall.</summary>
    private const ushort FallingFunction = 0xc6bf;
    /// <summary>$AA:C6AB, RTS_AAC6AB, disables movement during the seated upload sequence.</summary>
    private const ushort IdleFunction = 0xc6ab;
    /// <summary>Awakening holds and the upload loop count. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it. The loop count only repeats the authored upload pose.</summary>
    private const ushort FallingHold = 1, SittingInitialHold = 3, SeatedHold = 48,
        UploadInitialHold = 32, UploadLoopHold = 4, UploadLoopCount = 2,
        StandFirstHold = 32, StandSecondHold = 12, StandRemainingHold = 8,
        ColorHold = 4, ColorIterations = 16, HandoffHold = 16;
    /// <summary>Number of interleaved presentation-selector words in the awakening program.</summary>
    public static int PresentationWordCount => 21;

    /// <summary>Returns the native address of one presentation-selector word in the awakening program.</summary>
    /// <param name="index">Zero-based presentation-word index.</param>
    /// <returns>The bank-local address of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside <see cref="PresentationWordCount"/>.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return Select(index, visual: true).Address;
    }

    /// <summary>
    /// $AA:C9E2-CACD: native timed poses occupy four bytes, function/branch operands
    /// two, and each DMA opcode plus its separately owned descriptor occupies nine.
    /// Sitting movement traverses word offsets4,2,0; standing traverses0..10.
    /// </summary>
    internal static InstructionMechanicsWord Select(int index, bool visual)
    {
        var layout = new Layout(index, visual);
        layout.Function(FallingFunction);
        ushort falling = layout.Cursor;
        layout.Pose(FallingHold);
        layout.Word(TorizoInstructionCodes.Instruction_Torizo_GotoY_IfNotHitGround);
        layout.Word(falling);
        layout.Word(TorizoInstructionCodes.Instruction_Torizo_PlayTorizoFootstepsSFX);
        for (int pose = 0; pose < 3; pose++)
        {
            layout.Pose((ushort)(SittingInitialHold + pose));
            layout.Word(TorizoInstructionCodes.Instruction_Torizo_SittingDownMovement_IndexInY);
            layout.Word((ushort)(sizeof(ushort) * (2 - pose)));
        }
        layout.Function(IdleFunction);
        layout.Pose(SeatedHold);
        for (int upload = 0; upload < 4; upload++)
        {
            layout.Upload();
            layout.Pose((ushort)(upload < 3 ? UploadInitialHold >> upload : UploadInitialHold));
        }
        layout.Word(CommonEnemyInstructionCodes.SetTimer); layout.Word(UploadLoopCount);
        ushort uploadLoop = layout.Cursor;
        for (int upload = 0; upload < 4; upload++) { layout.Pose(UploadLoopHold); layout.Upload(); }
        layout.Word(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate); layout.Word(uploadLoop);
        for (int pose = 0; pose < 6; pose++)
        {
            layout.Pose(pose == 0 ? StandFirstHold : pose == 1 ? StandSecondHold : StandRemainingHold);
            layout.Word(TorizoInstructionCodes.Instruction_Torizo_StandingUpMovement_IndexInY);
            layout.Word((ushort)(sizeof(ushort) * pose));
        }
        layout.Word(TorizoInstructionCodes.Instruction_Torizo_LoadGoldenTorizoPalettes);
        layout.Word(CommonEnemyInstructionCodes.SetTimer); layout.Word(ColorIterations);
        ushort colorLoop = layout.Cursor;
        layout.Pose(ColorHold);
        layout.Word(TorizoInstructionCodes.Instruction_Torizo_AdvanceGradualColorChange);
        layout.Word(CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate); layout.Word(colorLoop);
        layout.Word(TorizoInstructionCodes.RTL_AAC2C8);
        layout.Word(TorizoInstructionCodes.Instruction_Torizo_ClearAnimationLock);
        layout.Word(TorizoInstructionCodes.Inst_Torizo_StartFightMusic_GoldenTorizoBellyPaletteFX);
        layout.Pose(HandoffHold);
        layout.Word(CommonEnemyInstructionCodes.Goto);
        layout.Word(GoldenTorizoCombatInstructionPointers.WalkingLeftLeftLeg);
        return layout.Result;
    }

    /// <summary>Walks the awakening instruction stream while locating a requested mechanics or presentation word.</summary>
    /// <param name="requested">The zero-based word ordinal selected for this layout pass.</param>
    /// <param name="visual">Whether the pass selects interleaved presentation operands instead of mechanics words.</param>
    private ref struct Layout(int requested, bool visual)
    {
        /// <summary>Gets the bank-local address reached after the instruction words and separately owned upload descriptors traversed so far.</summary>
        internal ushort Cursor { get; private set; } = Start;

        /// <summary>Tracks mechanics-word and presentation-operand ordinals while walking the instruction stream.</summary>
        private int mechanics, presentation;

        /// <summary>Gets the selected instruction address and value produced during this layout pass.</summary>
        internal InstructionMechanicsWord Result { get; private set; }

        /// <summary>Accounts for one instruction word and records it when it matches the requested mechanics ordinal.</summary>
        /// <param name="value">The native opcode, operand, timer, or branch word at the current cursor.</param>
        internal void Word(ushort value)
        {
            if (!visual && mechanics == requested) Result = new(Cursor, value);
            mechanics++; Cursor += sizeof(ushort);
        }
        /// <summary>Accounts for a timed pose and exposes the following selector address to presentation-selection passes.</summary>
        /// <param name="duration">The authored number of frames for the pose.</param>
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (visual && presentation == requested) Result = new(Cursor, 0);
            presentation++; Cursor += sizeof(ushort);
        }
        /// <summary>Emits the native function-in-Y opcode and its function pointer operand.</summary>
        /// <param name="function">The bank-local function address installed by the instruction.</param>
        internal void Function(ushort function)
        {
            Word(TorizoInstructionCodes.Instruction_Torizo_FunctionInY); Word(function);
        }
        /// <summary>Emits the VRAM-copy opcode and advances over its separately cataloged upload descriptor.</summary>
        internal void Upload()
        {
            Word(CommonEnemyInstructionCodes.CopyToVram);
            Cursor += sizeof(ushort) + 3 + sizeof(ushort);
        }
    }
}
