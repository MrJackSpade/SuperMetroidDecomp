namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$B3 instruction entry points used by escape-animal bytecode.</summary>
internal static class EscapeAnimalInstructionCodes
{
    /// <summary>Installs the following bank-$B3 pre-instruction pointer at $B3:806B.</summary>
    public const ushort Instruction_CommonB3_Enemy0FB2_InY = 0x806b;

    /// <summary>Clears the pre-instruction pointer to the bank-common RTS at $B3:8074.</summary>
    public const ushort Instruction_CommonB3_SetEnemy0FB2ToRTS = 0x8074;
}
