using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One of the Tourian entrance statue's decoration animations (#2554), whose cue reaches the decoration's behaviour.</summary>
/// <param name="segment">The decoration the animation shows.</param>
/// <param name="cues">The decoration's behaviour, which owns the animation's cue.</param>
internal sealed class TourianStatueDecorationAnimation(TourianStatueDecorationSegment segment, IEnemyProjectileCues<TourianStatueDecorationCue> cues)
    : EnemyProjectileFrameAnimation<TourianStatueDecorationSegment, TourianStatueDecorationCue>(TourianStatueDecorationAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<TourianStatueDecorationSegment, EnemyProjectileFrameId, TourianStatueDecorationCue> Segments => TourianStatueDecorationAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<TourianStatueDecorationSegment, Animation<TourianStatueDecorationSegment>> StartingAt => TourianStatueDecorationAnimations.StartingAt;
}
