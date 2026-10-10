namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A2 code pointers consumed by translated Rinka dispatchers.</summary>
internal enum RinkaInstruction : ushort
{
    /// <summary><c>UNUSED_Instruction_Rinka_GotoYIfCounterGreaterThan2_A2B9A2</c> at $A2:B9A2.</summary>
    UNUSED_Instruction_Rinka_GotoYIfCounterGreaterThan2_A2B9A2 = 0xb9a2,

    /// <summary><c>Instruction_Rinka_SetAsIntangibleAndInvisible</c> at $A2:B9B3.</summary>
    Instruction_Rinka_SetAsIntangibleAndInvisible = 0xb9b3,

    /// <summary><c>Instruction_Rinka_SetAsIntangibleInvisibleAndActiveOffScreen</c> at $A2:B9BD.</summary>
    Instruction_Rinka_SetAsIntangibleInvisibleAndActiveOffScreen = 0xb9bd,

    /// <summary><c>Instruction_Rinka_FireRinka</c> at $A2:B9C7.</summary>
    Instruction_Rinka_FireRinka = 0xb9c7,
}
