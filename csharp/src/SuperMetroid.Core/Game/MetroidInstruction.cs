namespace SuperMetroid.Core.Game;

/// <summary>Metroid's private bank-$A3 animation instructions.</summary>
internal enum MetroidInstruction : ushort
{
    /// <summary><c>Instruction_Metroid_PlayDrainingSamusSFX</c> at $A3:EAA5.</summary>
    PlayDrainingSamusSFX = 0xeaa5,

    /// <summary><c>Instruction_Metroid_PlayRandomMetroidSFX</c> at $A3:EAB1.</summary>
    PlayRandomMetroidSFX = 0xeab1,
}
