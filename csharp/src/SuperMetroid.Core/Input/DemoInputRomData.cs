namespace SuperMetroid.Core.Input;

/// <summary>Verified bank-$91 demo-controller object and bytecode definitions.</summary>
public static class DemoInputRomData
{

    /// <summary>Bank-$91 return-only routine identities accepted by the demo object's initializer and pre-instruction dispatch.</summary>
    public static class Routines
    {
        /// <summary>$91:83BF, <c>RTS_9183BF</c>: performs no initialization or per-update pre-instruction work.</summary>
        public const ushort NoOp = 0x83bf;
        /// <summary>$91:8447, <c>Instruction_DemoInputObject_ClearPreInstruction.return</c>: the no-op RTS installed when the demo pre-instruction is cleared.</summary>
        public const ushort ClearedPreInstruction = 0x8447;
    }

    /// <summary>Title-demo-specific pre-instructions and their redirect destinations.</summary>
    public static class Attract
    {
        /// <summary>$91:8776, InstList_DemoInput_Delete: common demo object deletion list.</summary>
        public const ushort DeleteList = 0x8776;
        /// <summary>$91:9346, unused shinespark continuation targeted literally by $8AB0.</summary>
        public const ushort ShinesparkContinuation = 0x9346;
    }

    /// <summary>Bank-$91 demo-list control routines and the six-byte input-record layout consumed by the instruction interpreter.</summary>
    public static class Instructions
    {
        /// <summary>Leading-word bit $8000 tested at $91:8400-$8403: set selects an indirect bank-$91 routine address; clear selects an input-record duration.</summary>
        public const ushort OpcodeBit = 0x8000;
        /// <summary>$91:8427, <c>Instruction_DemoInputObject_Delete</c>: no operands; clears the instruction pointer and held/new input words, terminating list processing.</summary>
        public const ushort Delete = 0x8427;
        /// <summary>$91:8434, <c>Instruction_DemoInputObject_PreInstructionInY</c>: consumes one bank-$91 routine-pointer word and installs it as the object's per-update pre-instruction.</summary>
        public const ushort SetPreInstruction = 0x8434;
        /// <summary>$91:843F, <c>Instruction_DemoInputObject_ClearPreInstruction</c>: no operands; installs the return-only target <see cref="Routines.ClearedPreInstruction"/>.</summary>
        public const ushort ClearPreInstruction = 0x843f;
        /// <summary>$91:8448, <c>Instruction_DemoInputObject_GotoY</c>: replaces the list cursor with the following sixteen-bit bank-$91 byte pointer.</summary>
        public const ushort Goto = 0x8448;
        /// <summary>$91:844F, <c>Instruction_DemoInputObject_DecrementTimer_GotoYIfNonZero</c>: decrements the separate sixteen-bit loop timer and jumps through the following pointer unless the result is zero; zero skips that operand.</summary>
        public const ushort DecrementTimerAndGoto = 0x844f;
        /// <summary>$91:8459, <c>Instruction_DemoInputObject_TimerInY</c>: consumes one word to set the loop timer, independently of the input-record duration countdown.</summary>
        public const ushort SetTimer = 0x8459;
        /// <summary>Six bytes per input record: duration word, held-button word, and explicitly authored newly-pressed-button word; $91:8420 advances the cursor by this byte count.</summary>
        public const int InputRecordBytes = 6;
    }
}

/// <summary>The bank-$91 pre-instructions a title-demo input object can hold.</summary>
public enum AttractDemoPreInstruction : ushort
{
    /// <summary>$91:83BF, the return-only routine installed by the demo object definition.</summary>
    NoOp = DemoInputRomData.Routines.NoOp,
    /// <summary>$91:8447, the return-only routine installed when the pre-instruction is cleared.</summary>
    Cleared = DemoInputRomData.Routines.ClearedPreInstruction,
    /// <summary>$91:8A9B, DemoPreInstr_CheckLeaveDemo: delete during game state $2C.</summary>
    CheckLeave = 0x8a9b,
    /// <summary>$91:8AB0, DemoPreInstr_8AB0: redirects unless movement type is $1A.</summary>
    Shinespark = 0x8ab0,
}
