using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Input;

/// <summary>The six bank-$91 control routines shared by every demo-input instruction list.</summary>
internal enum DemoInputInstruction : ushort
{
    /// <summary>$91:8427, <c>Instruction_DemoInputObject_Delete</c>: no operands; clears the instruction pointer and held/new input words, terminating list processing.</summary>
    Delete = 0x8427,
    /// <summary>$91:8434, <c>Instruction_DemoInputObject_PreInstructionInY</c>: consumes one bank-$91 routine-pointer word and installs it as the object's per-update pre-instruction.</summary>
    SetPreInstruction = 0x8434,
    /// <summary>$91:843F, <c>Instruction_DemoInputObject_ClearPreInstruction</c>: no operands; installs the return-only target <see cref="DemoInputRomData.Routines.ClearedPreInstruction"/>.</summary>
    ClearPreInstruction = 0x843f,
    /// <summary>$91:8448, <c>Instruction_DemoInputObject_GotoY</c>: replaces the list cursor with the following sixteen-bit bank-$91 byte pointer.</summary>
    Goto = 0x8448,
    /// <summary>$91:844F, <c>Instruction_DemoInputObject_DecrementTimer_GotoYIfNonZero</c>: decrements the separate sixteen-bit loop timer and jumps through the following pointer unless the result is zero; zero skips that operand.</summary>
    DecrementTimerAndGoto = 0x844f,
    /// <summary>$91:8459, <c>Instruction_DemoInputObject_TimerInY</c>: consumes one word to set the loop timer, independently of the input-record duration countdown.</summary>
    SetTimer = 0x8459,
}
