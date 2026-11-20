namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A cue of a shot spore's burst (#2554).</summary>
internal enum SporeSpawnSporeCue
{
    /// <summary>Makes the shot spore harmless.</summary>
    SetImpactProperties,
    /// <summary>Queues the enemy-killed sound.</summary>
    QueueKilledSound,
    /// <summary>Requests the shot spore's drop.</summary>
    RequestDrop,
    /// <summary>Deletes the spore, ending its burst.</summary>
    Delete,
}
