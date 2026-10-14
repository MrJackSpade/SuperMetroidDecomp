namespace SuperMetroid.Core.Game;

/// <summary>Skree's private bank-$A3 animation instruction.</summary>
internal enum SkreeInstruction : ushort
{
    /// <summary><c>Instruction_Skree_SetAttackReadyFlag</c> at $A3:C6A4.</summary>
    SetAttackReadyFlag = 0xc6a4,
}
