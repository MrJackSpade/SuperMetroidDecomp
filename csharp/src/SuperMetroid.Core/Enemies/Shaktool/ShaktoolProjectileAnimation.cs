using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Shaktool.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Shaktool attack circle's typed animation.</summary>
internal sealed class ShaktoolProjectileAnimation(ShaktoolProjectileProgram program)
    : EnemyProjectileProgramAnimation<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<ShaktoolProjectileProgram, ShaktoolProjectileFrame, EnemyProjectileCommonCue> Programs => ShaktoolProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(ShaktoolProjectileFrame frame) =>
        ShaktoolProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
