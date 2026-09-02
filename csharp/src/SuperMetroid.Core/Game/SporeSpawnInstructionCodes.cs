namespace SuperMetroid.Core.Game;

/// <summary>Named bank-$A5 code pointers consumed by translated Spore Spawn dispatchers.</summary>
internal static class SporeSpawnInstructionCodes
{
    /// <summary><c>Instruction_SporeSpawn_IncreaseMaxXRadius</c> at $A5:E75F.</summary>
    public const ushort Instruction_SporeSpawn_IncreaseMaxXRadius = 0xe75f;

    /// <summary><c>Instruction_SporeSpawn_ClearDamagedFlag</c> at $A5:E771.</summary>
    public const ushort Instruction_SporeSpawn_ClearDamagedFlag = 0xe771;

    /// <summary><c>Instruction_SporeSpawn_SetMaxXRadiusAndAngleDelta</c> at $A5:E82D.</summary>
    public const ushort Instruction_SporeSpawn_SetMaxXRadiusAndAngleDelta = 0xe82d;

    /// <summary><c>Instruction_SporeSpawn_SporeGenerationFlagInY</c> at $A5:E872.</summary>
    public const ushort Instruction_SporeSpawn_SporeGenerationFlagInY = 0xe872;

    /// <summary><c>Instruction_SporeSpawn_Harden</c> at $A5:E87C.</summary>
    public const ushort Instruction_SporeSpawn_Harden = 0xe87c;

    /// <summary><c>Instruction_SporeSpawn_QueueSFXInY_Lib2_Max6</c> at $A5:E895.</summary>
    public const ushort Instruction_SporeSpawn_QueueSFXInY_Lib2_Max6 = 0xe895;

    /// <summary><c>Instruction_SporeSpawn_CallSporeSpawnDeathItemDropRoutine</c> at $A5:E8B1.</summary>
    public const ushort Instruction_SporeSpawn_CallSporeSpawnDeathItemDropRoutine = 0xe8b1;

    /// <summary><c>Instruction_SporeSpawn_FunctionInY</c> at $A5:E8BA.</summary>
    public const ushort Instruction_SporeSpawn_FunctionInY = 0xe8ba;

    /// <summary><c>Instruction_SporeSpawn_LoadDeathSequencePalette</c> at $A5:E8CA.</summary>
    public const ushort Instruction_SporeSpawn_LoadDeathSequencePalette = 0xe8ca;

    /// <summary><c>Instruction_SporeSpawn_LoadDeathSequenceTargetPalette</c> at $A5:E91C.</summary>
    public const ushort Instruction_SporeSpawn_LoadDeathSequenceTargetPalette = 0xe91c;

    /// <summary><c>Instruction_SporeSpawn_SpawnHardeningDustCloud</c> at $A5:E96E.</summary>
    public const ushort Instruction_SporeSpawn_SpawnHardeningDustCloud = 0xe96e;

    /// <summary><c>Instruction_SporeSpawn_SpawnDyingExplosion</c> at $A5:E9B1.</summary>
    public const ushort Instruction_SporeSpawn_SpawnDyingExplosion = 0xe9b1;

}
