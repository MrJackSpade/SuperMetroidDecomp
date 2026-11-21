using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>What one of Spore Spawn's spores keeps: the way it first drifts, and how far along its drift path it is.</summary>
/// <param name="drift">The way the spore first drifts, its emitter's.</param>
internal sealed partial class SporeSpawnSporeState(HorizontalDirection drift) : EnemyProjectileState
{
    /// <summary>The way the spore first drifts: right along its drift path as authored, left along it mirrored.</summary>
    internal HorizontalDirection Drift { get; } = drift;

    /// <summary>
    /// The step of its drift path the spore takes next (#2410): it starts at the path's first and repeats the path after
    /// its last.
    /// </summary>
    internal ListStep<Offset16<RoomPixels>> PathStep { get; set; } = ListStep<Offset16<RoomPixels>>.FirstOf(SporeSpawnProjectileDefinitions.Drift);
}
