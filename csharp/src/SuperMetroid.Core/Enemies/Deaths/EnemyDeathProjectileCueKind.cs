namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>What a signalling cue of an enemy's death explosion and drop animations does: turn the explosion into its drop, delete it, respawn the enemy, or queue a death sound.</summary>
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
}
