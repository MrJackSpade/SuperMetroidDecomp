using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>A Ridley fireball's or afterburn's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
internal sealed class RidleyFireballAnimation(IEnemyProjectileCues<RidleyFireballCue> owner, RidleyFireballSegment segment)
    : EnemyProjectileFrameAnimation<RidleyFireballSegment, RidleyFireballCue>(RidleyFireballAnimations.StartingWith(segment), EnemyProjectileCueRunner<RidleyFireballSegment, RidleyFireballCue>.By(owner))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<RidleyFireballSegment, EnemyProjectileFrameId, RidleyFireballCue> Segments =>
        RidleyFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<RidleyFireballSegment, Animation<RidleyFireballSegment>> StartingAt =>
        RidleyFireballAnimations.StartingAt;
}
