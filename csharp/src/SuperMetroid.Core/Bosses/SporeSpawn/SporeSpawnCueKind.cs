namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>What a cue of Spore Spawn's body programs does: one per kind of step its lists use.</summary>
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
