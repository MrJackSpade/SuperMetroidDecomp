using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>A Ridley fireball's or afterburn's typed animation.</summary>
internal sealed partial class RidleyFireballAnimation
    : EnemyProjectileFrameAnimation<RidleyFireballSegment, RidleyFireballCue>
{
    /// <summary>Creates a <see cref="RidleyFireballAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="segment">Where the animation starts.</param>
    public RidleyFireballAnimation(IEnemyProjectileCues<RidleyFireballCue> owner, RidleyFireballSegment segment)
        : base(segment, RidleyFireballAnimations.StartingAt[segment], EnemyProjectileCueRunner<RidleyFireballSegment, RidleyFireballCue>.By(owner))
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<RidleyFireballSegment, EnemyProjectileFrameId, RidleyFireballCue> Segments =>
        RidleyFireballAnimations.Segments;
}
