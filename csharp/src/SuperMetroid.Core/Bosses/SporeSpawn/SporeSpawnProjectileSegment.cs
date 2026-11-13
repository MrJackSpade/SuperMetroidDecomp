namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's typed projectile animations (#641), in authored order.</summary>
internal enum SporeSpawnProjectileSegment
{
    /// <summary>The closed ceiling emitter.</summary>
    SpawnerClosed,
    /// <summary>The emitter releasing a spore.</summary>
    SpawnerRelease,
    /// <summary>A falling spore's loop.</summary>
    Spore,
    /// <summary>A stalk segment.</summary>
    Stalk,
    /// <summary>A shot spore's burst and drop.</summary>
    SporeShot,
}
