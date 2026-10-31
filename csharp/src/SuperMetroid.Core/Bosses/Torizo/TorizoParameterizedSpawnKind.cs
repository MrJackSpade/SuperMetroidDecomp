namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which parameterizedspawn a <see cref="TorizoCue.ParameterizedSpawn"/> cue is. Spawns a Torizo projectile with a spawn parameter.</summary>
internal enum TorizoParameterizedSpawnKind
{
    /// <summary>Spawns a Bomb Torizo sonic boom (<c>Instruction_Torizo_SpawnBombTorizoSonicBoomWithParameterY</c>, $AA:C5E3).</summary>
    SpawnBombTorizoSonicBoom,
    /// <summary>Spawns a Golden Torizo sonic boom (<c>Instruction_Torizo_SpawnGoldenTorizoSonicBoomWithParameterY</c>, $AA:C5F2).</summary>
    SpawnGoldenTorizoSonicBoom,
    /// <summary>Spawns an explosive swipe (<c>Instruction_Torizo_SpawnBombTorizoExplosiveSwipeWithParamY</c>, $AA:C601).</summary>
    SpawnExplosiveSwipe,
    /// <summary>Spawns an eye beam (<c>Instruction_GoldenTorizo_SpawnEyeBeam</c>, $AA:D436).</summary>
    SpawnEyeBeam,
}
