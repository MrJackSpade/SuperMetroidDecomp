namespace SuperMetroid.Core.Game;

/// <summary>Stoke's private bank-$A2 animation instructions.</summary>
internal enum StokeInstruction : ushort
{
    /// <summary><c>Instruction_Stoke_SpawnFireball</c> at $A2:897E.</summary>
    SpawnFireball = 0x897e,

    /// <summary><c>Instruction_Stoke_SetMovingLeft</c> at $A2:8990.</summary>
    SetMovingLeft = 0x8990,

    /// <summary><c>Instruction_Stoke_SetMovingRight</c> at $A2:899D.</summary>
    SetMovingRight = 0x899d,
}
