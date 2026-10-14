namespace SuperMetroid.Core.Game;

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
