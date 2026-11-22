using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// Enemy death and drop's typed animation: a drop's or the respawn pause's, which start at their segments, or a death
/// explosion's, started by the explosion (<see cref="EnemyDeathAndDropAnimations.Explosions"/>, #2061).
/// </summary>
internal sealed partial class EnemyDeathAndDropAnimation
    : EnemyProjectileFrameAnimation<EnemyDeathAndDropSegment, EnemyDeathAndDropCue>
{
    /// <summary>Creates an <see cref="EnemyDeathAndDropAnimation"/>.</summary>
    /// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
    /// <param name="animation">The animation it plays.</param>
    public EnemyDeathAndDropAnimation(IEnemyProjectileBranchingCues<EnemyDeathAndDropSegment, EnemyDeathAndDropCue> owner, Animation<EnemyDeathAndDropSegment> animation)
        : base(animation.Phases[0].Segment, animation, EnemyProjectileCueRunner<EnemyDeathAndDropSegment, EnemyDeathAndDropCue>.By(owner))
    {
    }

    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal EnemyDeathAndDropAnimation(IEnemyProjectileBranchingCues<EnemyDeathAndDropSegment, EnemyDeathAndDropCue> owner, EnemyDeathAndDropSegment segment)
        : this(owner, EnemyDeathAndDropAnimations.StartingAt[segment])
    {
    }

    /// <inheritdoc/>
    private protected override IUnpausedAnimationPlay<EnemyDeathAndDropSegment, EnemyProjectileFrameId, EnemyDeathAndDropCue> Segments => EnemyDeathAndDropAnimations.Segments;
}
