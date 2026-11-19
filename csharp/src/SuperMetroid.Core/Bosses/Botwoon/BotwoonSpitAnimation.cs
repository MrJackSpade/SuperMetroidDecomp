using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's spit's typed animation (#2400): its glowing frames, cycled for as long as it flies.</summary>
internal sealed class BotwoonSpitAnimation() : EnemyProjectileFrameAnimation<BotwoonSpitSegment, NoAnimationCue>(BotwoonAnimations.SpitFlight, EnemyProjectileCueRunners.None<BotwoonSpitSegment>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<BotwoonSpitSegment, EnemyProjectileFrameId, NoAnimationCue> Segments =>
        BotwoonAnimations.SpitSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<BotwoonSpitSegment, Animation<BotwoonSpitSegment>> StartingAt =>
        BotwoonAnimations.SpitStartingAt;
}
