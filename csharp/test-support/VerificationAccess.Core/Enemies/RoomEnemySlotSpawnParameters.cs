namespace SuperMetroid.Core.Enemies;

/// <summary>Authors a fixture slot's spawn settings, which the enemy's initialization and authored reads take.</summary>
internal static class RoomEnemySlotSpawnParameters
{
    /// <summary>Sets the spawn settings the slot's population entry gives its species (#1471).</summary>
    internal static void SetSpawnSettings(this RoomEnemySlot slot, EnemySpawnSettings settings) =>
        slot.Spawn = slot.Spawn with { Population = slot.Spawn.Population with { SpawnSettings = settings } };
}
