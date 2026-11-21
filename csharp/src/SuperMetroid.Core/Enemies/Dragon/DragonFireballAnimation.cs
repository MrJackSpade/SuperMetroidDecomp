using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>Dragon fireball's typed animation.</summary>
internal sealed class DragonFireballAnimation(DragonFireballSegment segment)
    : EnemyProjectileFrameAnimation<DragonFireballSegment, EnemyProjectileCommonCue>(segment, DragonFireballAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<DragonFireballSegment>())
{
    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<DragonFireballSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => DragonFireballAnimations.Segments;
}
