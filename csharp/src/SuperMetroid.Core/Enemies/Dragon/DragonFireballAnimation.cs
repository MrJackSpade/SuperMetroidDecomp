using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Dragon fireball's typed animation.</summary>
internal sealed class DragonFireballAnimation(DragonFireballProgram program)
    : EnemyProjectileProgramAnimation<DragonFireballProgram, DragonFireballFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<DragonFireballProgram, DragonFireballFrame, EnemyProjectileCommonCue> Programs => DragonFireballAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(DragonFireballFrame frame) =>
        DragonFireballFrameDefinitions.ShownProjectileFrames[frame];
}
