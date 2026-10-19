namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>How one of Spore Spawn's rectangles reacts when a Samus projectile hits it.</summary>
internal enum SporeSpawnHitboxShot
{
    /// <summary>The exposed vulnerable core: Spore Spawn's own shot reaction takes the hit.</summary>
    Vulnerable = 1,
    /// <summary>The shell and the shielded core: the shot is deflected as a dud.</summary>
    Dud,
}
