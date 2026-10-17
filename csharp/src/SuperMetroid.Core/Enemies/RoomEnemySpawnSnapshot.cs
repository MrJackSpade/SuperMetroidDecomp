namespace SuperMetroid.Core.Enemies;

/// <summary>Immutable spawn words retained beside the mutable native enemy slot.</summary>
/// <param name="Population">Original sixteen-byte population values, preserved when the live enemy position, properties, or parameters change.</param>
public readonly record struct RoomEnemySpawnSnapshot(
    RoomEnemyPopulationRecord Population);
