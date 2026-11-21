using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>Skree/Metaree particle's typed animation.</summary>
internal sealed partial class SkreeMetareeParticleAnimation
    : EnemyProjectileFrameAnimation<SkreeMetareeParticleSegment, EnemyProjectileCommonCue>
{
    /// <summary>Creates a <see cref="SkreeMetareeParticleAnimation"/>.</summary>
    /// <param name="segment">segment</param>
    public SkreeMetareeParticleAnimation(SkreeMetareeParticleSegment segment)
        : base(segment, SkreeMetareeParticleAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<SkreeMetareeParticleSegment>())
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<SkreeMetareeParticleSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => SkreeMetareeParticleAnimations.Segments;
}
