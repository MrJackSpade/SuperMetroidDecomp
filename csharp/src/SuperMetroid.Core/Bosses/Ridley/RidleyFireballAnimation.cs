using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>A Ridley fireball's or afterburn's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class RidleyFireballAnimation(IEnemyProjectileCues<RidleyFireballCue> owner, RidleyFireballSegment segment)
    : EnemyProjectileFrameAnimation<RidleyFireballSegment, RidleyFireballCue>(RidleyFireballAnimations.StartingWith(segment), EnemyProjectileCueRunner<RidleyFireballSegment, RidleyFireballCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override UnpausedAnimationSegments<RidleyFireballSegment, EnemyProjectileFrameId, RidleyFireballCue> Segments =>
        RidleyFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<RidleyFireballSegment, Animation<RidleyFireballSegment>>> StartingAt =>
        RidleyFireballAnimations.StartingAt;
}
