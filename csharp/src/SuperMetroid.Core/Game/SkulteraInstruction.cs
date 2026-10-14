namespace SuperMetroid.Core.Game;

/// <summary>Skultera's private bank-$A3 animation instructions.</summary>
internal enum SkulteraInstruction : ushort
{
    /// <summary><c>Instruction_Skultera_SetLayerTo6</c> at $A3:9096.</summary>
    SetLayerTo6 = 0x9096,

    /// <summary><c>Instruction_Skultera_SetLayerTo2</c> at $A3:90A0.</summary>
    SetLayerTo2 = 0x90a0,

    /// <summary><c>Instruction_Skultera_SetTurnFinishedFlag</c> at $A3:90AA.</summary>
    SetTurnFinishedFlag = 0x90aa,
}
