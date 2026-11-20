using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Chozo/Tourian dust's typed animation: the footstep dust, which starts at its segment, or the Tourian statue's descent
/// dust, started by name (<see cref="ChozoTourianDustAnimations.TourianDescentDust"/>, #2061).
/// </summary>
internal sealed class ChozoTourianDustAnimation(Animation<ChozoTourianDustSegment> animation)
    : EnemyProjectileFrameAnimation<ChozoTourianDustSegment, EnemyProjectileCommonCue>(animation, EnemyProjectileCueRunners.Common<ChozoTourianDustSegment>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<ChozoTourianDustSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => ChozoTourianDustAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<ChozoTourianDustSegment, Animation<ChozoTourianDustSegment>> StartingAt => ChozoTourianDustAnimations.StartingAt;
}
