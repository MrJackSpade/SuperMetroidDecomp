namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// How a ceiling spore spawner in Spore Spawn's room moves itself (#1843, #2410): only a spawner's spawn starts it, so its
/// release restarts only spawners.
/// </summary>
internal enum SporeSpawnSpawnerMotion
{
    /// <summary>Releases spores on the spawner's random cadence while Spore Spawn lets it.</summary>
    Releasing,
}
