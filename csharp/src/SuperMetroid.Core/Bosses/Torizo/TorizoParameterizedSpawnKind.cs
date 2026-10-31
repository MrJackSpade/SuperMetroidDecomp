namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which parameterizedspawn a <see cref="TorizoCue.ParameterizedSpawn"/> cue is. Spawns a Torizo projectile with a spawn parameter.</summary>
internal enum TorizoParameterizedSpawnKind
{
    /// <summary>Spawns a Bomb Torizo sonic boom.</summary>
    SpawnBombTorizoSonicBoom,
    /// <summary>Spawns a Golden Torizo sonic boom.</summary>
    SpawnGoldenTorizoSonicBoom,
    /// <summary>Spawns an explosive swipe.</summary>
    SpawnExplosiveSwipe,
    /// <summary>Spawns an eye beam.</summary>
    SpawnEyeBeam,
}
