using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Cacatac spike's typed animation.</summary>
internal sealed class CacatacProjectileAnimation(CacatacSpikeDirection direction)
    : EnemyProjectileFrameAnimation<CacatacSpikeDirection, EnemyProjectileCommonCue>(direction, CacatacProjectileAnimations.StartingWith(direction), EnemyProjectileCueRunners.Common<CacatacSpikeDirection>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<CacatacSpikeDirection, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => CacatacProjectileAnimations.Segments;
}
