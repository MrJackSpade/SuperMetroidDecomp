using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A Spore Spawn spore's, emitter's or stalk segment's typed animation.</summary>
internal sealed partial class SporeSpawnProjectileAnimation
    : EnemyProjectileFrameAnimation<SporeSpawnProjectileSegment, SporeSpawnProjectileCue>
{
    /// <summary>Creates a <see cref="SporeSpawnProjectileAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public SporeSpawnProjectileAnimation(IEnemyProjectileCues<SporeSpawnProjectileCue> owner, SporeSpawnProjectileSegment segment)
        : base(segment, SporeSpawnProjectileAnimations.StartingWith(segment), EnemyProjectileCueRunner<SporeSpawnProjectileSegment, SporeSpawnProjectileCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<SporeSpawnProjectileSegment, EnemyProjectileFrameId, SporeSpawnProjectileCue> Segments => SporeSpawnProjectileAnimations.Segments;
}
