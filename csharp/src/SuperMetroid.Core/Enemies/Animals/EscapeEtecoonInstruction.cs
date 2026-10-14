namespace SuperMetroid.Core.Enemies.Animals;

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
