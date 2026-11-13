using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Nuclear Waffle body's typed animation.</summary>
internal sealed class NuclearWaffleProjectileAnimation(NuclearWaffleProjectileSegment segment)
    : EnemyProjectileFrameAnimation<NuclearWaffleProjectileSegment, EnemyProjectileCommonCue>(NuclearWaffleProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<NuclearWaffleProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => NuclearWaffleProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<NuclearWaffleProjectileSegment, Animation<NuclearWaffleProjectileSegment>> StartingAt => NuclearWaffleProjectileAnimations.StartingAt;
}
