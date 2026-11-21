using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A Draygon goop's or wall-turret shot's typed animation.</summary>
internal sealed partial class DraygonProjectileAnimation
    : EnemyProjectileFrameAnimation<DraygonProjectileSegment, DraygonProjectileCue>
{
    /// <summary>Creates a <see cref="DraygonProjectileAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public DraygonProjectileAnimation(IEnemyProjectileCues<DraygonProjectileCue> owner, DraygonProjectileSegment segment)
        : base(segment, DraygonProjectileAnimations.StartingAt[segment], EnemyProjectileCueRunner<DraygonProjectileSegment, DraygonProjectileCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<DraygonProjectileSegment, EnemyProjectileFrameId, DraygonProjectileCue> Segments =>
        DraygonProjectileAnimations.Segments;
}
