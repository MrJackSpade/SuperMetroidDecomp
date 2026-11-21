namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The ceiling's typed projectile animations in Spore Spawn's room (#641, #2553), in authored order.</summary>
internal enum SporeSpawnCeilingSegment
{
    /// <summary>The spawner shut, its one pose held until it next releases a spore.</summary>
    SpawnerClosed,
    /// <summary>The spawner releasing a spore.</summary>
    SpawnerRelease,
    /// <summary>A stalk segment.</summary>
    Stalk,
}