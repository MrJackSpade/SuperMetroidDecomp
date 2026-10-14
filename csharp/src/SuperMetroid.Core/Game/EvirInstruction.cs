namespace SuperMetroid.Core.Game;

/// <summary>The Evir projectile's private bank-$A8 animation instructions.</summary>
internal enum EvirInstruction : ushort
{
    /// <summary><c>Instruction_Evir_PlaySpitSFX</c> at $A8:878F.</summary>
    PlaySpitSFX = 0x878f,

    /// <summary><c>Instruction_Evir_SetInitialRegenerationXOffset</c> at $A8:879B.</summary>
    SetInitialRegenerationXOffset = 0x879b,

    /// <summary><c>Instruction_Evir_AdvanceRegenerationXOffset</c> at $A8:87B6.</summary>
    AdvanceRegenerationXOffset = 0x87b6,

    /// <summary><c>Instruction_Evir_FinishRegeneration</c> at $A8:87CB.</summary>
    FinishRegeneration = 0x87cb,
}
