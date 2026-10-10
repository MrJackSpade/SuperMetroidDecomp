namespace SuperMetroid.Core.Game;

/// <summary>Family-specific bank-$B3 instructions referenced by the escape Etecoon lists.</summary>
internal enum EscapeEtecoonInstruction : ushort
{
    /// <summary>Installs the following bank-$B3 pre-instruction pointer at $B3:806B.</summary>
    SetPreInstruction = 0x806b,
    /// <summary>Clears the pre-instruction pointer to the bank-common RTS at $B3:8074.</summary>
    ClearPreInstruction = 0x8074,
    /// <summary><c>Instruction_EtecoonEscape_GotoY_IfAcidPositionLessThanCE</c> at $B3:E545.</summary>
    GotoIfAcidYLessThanCE = 0xe545,
    /// <summary><c>Instruction_EtecoonEscape_XPositionPlusY</c> at $B3:E610.</summary>
    AddXPosition = 0xe610,
}

/// <summary>Family-specific bank-$B3 instructions referenced by the escape Dachora lists.</summary>
internal enum EscapeDachoraInstruction : ushort
{
    /// <summary><c>InstList_DachoraEscape_GotoY_IfAcidLessThanCE</c> at $B3:EAA8.</summary>
    GotoIfAcidYLessThanCE = 0xeaa8,
    /// <summary><c>InstList_DachoraEscape_GotoY_IfCrittersEscaped</c> at $B3:EAB8.</summary>
    GotoIfCrittersEscaped = 0xeab8,
    /// <summary><c>Instruction_DachoraEscape_XPositionMinus6</c> at $B3:EAC9.</summary>
    MoveLeft = 0xeac9,
    /// <summary><c>Instruction_DachoraEscape_XPositionPlus6</c> at $B3:EAD7.</summary>
    MoveRight = 0xead7,
}
