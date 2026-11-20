using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A Tourian statue projectile's typed animation.</summary>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class TourianStatueProjectileAnimation(TourianStatueProjectileSegment segment, IEnemyProjectileCues<TourianStatueProjectileCue> cues)
    : EnemyProjectileFrameAnimation<TourianStatueProjectileSegment, TourianStatueProjectileCue>(TourianStatueProjectileAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<TourianStatueProjectileSegment, EnemyProjectileFrameId, TourianStatueProjectileCue> Segments => TourianStatueProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<TourianStatueProjectileSegment, Animation<TourianStatueProjectileSegment>> StartingAt => TourianStatueProjectileAnimations.StartingAt;
}
