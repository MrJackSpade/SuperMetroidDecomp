using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>A Ridley fireball's or afterburn's typed animation.</summary>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class RidleyFireballAnimation(RidleyFireballSegment segment, IEnemyProjectileCues<RidleyFireballCue> cues)
    : EnemyProjectileFrameAnimation<RidleyFireballSegment, RidleyFireballCue>(RidleyFireballAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<RidleyFireballSegment, EnemyProjectileFrameId, RidleyFireballCue> Segments =>
        RidleyFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<RidleyFireballSegment, Animation<RidleyFireballSegment>> StartingAt =>
        RidleyFireballAnimations.StartingAt;
}
