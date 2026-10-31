namespace SuperMetroid.Core.Enemies;

/// <summary>The population entry an enemy was spawned from, kept unchanged beside its live state.</summary>
/// <param name="Population">The entry, preserved while the enemy's live position and properties change.</param>
public readonly record struct RoomEnemySpawnSnapshot(
    RoomEnemyPopulationRecord Population);
