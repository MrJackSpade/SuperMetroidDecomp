using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago bug's typed animation.</summary>
internal sealed class KagoBugProjectileAnimation(KagoBugProjectileSegment program)
    : EnemyProjectileFrameAnimation<KagoBugProjectileSegment, KagoBugProjectileCue>(KagoBugProjectileAnimations.StartingWith(program))
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KagoBugProjectileSegment, EnemyProjectileFrameId, KagoBugProjectileCue> Segments => KagoBugProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KagoBugProjectileSegment, Animation<KagoBugProjectileSegment>> StartingAt => KagoBugProjectileAnimations.StartingAt;
}
