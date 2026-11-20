using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A Tourian statue projectile's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class TourianStatueProjectileAnimation(IEnemyProjectileCues<TourianStatueProjectileCue> owner, TourianStatueProjectileSegment segment)
    : EnemyProjectileFrameAnimation<TourianStatueProjectileSegment, TourianStatueProjectileCue>(TourianStatueProjectileAnimations.StartingWith(segment), EnemyProjectileCueRunner<TourianStatueProjectileSegment, TourianStatueProjectileCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<TourianStatueProjectileSegment, EnemyProjectileFrameId, TourianStatueProjectileCue> Segments => TourianStatueProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<TourianStatueProjectileSegment, Animation<TourianStatueProjectileSegment>>> StartingAt => TourianStatueProjectileAnimations.StartingAt;
}
