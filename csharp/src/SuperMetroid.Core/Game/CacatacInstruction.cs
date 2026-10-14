namespace SuperMetroid.Core.Game;

/// <summary>Cacatac's private bank-$A2 animation instructions.</summary>
internal enum CacatacInstruction : ushort
{
    /// <summary><c>Instruction_Cacatac_PlaySpikesSFX</c> at $A2:9F2A.</summary>
    PlaySpikesSFX = 0x9f2a,

    /// <summary><c>Instruction_Cacatac_SetFunction_MovingLeftRight</c> at $A2:A095.</summary>
    SetFunctionMovingLeftRight = 0xa095,

    /// <summary><c>Instruction_Cacatac_SpawnSpikeProjectileWithParameterInY</c> at $A2:A0A7.</summary>
    SpawnSpikeProjectileWithParameterInY = 0xa0a7,
}
