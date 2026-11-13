using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Skree/Metaree particle's typed animation.</summary>
internal sealed class SkreeMetareeParticleAnimation(SkreeMetareeParticleProgram program)
    : EnemyProjectileFrameAnimation<SkreeMetareeParticleProgram, EnemyProjectileCommonCue>(SkreeMetareeParticleAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SkreeMetareeParticleProgram, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => SkreeMetareeParticleAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SkreeMetareeParticleProgram, Animation<SkreeMetareeParticleProgram>> StartingAt => SkreeMetareeParticleAnimations.StartingAt;
}
