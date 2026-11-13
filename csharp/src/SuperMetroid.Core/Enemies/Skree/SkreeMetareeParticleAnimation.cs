using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Skree/Metaree particle's typed animation.</summary>
internal sealed class SkreeMetareeParticleAnimation(SkreeMetareeParticleSegment segment)
    : EnemyProjectileFrameAnimation<SkreeMetareeParticleSegment, EnemyProjectileCommonCue>(SkreeMetareeParticleAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<SkreeMetareeParticleSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => SkreeMetareeParticleAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<SkreeMetareeParticleSegment, Animation<SkreeMetareeParticleSegment>> StartingAt => SkreeMetareeParticleAnimations.StartingAt;
}
