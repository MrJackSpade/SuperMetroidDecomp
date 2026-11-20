using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Alcoon fireball's typed animation.</summary>
internal sealed class AlcoonFireballAnimation(AlcoonFireballSegment segment)
    : EnemyProjectileFrameAnimation<AlcoonFireballSegment, EnemyProjectileCommonCue>(AlcoonFireballAnimations.StartingWith(segment), EnemyProjectileCueRunners.Common<AlcoonFireballSegment>())
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<AlcoonFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => AlcoonFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<AlcoonFireballSegment, Animation<AlcoonFireballSegment>>> StartingAt => AlcoonFireballAnimations.StartingAt;
}
