namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The cues a Spore Spawn spore's animations publish (#2553): its burst's, once it is shot.</summary>
internal enum SporeSpawnSporeCue
{
    /// <summary>Makes a shot spore harmless.</summary>
    SetImpactProperties,
    /// <summary>Queues the enemy-killed sound.</summary>
    QueueKilledSound,
    /// <summary>Requests a shot spore's drop.</summary>
    RequestDrop,
    /// <summary>Deletes the projectile, ending a shot spore's animation.</summary>
    Delete,
}