namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A7 code pointers consumed by translated Kraid dispatchers.</summary>
internal static class KraidInstructionCodes
{
    /// <summary><c>Instruction_Kraid_NOP_A7B633</c> at $A7:B633.</summary>
    public const ushort Instruction_Kraid_NOP_A7B633 = 0xb633;

    /// <summary><c>Instruction_Kraid_DecrementYPosition</c> at $A7:B636.</summary>
    public const ushort Instruction_Kraid_DecrementYPosition = 0xb636;

    /// <summary><c>Instruction_Kraid_IncrementYPosition_SetScreenShaking</c> at $A7:B63C.</summary>
    public const ushort Instruction_Kraid_IncrementYPosition_SetScreenShaking = 0xb63c;

    /// <summary><c>Instruction_Kraid_QueueSFX76_Lib2_Max6</c> at $A7:B64E.</summary>
    public const ushort Instruction_Kraid_QueueSFX76_Lib2_Max6 = 0xb64e;

    /// <summary><c>Instruction_Kraid_XPositionMinus3</c> at $A7:B65A.</summary>
    public const ushort Instruction_Kraid_XPositionMinus3 = 0xb65a;

    /// <summary><c>Instruction_Kraid_XPositionMinus3_duplicate</c> at $A7:B667.</summary>
    public const ushort Instruction_Kraid_XPositionMinus3_duplicate = 0xb667;

    /// <summary><c>Instruction_Kraid_XPositionPlus3</c> at $A7:B674.</summary>
    public const ushort Instruction_Kraid_XPositionPlus3 = 0xb674;

    /// <summary><c>UNUSED_Instruction_Kraid_MoveRight_A7B683</c> at $A7:B683.</summary>
    public const ushort UNUSED_Instruction_Kraid_MoveRight_A7B683 = 0xb683;

}
