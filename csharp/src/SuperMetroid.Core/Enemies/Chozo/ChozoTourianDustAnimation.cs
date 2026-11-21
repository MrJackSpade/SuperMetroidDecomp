using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Chozo/Tourian dust's typed animation: the footstep dust, which starts at its segment, or the Tourian statue's descent
/// dust, started by name (<see cref="ChozoTourianDustAnimations.TourianDescentDust"/>, #2061).
/// </summary>
internal sealed partial class ChozoTourianDustAnimation
    : EnemyProjectileFrameAnimation<ChozoTourianDustSegment, EnemyProjectileCommonCue>
{
    /// <summary>Creates a <see cref="ChozoTourianDustAnimation"/>.</summary>
    /// <param name="animation">animation</param>
    public ChozoTourianDustAnimation(Animation<ChozoTourianDustSegment> animation)
        : base(animation.Phases[0].Segment, animation, EnemyProjectileCueRunners.Common<ChozoTourianDustSegment>())
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<ChozoTourianDustSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => ChozoTourianDustAnimations.Segments;
}
