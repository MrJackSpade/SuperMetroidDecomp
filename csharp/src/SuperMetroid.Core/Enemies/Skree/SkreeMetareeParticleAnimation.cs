using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Skree/Metaree particle's typed animation.</summary>
internal sealed class SkreeMetareeParticleAnimation(SkreeMetareeParticleSegment program)
    : EnemyProjectileFrameAnimation<SkreeMetareeParticleSegment, EnemyProjectileCommonCue>(SkreeMetareeParticleAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SkreeMetareeParticleSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => SkreeMetareeParticleAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SkreeMetareeParticleSegment, Animation<SkreeMetareeParticleSegment>> StartingAt => SkreeMetareeParticleAnimations.StartingAt;
}
