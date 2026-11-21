using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Kago;

/// <summary>Kago bug's typed animation.</summary>
internal sealed partial class KagoBugProjectileAnimation
    : EnemyProjectileFrameAnimation<KagoBugProjectileSegment, KagoBugProjectileCue>
{
    /// <summary>Creates a <see cref="KagoBugProjectileAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public KagoBugProjectileAnimation(IEnemyProjectileCues<KagoBugProjectileCue> owner, KagoBugProjectileSegment segment)
        : base(segment, KagoBugProjectileAnimations.StartingAt[segment], EnemyProjectileCueRunner<KagoBugProjectileSegment, KagoBugProjectileCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<KagoBugProjectileSegment, EnemyProjectileFrameId, KagoBugProjectileCue> Segments => KagoBugProjectileAnimations.Segments;
}
