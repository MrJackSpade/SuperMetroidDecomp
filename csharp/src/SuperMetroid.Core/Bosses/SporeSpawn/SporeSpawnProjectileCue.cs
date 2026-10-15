namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The cues Spore Spawn's projectile programs publish.</summary>
internal enum SporeSpawnProjectileCue
{
    /// <summary>Releases a spore at the emitter (<c>Instruction_EnemyProjectile_SporeSpawner_SpawnSpore</c>, $86:DC77).</summary>
    SpawnSpore,
    /// <summary>Makes a shot spore harmless (<c>Instruction_EnemyProjectile_Spores_SetProperties3000</c>, $86:DC5A).</summary>
    SetImpactProperties,
    /// <summary>Queues the enemy-killed sound (<c>Instruction_EnemyProj_EnemyDeathExpl_QueueEnemyKilledSoundFX</c>, $86:EE8B).</summary>
    QueueKilledSound,
    /// <summary>Requests a shot spore's drop (<c>Instruction_EnemyProjectile_Spores_SpawnEnemyDrops</c>, $86:DC61).</summary>
    RequestDrop,
    /// <summary>Deletes the projectile, as the shot list's goto into the common deletion list ($86:84FC) does.</summary>
    Delete,
}
