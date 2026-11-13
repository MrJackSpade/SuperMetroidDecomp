namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The cues Spore Spawn's projectile animations publish.</summary>
internal enum SporeSpawnProjectileCue
{
    /// <summary>Releases a spore at the emitter.</summary>
    SpawnSpore,
    /// <summary>Makes a shot spore harmless.</summary>
    SetImpactProperties,
    /// <summary>Queues the enemy-killed sound.</summary>
    QueueKilledSound,
    /// <summary>Requests a shot spore's drop.</summary>
    RequestDrop,
    /// <summary>Deletes the projectile, as the shot animation's jump into the common deletion animation does.</summary>
    Delete,
}
