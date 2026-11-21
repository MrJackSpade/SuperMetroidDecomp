using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A ceiling spore spawner's or stalk segment's typed animation in Spore Spawn's room.</summary>
/// <param name="owner">The ceiling's cue owner, which carries out the spore releases the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class SporeSpawnCeilingAnimation(IEnemyProjectileCues<SporeSpawnCeilingCue> owner, SporeSpawnCeilingSegment segment)
    : EnemyProjectileFrameAnimation<SporeSpawnCeilingSegment, SporeSpawnCeilingCue>(segment, SporeSpawnProjectileAnimations.StartingWith(segment), EnemyProjectileCueRunner<SporeSpawnCeilingSegment, SporeSpawnCeilingCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<SporeSpawnCeilingSegment, EnemyProjectileFrameId, SporeSpawnCeilingCue> Segments => SporeSpawnProjectileAnimations.CeilingSegments;
}