namespace SuperMetroid.Core.Game;

/// <summary>Waver's private bank-$A3 animation instruction.</summary>
internal enum WaverInstruction : ushort
{
    /// <summary><c>Instruction_Waver_SetSpinFinishedFlag</c> at $A3:86E3.</summary>
    SetSpinFinishedFlag = 0x86e3,
}
