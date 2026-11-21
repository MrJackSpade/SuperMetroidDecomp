using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Nuclear Waffle body's typed animation.</summary>
internal sealed class NuclearWaffleProjectileAnimation(NuclearWaffleProjectileSegment segment)
    : EnemyProjectileFrameAnimation<NuclearWaffleProjectileSegment, EnemyProjectileCommonCue>(segment, NuclearWaffleProjectileAnimations.StartingAt[segment], EnemyProjectileCueRunners.Common<NuclearWaffleProjectileSegment>())
{
    /// <inheritdoc/>
    private protected override AnimationSegments<NuclearWaffleProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => NuclearWaffleProjectileAnimations.Segments;
}
