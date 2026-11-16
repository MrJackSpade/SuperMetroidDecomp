namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>What the Ceres explosion spawner's animation has its owner do (#2197).</summary>
internal enum CeresExplosionSpawnerCue
{
    /// <summary>Spawns Ceres's initial square-and-centre explosion wave.</summary>
    SpawnInitialWave,
    /// <summary>Starts the spawner's repeating explosions.</summary>
    StartRepeatingExplosions,
    /// <summary>Spawns Ceres's final large-burst explosion wave.</summary>
    SpawnFinalWave,
}
