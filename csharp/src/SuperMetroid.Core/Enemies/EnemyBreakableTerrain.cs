using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Tells which enemy spike blocks an enemy breaks rather than meets as solid: every enemy-breakable spike block, in any
/// area, breaks (<see cref="EnemyBreakableTerrainObjects.SpawnEnemyBreakableBlock"/>).
/// </summary>
public static class EnemyBreakableTerrain
{
    /// <summary>
    /// Whether the spike block with <paramref name="behavior"/> spawns the enemy-breakable block: only the enemy-breakable
    /// spikes do, so a behaviour that names no spike reaction is no breakable one.
    /// </summary>
    public static bool IsEnemyBreakable(RoomBlockBehavior behavior) => behavior.Spike switch
    {
        SpikeReaction.EnemyBreakable or SpikeReaction.EnemyBreakableLetsGrappleThrough => true,
        SpikeReaction.HeavySpike or SpikeReaction.LightSpike or SpikeReaction.DamagingSpikeAir or SpikeReaction.TurretSpike or
            SpikeReaction.Harmless or SpikeReaction.XrayRevealed or SpikeReaction.HarmlessLetsGrappleThrough => false,
        null => false,
        _ => throw new InvalidOperationException($"Undefined {nameof(SpikeReaction)} {behavior.Spike}."),
    };
}
