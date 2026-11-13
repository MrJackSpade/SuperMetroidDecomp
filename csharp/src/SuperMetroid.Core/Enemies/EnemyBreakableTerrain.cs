using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Tells which enemy spike blocks an enemy breaks rather than meets as solid: the enemy's spike reaction reads a spike's
/// reaction from either its common or its area's list alike, and spawns the block's break
/// (<see cref="EnemyBreakableTerrainObjects.TrySpawnEnemyBreakableBlock"/>) for the enemy-breakable reaction alone.
/// </summary>
public static class EnemyBreakableTerrain
{
    /// <summary>Whether the spike block with <paramref name="behavior"/> spawns the enemy-breakable block.</summary>
    /// <exception cref="InvalidDataException">The behavior's reaction lies past the enemy-breakable reaction table.</exception>
    public static bool IsEnemyBreakable(RoomBlockBehavior behavior) => behavior.Spike switch
    {
        SpikeReaction.EnemyBreakable or SpikeReaction.AreaEnemyBreakable => true,
        SpikeReaction.HeavySpike or SpikeReaction.LightSpike or SpikeReaction.DamagingSpikeAir or SpikeReaction.TurretSpike or
            SpikeReaction.Harmless or SpikeReaction.XrayRevealed or SpikeReaction.AreaHarmless => false,
        SpikeReaction.PastEnemyTable or SpikeReaction.AreaPastEnemyTable => throw new InvalidDataException(
            $"Enemy spike behaviour {behavior} names a reaction past the sixteen enemy-breakable reactions."),
        _ => throw new InvalidOperationException($"Undefined {nameof(SpikeReaction)} {behavior.Spike}."),
    };
}
