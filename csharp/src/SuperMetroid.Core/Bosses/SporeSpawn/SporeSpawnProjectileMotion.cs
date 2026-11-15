namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// How a Spore Spawn spore or ceiling spawner moves itself (#1843).
/// </summary>
internal enum SporeSpawnProjectileMotion
{
    /// <summary>Drifts a spore along its authored drift.</summary>
    SporeDrifting,

    /// <summary>Releases spores from the ceiling spawner on a random cadence while Spore Spawn is closed.</summary>
    SpawnerReleasing,
}
