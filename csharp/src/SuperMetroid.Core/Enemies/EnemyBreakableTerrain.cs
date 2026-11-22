using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Tells which enemy spike blocks an enemy breaks rather than meets as solid: every enemy-breakable spike block, in any
/// area, breaks (<see cref="EnemyBreakableTerrainObjects.SpawnEnemyBreakableBlock"/>).
/// </summary>
public static partial class EnemyBreakableTerrain
{
    /// <summary>Whether the spike block carrying <paramref name="spike"/> spawns the enemy-breakable block: only the enemy-breakable spikes do.</summary>
    /// <param name="spike">The reaction the spike block carries.</param>
    public static bool IsEnemyBreakable(SpikeReaction spike) => spike.BreaksUnderEnemies;
}
