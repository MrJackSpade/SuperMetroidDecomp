using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago bug's typed animation.</summary>
internal sealed class KagoBugProjectileAnimation(KagoBugProjectileSegment segment)
    : EnemyProjectileFrameAnimation<KagoBugProjectileSegment, KagoBugProjectileCue>(KagoBugProjectileAnimations.StartingWith(segment))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KagoBugProjectileSegment, EnemyProjectileFrameId, KagoBugProjectileCue> Segments => KagoBugProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KagoBugProjectileSegment, Animation<KagoBugProjectileSegment>> StartingAt => KagoBugProjectileAnimations.StartingAt;
}
