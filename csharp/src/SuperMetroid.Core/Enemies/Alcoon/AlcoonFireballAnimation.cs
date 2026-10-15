using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>Alcoon fireball's typed animation.</summary>
internal sealed class AlcoonFireballAnimation(AlcoonFireballProgram program)
    : EnemyProjectileProgramAnimation<AlcoonFireballProgram, AlcoonFireballFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<AlcoonFireballProgram, AlcoonFireballFrame, EnemyProjectileCommonCue> Programs => AlcoonFireballAnimations.Programs;

    /// <inheritdoc/>
    private protected override ushort PresentationKey(AlcoonFireballFrame frame) => AlcoonFireballFrameDefinitions.PresentationKey(frame);
}
