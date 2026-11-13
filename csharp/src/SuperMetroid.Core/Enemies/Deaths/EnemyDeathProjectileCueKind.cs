namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>What a cue of an enemy's death explosion and drop animations does: turn the explosion into its drop, delete it, respawn the enemy, queue a death sound, or spawn a sprite object near the explosion.</summary>
internal enum EnemyDeathProjectileCueKind
{
    /// <summary>Turns the explosion into its rolled drop, or into the respawn tail when nothing drops.</summary>
    BecomePickup,
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Respawns the killed enemy when its room keeps it.</summary>
    HandleRespawningEnemy,
    /// <summary>Queues the killed-by-contact sound.</summary>
    PlayContactKilledSound,
    /// <summary>Queues the enemy-killed sound.</summary>
    PlayEnemyKilledSound,
    /// <summary>Queues the small-explosion sound.</summary>
    PlaySmallExplosionSound,
    /// <summary>Spawns the sprite object up to 16 pixels from the explosion.</summary>
    SpawnSpriteWithin16,
    /// <summary>Spawns the sprite object up to 32 pixels from the explosion.</summary>
    SpawnSpriteWithin32,
}
