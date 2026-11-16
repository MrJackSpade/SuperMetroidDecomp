using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// Enemy death and drop's typed animation: a drop's or the respawn tail's, which start at their segments, or a death
/// explosion's, started by the explosion (<see cref="EnemyDeathProjectileAnimations.Explosions"/>, #2061).
/// </summary>
internal sealed class EnemyDeathProjectileAnimation(Animation<EnemyDeathProjectileSegment> animation)
    : EnemyProjectileFrameAnimation<EnemyDeathProjectileSegment, EnemyDeathProjectileCue>(animation)
{
    /// <summary>The animation that starts at <paramref name="segment"/>.</summary>
    internal EnemyDeathProjectileAnimation(EnemyDeathProjectileSegment segment)
        : this(EnemyDeathProjectileAnimations.StartingWith(segment))
    {
    }

    /// <summary>The segment <paramref name="started"/> starts at: a counted explosion's burst, or the segment the rest are keyed by.</summary>
    private protected override EnemyDeathProjectileSegment SegmentStartedAt(Animation<EnemyDeathProjectileSegment> started) =>
        StartingAt.Values.Any(keyed => ReferenceEquals(keyed, started)) ? base.SegmentStartedAt(started) : started.Phases[0].Segment;

    /// <inheritdoc/>
    private protected override AnimationSegments<EnemyDeathProjectileSegment, EnemyProjectileFrameId, EnemyDeathProjectileCue> Segments => EnemyDeathProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<EnemyDeathProjectileSegment, Animation<EnemyDeathProjectileSegment>> StartingAt => EnemyDeathProjectileAnimations.StartingAt;
}
