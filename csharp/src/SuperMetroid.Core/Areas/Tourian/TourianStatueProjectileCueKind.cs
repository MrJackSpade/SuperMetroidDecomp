namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>What a statue projectile animation cue does.</summary>
internal enum TourianStatueProjectileCueKind
{
    /// <summary>Deletes the projectile.</summary>
    Delete,
    /// <summary>Queues the statue release sound.</summary>
    PlayReleaseSound,
    /// <summary>Starts the unlocking earthquake.</summary>
    Earthquake,
    /// <summary>Spawns an unlocking particle at the projectile.</summary>
    SpawnParticle,
    /// <summary>Spawns the particle's tail at the projectile.</summary>
    SpawnTail,
}
