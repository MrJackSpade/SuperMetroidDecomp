namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>What a cue of Spore Spawn's body animations does: widen the sway, clear the damaged flag, harden at the death position, spawn the dying explosions and dust, or request the death drops.</summary>
internal enum SporeSpawnCueKind
{
    /// <summary>Widens the sway by eight pixels up to forty.</summary>
    IncreaseMaxXRadius,
    /// <summary>Clears the damaged flag.</summary>
    ClearDamagedFlag,
    /// <summary>Hardens the body at its death position.</summary>
    Harden,
    /// <summary>Spawns a dying explosion.</summary>
    SpawnDyingExplosion,
    /// <summary>Spawns a hardening dust cloud.</summary>
    SpawnHardeningDust,
    /// <summary>Requests the death drops.</summary>
    RequestDeathDrops,
}
