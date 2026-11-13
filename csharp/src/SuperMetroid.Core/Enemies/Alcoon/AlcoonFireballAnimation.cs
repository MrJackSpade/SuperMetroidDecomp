using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Alcoon fireball's typed animation.</summary>
internal sealed class AlcoonFireballAnimation(AlcoonFireballSegment program)
    : EnemyProjectileFrameAnimation<AlcoonFireballSegment, EnemyProjectileCommonCue>(AlcoonFireballAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<AlcoonFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => AlcoonFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<AlcoonFireballSegment, Animation<AlcoonFireballSegment>> StartingAt => AlcoonFireballAnimations.StartingAt;
}
