using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Alcoon fireball's typed animation.</summary>
internal sealed class AlcoonFireballAnimation(AlcoonFireballSegment segment)
    : EnemyProjectileFrameAnimation<AlcoonFireballSegment, EnemyProjectileCommonCue>(segment, AlcoonFireballAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<AlcoonFireballSegment>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<AlcoonFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => AlcoonFireballAnimations.Segments;
}
