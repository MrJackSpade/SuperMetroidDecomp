using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>A ceiling spore spawner's or stalk segment's typed animation in Spore Spawn's room.</summary>
internal sealed partial class SporeSpawnCeilingAnimation
    : EnemyProjectileFrameAnimation<SporeSpawnCeilingSegment, SporeSpawnCeilingCue>
{
    /// <summary>Creates a <see cref="SporeSpawnCeilingAnimation"/>.</summary>
    /// <param name="owner">The ceiling's cue owner, which carries out the spore releases the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public SporeSpawnCeilingAnimation(IEnemyProjectileCues<SporeSpawnCeilingCue> owner, SporeSpawnCeilingSegment segment)
        : base(segment, SporeSpawnProjectileAnimations.StartingWith(segment), EnemyProjectileCueRunner<SporeSpawnCeilingSegment, SporeSpawnCeilingCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<SporeSpawnCeilingSegment, EnemyProjectileFrameId, SporeSpawnCeilingCue> Segments => SporeSpawnProjectileAnimations.CeilingSegments;
}