using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Cacatac spike's typed animation.</summary>
internal sealed class CacatacProjectileAnimation(CacatacSpikeDirection direction)
    : EnemyProjectileFrameAnimation<CacatacSpikeDirection, EnemyProjectileCommonCue>(CacatacProjectileAnimations.StartingWith(direction))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<CacatacSpikeDirection, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => CacatacProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<CacatacSpikeDirection, Animation<CacatacSpikeDirection>> StartingAt => CacatacProjectileAnimations.StartingAt;
}
