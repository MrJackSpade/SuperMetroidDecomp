using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Shaktool attack circle's typed animation.</summary>
internal sealed class ShaktoolProjectileAnimation(ShaktoolProjectileSegment segment)
    : EnemyProjectileFrameAnimation<ShaktoolProjectileSegment, EnemyProjectileCommonCue>(ShaktoolProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<ShaktoolProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => ShaktoolProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<ShaktoolProjectileSegment, Animation<ShaktoolProjectileSegment>> StartingAt => ShaktoolProjectileAnimations.StartingAt;
}
