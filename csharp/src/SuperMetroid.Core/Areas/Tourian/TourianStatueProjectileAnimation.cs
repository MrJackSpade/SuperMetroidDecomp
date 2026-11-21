using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A Tourian statue projectile's typed animation.</summary>
internal sealed partial class TourianStatueProjectileAnimation
    : EnemyProjectileFrameAnimation<TourianStatueProjectileSegment, TourianStatueProjectileCue>
{
    /// <summary>Creates a <see cref="TourianStatueProjectileAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public TourianStatueProjectileAnimation(IEnemyProjectileCues<TourianStatueProjectileCue> owner, TourianStatueProjectileSegment segment)
        : base(segment, TourianStatueProjectileAnimations.StartingAt[segment], EnemyProjectileCueRunner<TourianStatueProjectileSegment, TourianStatueProjectileCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<TourianStatueProjectileSegment, EnemyProjectileFrameId, TourianStatueProjectileCue> Segments => TourianStatueProjectileAnimations.Segments;
}
