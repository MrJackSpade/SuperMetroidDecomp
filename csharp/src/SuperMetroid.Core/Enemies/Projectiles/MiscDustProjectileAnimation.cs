using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Misc dust's typed animation.</summary>
internal sealed class MiscDustProjectileAnimation(MiscDustProjectileProgram program)
    : EnemyProjectileProgramAnimation<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<MiscDustProjectileProgram, MiscDustProjectileFrame, EnemyProjectileCommonCue> Programs => MiscDustProjectileAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(MiscDustProjectileFrame frame) =>
        MiscDustProjectileFrameDefinitions.ShownProjectileFrames[frame];
}
