using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Alcoon fireball's typed animation.</summary>
internal sealed class AlcoonFireballAnimation(AlcoonFireballProgram program)
    : EnemyProjectileFrameAnimation<AlcoonFireballProgram, EnemyProjectileCommonCue>(AlcoonFireballAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<AlcoonFireballProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => AlcoonFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<AlcoonFireballProgram, Animation<AlcoonFireballProgram>> StartingAt => AlcoonFireballAnimations.StartingAt;
}
