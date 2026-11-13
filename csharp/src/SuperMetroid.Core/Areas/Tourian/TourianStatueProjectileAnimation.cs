using SuperMetroid.Core.Areas.Tourian.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A Tourian statue projectile's typed animation.</summary>
internal sealed class TourianStatueProjectileAnimation(TourianStatueProjectileSegment segment)
    : EnemyProjectileFrameAnimation<TourianStatueProjectileSegment, TourianStatueProjectileCue>(TourianStatueProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<TourianStatueProjectileSegment, EnemyProjectileFrameId, TourianStatueProjectileCue> Segments => TourianStatueProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<TourianStatueProjectileSegment, Animation<TourianStatueProjectileSegment>> StartingAt => TourianStatueProjectileAnimations.StartingAt;
}
