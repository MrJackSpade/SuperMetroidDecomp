namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>What a signalling cue of an enemy's death explosion does: become its drop, or queue a death sound (#2554).</summary>
internal enum EnemyDeathExplosionCueKind
{
    /// <summary>Turns the explosion into its rolled drop, or into the respawn pause when nothing drops.</summary>
    BecomePickup,
    /// <summary>Queues the killed-by-contact sound.</summary>
    PlayContactKilledSound,
    /// <summary>Queues the enemy-killed sound.</summary>
    PlayEnemyKilledSound,
    /// <summary>Queues the small-explosion sound.</summary>
    PlaySmallExplosionSound,
}
