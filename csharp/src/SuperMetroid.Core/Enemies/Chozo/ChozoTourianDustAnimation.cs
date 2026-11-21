using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Chozo/Tourian dust's typed animation: the footstep dust, which starts at its segment, or the Tourian statue's descent
/// dust, started by name (<see cref="ChozoTourianDustAnimations.TourianDescentDust"/>, #2061).
/// </summary>
internal sealed class ChozoTourianDustAnimation(Animation<ChozoTourianDustSegment> animation)
    : EnemyProjectileFrameAnimation<ChozoTourianDustSegment, EnemyProjectileCommonCue>(animation.Phases[0].Segment, animation, EnemyProjectileCueRunners.Common<ChozoTourianDustSegment>())
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<ChozoTourianDustSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => ChozoTourianDustAnimations.Segments;
}
