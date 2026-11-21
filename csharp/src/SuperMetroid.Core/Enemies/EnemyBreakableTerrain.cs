using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Tells which enemy spike blocks an enemy breaks rather than meets as solid: every enemy-breakable spike block, in any
/// area, breaks (<see cref="EnemyBreakableTerrainObjects.SpawnEnemyBreakableBlock"/>).
/// </summary>
public static class EnemyBreakableTerrain
{
    /// <summary>Whether the spike block with <paramref name="behavior"/> spawns the enemy-breakable block.</summary>
    /// <exception cref="InvalidDataException">The behaviour gives the spike no reaction.</exception>
    public static bool IsEnemyBreakable(RoomBlockBehavior behavior) => behavior.Spike is { } spike
        ? spike.BreaksUnderEnemies
        : throw new InvalidDataException($"Enemy spike behaviour {behavior} gives the spike no reaction.");
}
