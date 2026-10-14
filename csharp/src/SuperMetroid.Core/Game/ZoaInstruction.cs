namespace SuperMetroid.Core.Game;

/// <summary>Zoa's private bank-$A3 animation instructions.</summary>
internal enum ZoaInstruction : ushort
{
    /// <summary><c>Instruction_Zoa_SetXSpeedTableIndexTo4</c> at $A3:B429.</summary>
    SetXSpeedTableIndexTo4 = 0xb429,

    /// <summary><c>Instruction_Zoa_SetXSpeedTableIndexTo8</c> at $A3:B434.</summary>
    SetXSpeedTableIndexTo8 = 0xb434,

    /// <summary><c>Instruction_Zoa_SetXSpeedTableIndexToC</c> at $A3:B43F.</summary>
    SetXSpeedTableIndexToC = 0xb43f,
}
