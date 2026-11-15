using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>A Ridley fireball's or afterburn's typed animation.</summary>
internal sealed class RidleyFireballAnimation(RidleyFireballSegment segment)
    : EnemyProjectileFrameAnimation<RidleyFireballSegment, RidleyFireballCue>(RidleyFireballAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<RidleyFireballSegment, EnemyProjectileFrameId, RidleyFireballCue> Segments =>
        RidleyFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<RidleyFireballSegment, Animation<RidleyFireballSegment>> StartingAt =>
        RidleyFireballAnimations.StartingAt;
}
