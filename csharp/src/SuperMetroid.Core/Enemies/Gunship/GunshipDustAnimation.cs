using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>A gunship liftoff dust cloud's typed animation.</summary>
internal sealed partial class GunshipDustAnimation
    : EnemyProjectileFrameAnimation<GunshipDustSegment, GunshipDustCue>
{
    /// <summary>Creates a <see cref="GunshipDustAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public GunshipDustAnimation(IEnemyProjectileCues<GunshipDustCue> owner, GunshipDustSegment segment)
        : base(segment, GunshipDustAnimations.StartingWith(segment), EnemyProjectileCueRunner<GunshipDustSegment, GunshipDustCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<GunshipDustSegment, EnemyProjectileFrameId, GunshipDustCue> Segments => GunshipDustAnimations.Segments;
}
