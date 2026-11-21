using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Cacatac spike's typed animation.</summary>
internal sealed partial class CacatacProjectileAnimation
    : EnemyProjectileFrameAnimation<CacatacSpikeDirection, EnemyProjectileCommonCue>
{
    /// <summary>Creates a <see cref="CacatacProjectileAnimation"/>.</summary>
    /// <param name="direction">direction</param>
    public CacatacProjectileAnimation(CacatacSpikeDirection direction)
        : base(direction, CacatacProjectileAnimations.StartingAt[direction], EnemyProjectileCueRunners.Common<CacatacSpikeDirection>())
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<CacatacSpikeDirection, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => CacatacProjectileAnimations.Segments;
}
