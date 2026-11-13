using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>Enemy death and drop's typed animation.</summary>
internal sealed class EnemyDeathProjectileAnimation(EnemyDeathProjectileSegment segment)
    : EnemyProjectileFrameAnimation<EnemyDeathProjectileSegment, EnemyDeathProjectileCue>(EnemyDeathProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<EnemyDeathProjectileSegment, EnemyProjectileFrameId, EnemyDeathProjectileCue> Segments => EnemyDeathProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EnemyDeathProjectileSegment, Animation<EnemyDeathProjectileSegment>> StartingAt => EnemyDeathProjectileAnimations.StartingAt;
}
