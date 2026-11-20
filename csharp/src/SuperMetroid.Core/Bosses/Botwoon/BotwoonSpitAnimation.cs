using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's spit's typed animation (#2400): its glowing frames, cycled for as long as it flies.</summary>
internal sealed class BotwoonSpitAnimation() : EnemyProjectileSilentFrameAnimation<BotwoonSpitSegment>(BotwoonAnimations.SpitFlight)
{
    /// <inheritdoc/>
    private protected override UnpausedAnimationSegments<BotwoonSpitSegment, EnemyProjectileFrameId, NoAnimationCue> Segments =>
        BotwoonAnimations.SpitSegments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<BotwoonSpitSegment, Animation<BotwoonSpitSegment>>> StartingAt =>
        BotwoonAnimations.SpitStartingAt;
}
