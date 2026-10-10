namespace SuperMetroid.Core.Game;

/// <summary>The bank-$A7 Kraid foot instructions that move or shake Kraid's shared body.</summary>
internal enum KraidFootInstruction : ushort
{
    /// <summary><c>Instruction_Kraid_NOP_A7B633</c> at $A7:B633.</summary>
    Instruction_Kraid_NOP_A7B633 = 0xb633,

    /// <summary><c>Instruction_Kraid_DecrementYPosition</c> at $A7:B636.</summary>
    Instruction_Kraid_DecrementYPosition = 0xb636,

    /// <summary><c>Instruction_Kraid_IncrementYPosition_SetScreenShaking</c> at $A7:B63C.</summary>
    Instruction_Kraid_IncrementYPosition_SetScreenShaking = 0xb63c,

    /// <summary><c>Instruction_Kraid_QueueSFX76_Lib2_Max6</c> at $A7:B64E.</summary>
    Instruction_Kraid_QueueSFX76_Lib2_Max6 = 0xb64e,

    /// <summary><c>Instruction_Kraid_XPositionMinus3</c> at $A7:B65A.</summary>
    Instruction_Kraid_XPositionMinus3 = 0xb65a,

    /// <summary><c>Instruction_Kraid_XPositionMinus3_duplicate</c> at $A7:B667.</summary>
    Instruction_Kraid_XPositionMinus3_duplicate = 0xb667,

    /// <summary><c>Instruction_Kraid_XPositionPlus3</c> at $A7:B674.</summary>
    Instruction_Kraid_XPositionPlus3 = 0xb674,

    /// <summary><c>UNUSED_Instruction_Kraid_MoveRight_A7B683</c> at $A7:B683.</summary>
    UNUSED_Instruction_Kraid_MoveRight_A7B683 = 0xb683,
}

/// <summary>Kraid's arm's private bank-$A7 animation instruction.</summary>
internal enum KraidArmInstruction : ushort
{
    /// <summary><c>Instruction_KraidArm_SlowArmIfLessThanHalfHealth</c> at $A7:8A8F.</summary>
    SlowArmIfLessThanHalfHealth = 0x8a8f,
}
