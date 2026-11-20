using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// Enemy death and drop's typed animation: a drop's or the respawn pause's, which start at their segments, or a death
/// explosion's, started by the explosion (<see cref="EnemyDeathAndDropAnimations.Explosions"/>, #2061).
/// </summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="animation">The animation it plays.</param>
internal sealed class EnemyDeathAndDropAnimation(IEnemyProjectileBranchingCues<EnemyDeathAndDropSegment, EnemyDeathAndDropCue> owner, Animation<EnemyDeathAndDropSegment> animation)
    : EnemyProjectileFrameAnimation<EnemyDeathAndDropSegment, EnemyDeathAndDropCue>(animation, EnemyProjectileCueRunner<EnemyDeathAndDropSegment, EnemyDeathAndDropCue>.By(owner))
{
    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal EnemyDeathAndDropAnimation(IEnemyProjectileBranchingCues<EnemyDeathAndDropSegment, EnemyDeathAndDropCue> owner, EnemyDeathAndDropSegment segment)
        : this(owner, EnemyDeathAndDropAnimations.StartingWith(segment))
    {
    }

    /// <summary>The segment <paramref name="started"/> starts at: a counted explosion's burst, or the segment the rest are keyed by.</summary>
    private protected override EnemyDeathAndDropSegment SegmentStartedAt(Animation<EnemyDeathAndDropSegment> started) =>
        StartingAt.Any(keyed => ReferenceEquals(keyed.Value, started)) ? base.SegmentStartedAt(started) : started.Phases[0].Segment;

    /// <inheritdoc/>
    private protected override AnimationSegments<EnemyDeathAndDropSegment, EnemyProjectileFrameId, EnemyDeathAndDropCue> Segments => EnemyDeathAndDropAnimations.Segments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<EnemyDeathAndDropSegment, Animation<EnemyDeathAndDropSegment>>> StartingAt => EnemyDeathAndDropAnimations.StartingAt;
}
