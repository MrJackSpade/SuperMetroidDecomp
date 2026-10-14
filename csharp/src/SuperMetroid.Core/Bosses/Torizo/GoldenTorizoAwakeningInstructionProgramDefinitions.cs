using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

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
    /// <summary>One-update falling pose before the ground-contact branch retries.</summary>
    private const ushort FallingHold = 1;
    /// <summary>First sitting pose hold; later poses advance this duration by pose index.</summary>
    private const ushort SittingInitialHold = 3;
    /// <summary>Seated pause before the initial VRAM upload sequence.</summary>
    private const ushort SeatedHold = 48;
    /// <summary>Base hold for the first three upload poses; successive holds shift this value down.</summary>
    private const ushort UploadInitialHold = 32;
    /// <summary>Hold for each pose in the repeated four-upload cycle.</summary>
    private const ushort UploadLoopHold = 4;
    /// <summary>Native timer repeats the four-upload pose cycle twice without deriving a simulation quantity.</summary>
    private const ushort UploadLoopCount = 2;
    /// <summary>Long initial hold while the statue begins standing.</summary>
    private const ushort StandFirstHold = 32;
    /// <summary>Second standing pose hold before the remaining short poses.</summary>
    private const ushort StandSecondHold = 12;
    /// <summary>Hold shared by the last four standing movement poses.</summary>
    private const ushort StandRemainingHold = 8;
    /// <summary>Duration of each gradual color-change pose.</summary>
    private const ushort ColorHold = 4;
    /// <summary>Native timer counts sixteen color-change iterations before clearing the animation lock.</summary>
    private const ushort ColorIterations = 16;
    /// <summary>Final awakened pose hold before branching into the walking-left combat list.</summary>
    private const ushort HandoffHold = 16;
    /// <summary>Visual operands interleaved through the awakening sequence, separate from instruction control words.</summary>
    public static int PresentationWordCount => 21;
    /// <summary>Resolves a visual operand's bank-$AA address while leaving its editable frame selector outside mechanics storage.</summary>
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
        layout.Word((ushort)TorizoInstruction.Instruction_Torizo_GotoY_IfNotHitGround);
        layout.Word(falling);
        layout.Word((ushort)TorizoInstruction.Instruction_Torizo_PlayTorizoFootstepsSFX);
        for (int pose = 0; pose < 3; pose++)
        {
            layout.Pose((ushort)(SittingInitialHold + pose));
            layout.Word((ushort)TorizoInstruction.Instruction_Torizo_SittingDownMovement_IndexInY);
            layout.Word((ushort)(sizeof(ushort) * (2 - pose)));
        }
        layout.Function(IdleFunction);
        layout.Pose(SeatedHold);
        for (int upload = 0; upload < 4; upload++)
        {
            layout.Upload();
            layout.Pose((ushort)(upload < 3 ? UploadInitialHold >> upload : UploadInitialHold));
        }
        layout.Word((ushort)CommonEnemyInstruction.SetTimer); layout.Word(UploadLoopCount);
        ushort uploadLoop = layout.Cursor;
        for (int upload = 0; upload < 4; upload++) { layout.Pose(UploadLoopHold); layout.Upload(); }
        layout.Word((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate); layout.Word(uploadLoop);
        for (int pose = 0; pose < 6; pose++)
        {
            layout.Pose(pose == 0 ? StandFirstHold : pose == 1 ? StandSecondHold : StandRemainingHold);
            layout.Word((ushort)TorizoInstruction.Instruction_Torizo_StandingUpMovement_IndexInY);
            layout.Word((ushort)(sizeof(ushort) * pose));
        }
        layout.Word((ushort)TorizoInstruction.Instruction_Torizo_LoadGoldenTorizoPalettes);
        layout.Word((ushort)CommonEnemyInstruction.SetTimer); layout.Word(ColorIterations);
        ushort colorLoop = layout.Cursor;
        layout.Pose(ColorHold);
        layout.Word((ushort)TorizoInstruction.Instruction_Torizo_AdvanceGradualColorChange);
        layout.Word((ushort)CommonEnemyInstruction.DecrementTimerAndGotoDuplicate); layout.Word(colorLoop);
        layout.Word((ushort)TorizoInstruction.RTL_AAC2C8);
        layout.Word((ushort)TorizoInstruction.Instruction_Torizo_ClearAnimationLock);
        layout.Word((ushort)TorizoInstruction.Inst_Torizo_StartFightMusic_GoldenTorizoBellyPaletteFX);
        layout.Pose(HandoffHold);
        layout.Word((ushort)CommonEnemyInstruction.Goto);
        layout.Word(GoldenTorizoCombatInstructionPointers.WalkingLeftLeftLeg);
        return layout.Result;
    }

    /// <summary>Walks the variable-width native instruction stream while selecting one mechanics or presentation ordinal.</summary>
    private ref struct Layout(int requested, bool visual)
    {
        /// <summary>Current native instruction address as variable-width words and upload descriptors are laid out.</summary>
        internal ushort Cursor { get; private set; } = Start;
        /// <summary>Count of emitted control words before the requested mechanics ordinal.</summary>
        private int mechanics;
        /// <summary>Count of encountered visual operands before the requested presentation ordinal.</summary>
        private int presentation;
        /// <summary>Selected native address/value pair; visual selections carry a placeholder value.</summary>
        internal InstructionMechanicsWord Result { get; private set; }
        /// <summary>Advances a two-byte control word, capturing it when its mechanics ordinal is requested.</summary>
        internal void Word(ushort value)
        {
            if (!visual && mechanics == requested) Result = new(Cursor, value);
            mechanics++; Cursor += sizeof(ushort);
        }
        /// <summary>Places a timed pose's duration and separately records its following visual operand address.</summary>
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (visual && presentation == requested) Result = new(Cursor, 0);
            presentation++; Cursor += sizeof(ushort);
        }
        /// <summary>Emits the Torizo function-change opcode and its selected native routine pointer.</summary>
        internal void Function(ushort function)
        {
            Word((ushort)TorizoInstruction.Instruction_Torizo_FunctionInY); Word(function);
        }
        /// <summary>Emits CopyToVram and skips its separately owned seven-byte DMA descriptor.</summary>
        internal void Upload()
        {
            Word((ushort)CommonEnemyInstruction.CopyToVram);
            Cursor += sizeof(ushort) + 3 + sizeof(ushort);
        }
    }
}
