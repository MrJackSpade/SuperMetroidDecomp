using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>Cacatac spike's typed animation.</summary>
internal sealed class CacatacProjectileAnimation(CacatacProjectileSegment program)
    : EnemyProjectileFrameAnimation<CacatacProjectileSegment, EnemyProjectileCommonCue>(CacatacProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<CacatacProjectileSegment, EnemyProjectileFrameId, EnemyProjectileCommonCue> Segments => CacatacProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<CacatacProjectileSegment, Animation<CacatacProjectileSegment>> StartingAt => CacatacProjectileAnimations.StartingAt;
}
