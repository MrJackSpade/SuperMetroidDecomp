using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A Draygon goop's or wall-turret shot's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class DraygonProjectileAnimation(IEnemyProjectileCues<DraygonProjectileCue> owner, DraygonProjectileSegment segment)
    : EnemyProjectileFrameAnimation<DraygonProjectileSegment, DraygonProjectileCue>(DraygonProjectileAnimations.StartingWith(segment), EnemyProjectileCueRunner<DraygonProjectileSegment, DraygonProjectileCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override UnpausedAnimationSegments<DraygonProjectileSegment, EnemyProjectileFrameId, DraygonProjectileCue> Segments =>
        DraygonProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<DraygonProjectileSegment, Animation<DraygonProjectileSegment>>> StartingAt =>
        DraygonProjectileAnimations.StartingAt;
}
