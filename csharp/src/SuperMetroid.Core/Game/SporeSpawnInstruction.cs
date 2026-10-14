namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A5 code pointers consumed by translated Spore Spawn dispatchers.</summary>
internal enum SporeSpawnInstruction : ushort
{
    /// <summary><c>Instruction_SporeSpawn_IncreaseMaxXRadius</c> at $A5:E75F.</summary>
    IncreaseMaxXRadius = 0xe75f,

    /// <summary><c>Instruction_SporeSpawn_ClearDamagedFlag</c> at $A5:E771.</summary>
    ClearDamagedFlag = 0xe771,

    /// <summary><c>Instruction_SporeSpawn_SetMaxXRadiusAndAngleDelta</c> at $A5:E82D.</summary>
    SetMaxXRadiusAndAngleDelta = 0xe82d,

    /// <summary><c>Instruction_SporeSpawn_SporeGenerationFlagInY</c> at $A5:E872.</summary>
    SporeGenerationFlagInY = 0xe872,

    /// <summary><c>Instruction_SporeSpawn_Harden</c> at $A5:E87C.</summary>
    Harden = 0xe87c,

    /// <summary><c>Instruction_SporeSpawn_QueueSFXInY_Lib2_Max6</c> at $A5:E895.</summary>
    QueueSFXInY_Lib2_Max6 = 0xe895,

    /// <summary><c>Instruction_SporeSpawn_CallSporeSpawnDeathItemDropRoutine</c> at $A5:E8B1.</summary>
    CallSporeSpawnDeathItemDropRoutine = 0xe8b1,

    /// <summary><c>Instruction_SporeSpawn_FunctionInY</c> at $A5:E8BA.</summary>
    FunctionInY = 0xe8ba,

    /// <summary><c>Instruction_SporeSpawn_LoadDeathSequencePalette</c> at $A5:E8CA.</summary>
    LoadDeathSequencePalette = 0xe8ca,

    /// <summary><c>Instruction_SporeSpawn_LoadDeathSequenceTargetPalette</c> at $A5:E91C.</summary>
    LoadDeathSequenceTargetPalette = 0xe91c,

    /// <summary><c>Instruction_SporeSpawn_SpawnHardeningDustCloud</c> at $A5:E96E.</summary>
    SpawnHardeningDustCloud = 0xe96e,

    /// <summary><c>Instruction_SporeSpawn_SpawnDyingExplosion</c> at $A5:E9B1.</summary>
    SpawnDyingExplosion = 0xe9b1,

}
