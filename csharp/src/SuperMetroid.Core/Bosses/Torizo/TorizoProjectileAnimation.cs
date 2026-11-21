using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo projectile's typed animation: one starting at a segment, or one its owner starts by its own name, as a forming
/// drool's (#2411).
/// </summary>
internal sealed partial class TorizoProjectileAnimation
    : EnemyProjectileFrameAnimation<TorizoProjectileSegment, TorizoProjectileCue>
{
    /// <summary>The family's cue owner, which carries out the events the animation raises (#2571).</summary>
    private readonly IEnemyProjectileBranchingCues<TorizoProjectileSegment, TorizoProjectileCue> owner;

    /// <summary>Creates a <see cref="TorizoProjectileAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="animation">The animation the projectile plays from its start.</param>
    public TorizoProjectileAnimation(IEnemyProjectileBranchingCues<TorizoProjectileSegment, TorizoProjectileCue> owner, Animation<TorizoProjectileSegment> animation)
        : base(animation.Phases[0].Segment, animation, EnemyProjectileCueRunner<TorizoProjectileSegment, TorizoProjectileCue>.By(owner))
    {
        this.owner = owner;
    }

    /// <summary>A projectile animation starting at <paramref name="segment"/>.</summary>
    /// <param name="owner">The family's cue owner.</param>
    /// <param name="segment">The segment the animation starts at.</param>
    internal TorizoProjectileAnimation(IEnemyProjectileBranchingCues<TorizoProjectileSegment, TorizoProjectileCue> owner, TorizoProjectileSegment segment)
        : this(owner, TorizoProjectileAnimations.StartingAt[segment])
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<TorizoProjectileSegment, EnemyProjectileFrameId, TorizoProjectileCue> Segments => TorizoProjectileAnimations.Segments;

}
