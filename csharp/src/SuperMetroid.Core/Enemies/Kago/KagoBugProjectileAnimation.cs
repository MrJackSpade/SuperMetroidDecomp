using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago bug's typed animation.</summary>
/// <param name="segment">The segment the animation starts at.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class KagoBugProjectileAnimation(KagoBugProjectileSegment segment, IEnemyProjectileCues<KagoBugProjectileCue> cues)
    : EnemyProjectileFrameAnimation<KagoBugProjectileSegment, KagoBugProjectileCue>(KagoBugProjectileAnimations.StartingWith(segment), cues)
{
    /// <inheritdoc/>
    private protected override AnimationSegments<KagoBugProjectileSegment, EnemyProjectileFrameId, KagoBugProjectileCue> Segments => KagoBugProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<KagoBugProjectileSegment, Animation<KagoBugProjectileSegment>> StartingAt => KagoBugProjectileAnimations.StartingAt;
}
