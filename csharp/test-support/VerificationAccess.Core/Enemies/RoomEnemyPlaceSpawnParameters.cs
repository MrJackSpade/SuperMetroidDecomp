namespace SuperMetroid.Core.Enemies;

/// <summary>Authors a fixture slot's spawn settings, which the enemy's initialization and authored reads take.</summary>
internal static class RoomEnemyPlaceSpawnParameters
{
    /// <summary>Sets the spawn settings the slot's population entry gives its species (#1471).</summary>
    internal static void SetSpawnSettings(this RoomEnemyPlace place, EnemySpawnSettings settings) =>
        place.Spawn = place.Spawn with { Population = place.Spawn.Population with { SpawnSettings = settings } };

    /// <summary>
    /// The settings a fixture slot's population entry gives its species, as the <typeparamref name="T"/> the check placed it
    /// with: the species itself receives them typed from its entry (#2554).
    /// </summary>
    internal static T SpawnSettings<T>(this RoomEnemyPlace place) where T : EnemySpawnSettings =>
        (T)place.Spawn.Population.SpawnSettings;

    /// <summary>The settings a population entry gives its species, as the <typeparamref name="T"/> the check knows it carries.</summary>
    internal static T SpawnSettingsOf<T>(this RoomEnemyPlacement placement) where T : EnemySpawnSettings =>
        (T)placement.SpawnSettings;
}
