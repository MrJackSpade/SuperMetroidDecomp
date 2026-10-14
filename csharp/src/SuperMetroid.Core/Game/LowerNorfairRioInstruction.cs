namespace SuperMetroid.Core.Game;

/// <summary>Lower Norfair Rio (Holtz) private bank-$A2 instruction opcodes.</summary>
internal enum LowerNorfairRioInstruction : ushort
{
    /// <summary><c>Instruction_Holtz_SetAnimationFinishedFlag</c> at $A2:C6D2.</summary>
    SetAnimationFinishedFlag = 0xc6d2,

    /// <summary><c>Instruction_Holtz_HideFlames</c> at $A2:C6DD.</summary>
    HideFlames = 0xc6dd,

    /// <summary><c>Instruction_Holtz_ShowFlames</c> at $A2:C6E8.</summary>
    ShowFlames = 0xc6e8,
}
