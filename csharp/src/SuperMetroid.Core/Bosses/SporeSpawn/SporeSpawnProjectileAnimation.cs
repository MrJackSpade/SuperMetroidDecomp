using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A Spore Spawn spore's, emitter's or stalk segment's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class SporeSpawnProjectileAnimation(IEnemyProjectileCues<SporeSpawnProjectileCue> owner, SporeSpawnProjectileSegment segment)
    : EnemyProjectileFrameAnimation<SporeSpawnProjectileSegment, SporeSpawnProjectileCue>(SporeSpawnProjectileAnimations.StartingWith(segment), EnemyProjectileCueRunner<SporeSpawnProjectileSegment, SporeSpawnProjectileCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override UnpausedAnimationSegments<SporeSpawnProjectileSegment, EnemyProjectileFrameId, SporeSpawnProjectileCue> Segments => SporeSpawnProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<SporeSpawnProjectileSegment, Animation<SporeSpawnProjectileSegment>>> StartingAt => SporeSpawnProjectileAnimations.StartingAt;
}
