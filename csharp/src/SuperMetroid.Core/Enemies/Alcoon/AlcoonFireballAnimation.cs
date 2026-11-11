using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Alcoon fireball's typed animation.</summary>
internal sealed class AlcoonFireballAnimation(AlcoonFireballProgram program)
    : EnemyProjectileTypedAnimation<AlcoonFireballProgram, AlcoonFireballFrame, EnemyProjectileCommonCue>(AlcoonFireballAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<AlcoonFireballProgram, AlcoonFireballFrame, EnemyProjectileCommonCue> Segments => AlcoonFireballAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<AlcoonFireballProgram, Animation<AlcoonFireballProgram>> StartingAt => AlcoonFireballAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(AlcoonFireballFrame frame) =>
        AlcoonFireballFrameDefinitions.ShownProjectileFrames[frame];
}
