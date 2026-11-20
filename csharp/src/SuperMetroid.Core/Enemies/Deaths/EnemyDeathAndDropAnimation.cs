using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// Enemy death and drop's typed animation: a drop's or the respawn pause's, which start at their segments, or a death
/// explosion's, started by the explosion (<see cref="EnemyDeathAndDropAnimations.Explosions"/>, #2061).
/// </summary>
/// <param name="animation">The animation.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class EnemyDeathAndDropAnimation(Animation<EnemyDeathAndDropSegment> animation, IEnemyProjectileBranchingCues<EnemyDeathAndDropSegment, EnemyDeathAndDropCue> cues)
    : EnemyProjectileFrameAnimation<EnemyDeathAndDropSegment, EnemyDeathAndDropCue>(animation, cues)
{
    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal EnemyDeathAndDropAnimation(EnemyDeathAndDropSegment segment, IEnemyProjectileBranchingCues<EnemyDeathAndDropSegment, EnemyDeathAndDropCue> cues)
        : this(EnemyDeathAndDropAnimations.StartingWith(segment), cues)
    {
    }

    /// <summary>The segment <paramref name="started"/> starts at: a counted explosion's burst, or the segment the rest are keyed by.</summary>
    private protected override EnemyDeathAndDropSegment SegmentStartedAt(Animation<EnemyDeathAndDropSegment> started) =>
        StartingAt.Values.Any(keyed => ReferenceEquals(keyed, started)) ? base.SegmentStartedAt(started) : started.Phases[0].Segment;

    /// <inheritdoc/>
    private protected override AnimationSegments<EnemyDeathAndDropSegment, EnemyProjectileFrameId, EnemyDeathAndDropCue> Segments => EnemyDeathAndDropAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EnemyDeathAndDropSegment, Animation<EnemyDeathAndDropSegment>> StartingAt => EnemyDeathAndDropAnimations.StartingAt;
}
