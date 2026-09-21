namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$B3 instruction entry points used by escape-animal bytecode.</summary>
internal static class EscapeAnimalInstructionCodes
{
    /// <summary>Installs the following bank-$B3 pre-instruction pointer at $B3:806B.</summary>
    public const ushort Instruction_CommonB3_Enemy0FB2_InY = 0x806b;

    /// <summary>Clears the pre-instruction pointer to the bank-common RTS at $B3:8074.</summary>
    public const ushort Instruction_CommonB3_SetEnemy0FB2ToRTS = 0x8074;

    /// <summary><c>Instruction_EtecoonEscape_GotoY_IfAcidPositionLessThanCE</c> at $B3:E545.</summary>
    public const ushort Instruction_EtecoonEscape_GotoY_IfAcidPositionLessThanCE = 0xe545;

    /// <summary><c>Instruction_EtecoonEscape_XPositionPlusY</c> at $B3:E610.</summary>
    public const ushort Instruction_EtecoonEscape_XPositionPlusY = 0xe610;

    /// <summary><c>InstList_DachoraEscape_GotoY_IfAcidLessThanCE</c> at $B3:EAA8.</summary>
    public const ushort InstList_DachoraEscape_GotoY_IfAcidLessThanCE = 0xeaa8;

    /// <summary><c>InstList_DachoraEscape_GotoY_IfCrittersEscaped</c> at $B3:EAB8.</summary>
    public const ushort InstList_DachoraEscape_GotoY_IfCrittersEscaped = 0xeab8;

    /// <summary><c>Instruction_DachoraEscape_XPositionMinus6</c> at $B3:EAC9.</summary>
    public const ushort Instruction_DachoraEscape_XPositionMinus6 = 0xeac9;

    /// <summary><c>Instruction_DachoraEscape_XPositionPlus6</c> at $B3:EAD7.</summary>
    public const ushort Instruction_DachoraEscape_XPositionPlus6 = 0xead7;
}
