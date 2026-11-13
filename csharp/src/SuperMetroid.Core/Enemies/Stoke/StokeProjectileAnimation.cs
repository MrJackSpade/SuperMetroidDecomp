using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>Stoke projectile's typed animation.</summary>
internal sealed class StokeProjectileAnimation(StokeProjectileSegment segment)
    : EnemyProjectileFrameAnimation<StokeProjectileSegment, EnemyProjectileCommonCue>(StokeProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<StokeProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => StokeProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<StokeProjectileSegment, Animation<StokeProjectileSegment>> StartingAt => StokeProjectileAnimations.StartingAt;
}
