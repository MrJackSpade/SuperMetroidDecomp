using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Yapping Maw body link's typed animation.</summary>
internal sealed class YappingMawBodyProjectileAnimation(YappingMawBodyProjectileSegment segment)
    : EnemyProjectileFrameAnimation<YappingMawBodyProjectileSegment, EnemyProjectileCommonCue>(segment, YappingMawBodyProjectileAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<YappingMawBodyProjectileSegment>())
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<YappingMawBodyProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => YappingMawBodyProjectileAnimations.Segments;
}
