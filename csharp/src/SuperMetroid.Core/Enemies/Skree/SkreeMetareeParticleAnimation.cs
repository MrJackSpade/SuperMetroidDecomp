using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Skree/Metaree particle's typed animation.</summary>
internal sealed class SkreeMetareeParticleAnimation(SkreeMetareeParticleProgram program)
    : EnemyProjectileProgramAnimation<SkreeMetareeParticleProgram, SkreeMetareeParticleFrame, EnemyProjectileCommonCue>(program)
{
    /// <inheritdoc/>
    private protected override AnimationProgramSet<SkreeMetareeParticleProgram, SkreeMetareeParticleFrame, EnemyProjectileCommonCue> Programs => SkreeMetareeParticleAnimations.Programs;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(SkreeMetareeParticleFrame frame) =>
        SkreeMetareeParticleFrameDefinitions.ShownProjectileFrames[frame];
}
