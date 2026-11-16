namespace SuperMetroid.Core.Enemies;

/// <summary>Authors a fixture slot's spawn settings, which the enemy's initialization and authored reads take.</summary>
internal static class RoomEnemyPlaceSpawnParameters
{
    /// <summary>Sets the spawn settings the slot's population entry gives its species (#1471).</summary>
    internal static void SetSpawnSettings(this RoomEnemyPlace place, EnemySpawnSettings settings) =>
        place.Spawn = place.Spawn with { Population = place.Spawn.Population with { SpawnSettings = settings } };
}
