using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A Spore Spawn spore's typed animation: its fall, or its burst once shot.</summary>
/// <param name="owner">The spores' cue owner, which carries out the burst's cues (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class SporeSpawnSporeAnimation(IEnemyProjectileCues<SporeSpawnSporeCue> owner, SporeSpawnSporeSegment segment)
    : EnemyProjectileFrameAnimation<SporeSpawnSporeSegment, SporeSpawnSporeCue>(segment, SporeSpawnProjectileAnimations.StartingWith(segment), EnemyProjectileCueRunner<SporeSpawnSporeSegment, SporeSpawnSporeCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<SporeSpawnSporeSegment, EnemyProjectileFrameId, SporeSpawnSporeCue> Segments => SporeSpawnProjectileAnimations.SporeSegments;
}