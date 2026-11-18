namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>What the Ceres explosion spawner's animation has the destruction scene do (#2197, #2489).</summary>
internal enum CeresExplosionSpawnerCue
{
    /// <summary>Spawns Ceres's first wave: the five small blasts round the square and its centre.</summary>
    SpawnInitialWave,
    /// <summary>Spawns the next of the repeating small blasts round the station, unless the gunship is already flying away.</summary>
    SpawnRepeatingBlast,
    /// <summary>Spawns Ceres's final wave: the four large blasts.</summary>
    SpawnFinalWave,
}
