namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's typed projectile programs (#641), in the authored order of their bank-$86 lists.</summary>
internal enum SporeSpawnProjectileProgram
{
    /// <summary>The closed ceiling emitter ($86:DC00).</summary>
    SpawnerClosed,
    /// <summary>The emitter releasing a spore ($86:DC06).</summary>
    SpawnerRelease,
    /// <summary>A falling spore's loop ($86:DC1E).</summary>
    Spore,
    /// <summary>A stalk segment ($86:DC2E).</summary>
    Stalk,
    /// <summary>A shot spore's burst and drop ($86:DC34).</summary>
    SporeShot,
}
