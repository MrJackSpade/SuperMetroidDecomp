namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>What a cue of Spore Spawn's body programs does: one per bank-$A5 Spore Spawn instruction its lists use.</summary>
internal enum SporeSpawnCueKind
{
    /// <summary>Selects the main-AI function (<c>Instruction_SporeSpawn_FunctionInY</c>, $A5:E8BA).</summary>
    SetFunction,
    /// <summary>Sets the sway's maximum X radius and angle delta (<c>Instruction_SporeSpawn_SetMaxXRadiusAndAngleDelta</c>, $A5:E82D).</summary>
    SetMaxXRadiusAndAngleDelta,
    /// <summary>Widens the sway by eight pixels up to forty (<c>Instruction_SporeSpawn_IncreaseMaxXRadius</c>, $A5:E75F).</summary>
    IncreaseMaxXRadius,
    /// <summary>Starts or stops spore generation (<c>Instruction_SporeSpawn_SporeGenerationFlagInY</c>, $A5:E872).</summary>
    SetSporeGeneration,
    /// <summary>Queues a library-two sound (<c>Instruction_SporeSpawn_QueueSFXInY_Lib2_Max6</c>, $A5:E895).</summary>
    QueueSound,
    /// <summary>Clears the damaged flag (<c>Instruction_SporeSpawn_ClearDamagedFlag</c>, $A5:E771).</summary>
    ClearDamagedFlag,
    /// <summary>Hardens the body at its death position (<c>Instruction_SporeSpawn_Harden</c>, $A5:E87C).</summary>
    Harden,
    /// <summary>Spawns a dying explosion (<c>Instruction_SporeSpawn_SpawnDyingExplosion</c>, $A5:E9B1).</summary>
    SpawnDyingExplosion,
    /// <summary>Spawns a hardening dust cloud (<c>Instruction_SporeSpawn_SpawnHardeningDustCloud</c>, $A5:E96E).</summary>
    SpawnHardeningDust,
    /// <summary>Loads a death-sequence palette row (<c>Instruction_SporeSpawn_LoadDeathSequencePalette</c>, $A5:E8CA).</summary>
    LoadDeathPalette,
    /// <summary>Loads a death-sequence palette row as the fade target (<c>Instruction_SporeSpawn_LoadDeathSequenceTargetPalette</c>, $A5:E91C).</summary>
    LoadDeathTargetPalette,
    /// <summary>Requests the death drops (<c>Instruction_SporeSpawn_CallSporeSpawnDeathItemDropRoutine</c>, $A5:E8B1).</summary>
    RequestDeathDrops,
}
