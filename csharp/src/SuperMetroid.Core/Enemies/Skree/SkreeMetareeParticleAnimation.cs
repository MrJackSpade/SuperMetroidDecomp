using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Skree/Metaree particle's typed animation.</summary>
internal sealed class SkreeMetareeParticleAnimation(SkreeMetareeParticleProgram program)
    : EnemyProjectileTypedAnimation<SkreeMetareeParticleProgram, SkreeMetareeParticleFrame, EnemyProjectileCommonCue>(SkreeMetareeParticleAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SkreeMetareeParticleProgram, SkreeMetareeParticleFrame, EnemyProjectileCommonCue> Segments => SkreeMetareeParticleAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SkreeMetareeParticleProgram, Animation<SkreeMetareeParticleProgram>> StartingAt => SkreeMetareeParticleAnimations.StartingAt;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(SkreeMetareeParticleFrame frame) =>
        SkreeMetareeParticleFrameDefinitions.ShownProjectileFrames[frame];
}
