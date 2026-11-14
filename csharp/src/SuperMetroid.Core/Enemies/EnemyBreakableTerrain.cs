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
    /// <exception cref="InvalidDataException">The behaviour gives the spike no reaction.</exception>
    public static bool IsEnemyBreakable(RoomBlockBehavior behavior) => behavior.Spike switch
    {
        SpikeReaction.EnemyBreakable or SpikeReaction.EnemyBreakableLetsGrappleThrough => true,
        SpikeReaction.HeavySpike or SpikeReaction.LightSpike or SpikeReaction.DamagingSpikeAir or SpikeReaction.TurretSpike or
            SpikeReaction.Harmless or SpikeReaction.XrayRevealed or SpikeReaction.HarmlessLetsGrappleThrough => false,
        null => throw new InvalidDataException(
            $"Enemy spike behaviour {behavior} gives the spike no reaction."),
        _ => throw new InvalidOperationException($"Undefined {nameof(SpikeReaction)} {behavior.Spike}."),
    };
}
