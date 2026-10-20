namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>What a cue of Spore Spawn's body programs does: one per bank-$A5 Spore Spawn instruction its lists use.</summary>
internal enum SporeSpawnCueKind
{
    /// <summary>Widens the sway by eight pixels up to forty (<c>Instruction_SporeSpawn_IncreaseMaxXRadius</c>, $A5:E75F).</summary>
    IncreaseMaxXRadius,
    /// <summary>Clears the damaged flag (<c>Instruction_SporeSpawn_ClearDamagedFlag</c>, $A5:E771).</summary>
    ClearDamagedFlag,
    /// <summary>Hardens the body at its death position (<c>Instruction_SporeSpawn_Harden</c>, $A5:E87C).</summary>
    Harden,
    /// <summary>Spawns a dying explosion (<c>Instruction_SporeSpawn_SpawnDyingExplosion</c>, $A5:E9B1).</summary>
    SpawnDyingExplosion,
    /// <summary>Spawns a hardening dust cloud (<c>Instruction_SporeSpawn_SpawnHardeningDustCloud</c>, $A5:E96E).</summary>
    SpawnHardeningDust,
    /// <summary>Requests the death drops (<c>Instruction_SporeSpawn_CallSporeSpawnDeathItemDropRoutine</c>, $A5:E8B1).</summary>
    RequestDeathDrops,
}
