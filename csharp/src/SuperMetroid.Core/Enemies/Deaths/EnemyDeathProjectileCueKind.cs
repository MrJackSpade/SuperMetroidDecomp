namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>What a cue of an enemy's death explosion and drop programs does: one per bank-$86 command its lists use.</summary>
internal enum EnemyDeathProjectileCueKind
{
    /// <summary>Turns the explosion into its rolled drop, or into the respawn tail when nothing drops (<c>Instruction_EnemyProjectile_EnemyDeathExplosion_BecomePickup</c>, $86:EEAF).</summary>
    BecomePickup,
    /// <summary>Deletes the projectile (<c>Instruction_EnemyProjectile_Delete</c>, $86:8154).</summary>
    Delete,
    /// <summary>Respawns the killed enemy when its room keeps it (<c>Instruction_EnemyProjectile_Pickup_HandleRespawningEnemy</c>, $86:EF10).</summary>
    HandleRespawningEnemy,
    /// <summary>Queues the killed-by-contact sound (<c>Instruction_EnemyProj_EDeathExplo_QueueContactKilledSoundFX</c>, $86:EEA3).</summary>
    PlayContactKilledSound,
    /// <summary>Queues the enemy-killed sound (<c>Instruction_EnemyProj_EnemyDeathExpl_QueueEnemyKilledSoundFX</c>, $86:EE8B).</summary>
    PlayEnemyKilledSound,
    /// <summary>Queues the small-explosion sound (<c>Instruction_EnemyProj_EDeathExplo_QueueSmallExplosionSoundFX</c>, $86:EE97).</summary>
    PlaySmallExplosionSound,
    /// <summary>Spawns the sprite object up to 16 pixels from the explosion (<c>Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_10</c>, $86:ED17).</summary>
    SpawnSpriteWithin16,
    /// <summary>Spawns the sprite object up to 32 pixels from the explosion (<c>Instruction_EnemyProj_EnemyDeathExpl_SpawnSpriteObjectInY_20</c>, $86:ECE3).</summary>
    SpawnSpriteWithin32,
}
