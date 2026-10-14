namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>The Fune/Namihe family's private bank-$A8 animation instructions.</summary>
internal enum FuneNamiheInstruction : ushort
{
    /// <summary><c>Instruction_FuneNamihe_QueueSpitSFX</c> at $A8:9625.</summary>
    QueueSpitSFX = 0x9625,

    /// <summary><c>Instruction_Namihe_SpawnFireball_FacingLeft</c> at $A8:9631.</summary>
    NamiheSpawnFireballFacingLeft = 0x9631,

    /// <summary><c>Instruction_Namihe_SpawnFireball_FacingRight</c> at $A8:964A.</summary>
    NamiheSpawnFireballFacingRight = 0x964a,

    /// <summary><c>Instruction_Fune_SpawnFireball_FacingLeft</c> at $A8:9663.</summary>
    FuneSpawnFireballFacingLeft = 0x9663,

    /// <summary><c>Instruction_Fune_SpawnFireball_FacingRight</c> at $A8:967C.</summary>
    FuneSpawnFireballFacingRight = 0x967c,

    /// <summary><c>Instruction_FuneNamihe_FinishActivity</c> at $A8:9695.</summary>
    FinishActivity = 0x9695,

    /// <summary><c>Instruction_FuneNamihe_FinishActivity_duplicate</c> at $A8:96B4.</summary>
    FinishActivityDuplicate = 0x96b4,
}
