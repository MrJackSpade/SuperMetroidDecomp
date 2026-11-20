using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A typed animation of the Torizos' drool, swipes, sonic booms, landing dust and statue fragments: one starting at a segment, or one its owner starts by its own name, as a forming
/// drool's (#2411).
/// </summary>
/// <param name="animation">The animation the projectile plays from its start.</param>
/// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
internal sealed class TorizoProjectileAnimation(Animation<TorizoProjectileSegment> animation, IEnemyProjectileBranchingCues<TorizoProjectileSegment, TorizoFamilyCue<TorizoFallCueKind>> cues)
    : EnemyProjectileFrameAnimation<TorizoProjectileSegment, TorizoFamilyCue<TorizoFallCueKind>>(animation, cues)
{
    /// <summary>A projectile animation starting at <paramref name="segment"/>.</summary>
    /// <param name="segment">The segment the animation starts at.</param>
    /// <param name="cues">The behaviour that started the animation, which owns its cues.</param>
    internal TorizoProjectileAnimation(TorizoProjectileSegment segment, IEnemyProjectileBranchingCues<TorizoProjectileSegment, TorizoFamilyCue<TorizoFallCueKind>> cues)
        : this(TorizoProjectileAnimations.StartingWith(segment), cues)
    {
    }

    /// <inheritdoc/>
    private protected override AnimationSegments<TorizoProjectileSegment, EnemyProjectileFrameId, TorizoFamilyCue<TorizoFallCueKind>> Segments => TorizoProjectileAnimations.Segments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<TorizoProjectileSegment, Animation<TorizoProjectileSegment>> StartingAt => TorizoProjectileAnimations.StartingAt;

    /// <summary>The segment <paramref name="started"/> starts at: its key in <see cref="StartingAt"/>, or a forming drool's first segment.</summary>
    private protected override TorizoProjectileSegment SegmentStartedAt(Animation<TorizoProjectileSegment> started) =>
        StartingAt.Values.Any(keyed => ReferenceEquals(keyed, started)) ? base.SegmentStartedAt(started) : Ensure.NotNull(started).Phases[0].Segment;
}
