using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo projectile's typed animation: one starting at a segment, or one its owner starts by its own name, as a forming
/// drool's (#2411).
/// </summary>
/// <param name="animation">The animation the projectile plays from its start.</param>
internal sealed class TorizoProjectileAnimation(Animation<TorizoProjectileSegment> animation)
    : EnemyProjectileFrameAnimation<TorizoProjectileSegment, TorizoProjectileCue>(animation)
{
    /// <summary>A projectile animation starting at <paramref name="segment"/>.</summary>
    /// <param name="segment">The segment the animation starts at.</param>
    internal TorizoProjectileAnimation(TorizoProjectileSegment segment)
        : this(TorizoProjectileAnimations.StartingWith(segment))
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<TorizoProjectileSegment, EnemyProjectileFrameId, TorizoProjectileCue> Segments => TorizoProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>> StartingAt => TorizoProjectileAnimations.StartingAt;

    /// <summary>The segment <paramref name="started"/> starts at: its key in <see cref="StartingAt"/>, or a forming drool's first segment.</summary>
    private protected override TorizoProjectileSegment SegmentStartedAt(Animation<TorizoProjectileSegment> started) =>
        StartingAt.Values.Any(keyed => ReferenceEquals(keyed, started)) ? base.SegmentStartedAt(started) : Ensure.NotNull(started).Phases[0].Segment;
}
