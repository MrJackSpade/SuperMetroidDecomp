using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The plays a cinematic family's animations are written in (#2196, #2358): a still picture, one run of a segment, and the
/// phases that play a segment whole, once or a counted number of times. Each phase plays every beat of its segment, its
/// length read from the family's segments, so no animation counts its beats or names a beat by its place.
/// </summary>
/// <typeparam name="TAnimation">The family's animation identity.</typeparam>
/// <typeparam name="TFrame">What the family's actors show.</typeparam>
/// <typeparam name="TCue">The cues the family's animations publish.</typeparam>
/// <param name="segments">The family's segments, which the plays read their lengths from.</param>
internal sealed class CinematicPlays<TAnimation, TFrame, TCue>(AnimationSegments<TAnimation, TFrame, TCue> segments)
    where TAnimation : struct
    where TFrame : struct
    where TCue : notnull
{
    /// <summary>The family's segments.</summary>
    private readonly AnimationSegments<TAnimation, TFrame, TCue> segments = Ensure.NotNull(segments);

    /// <summary>The animation of a still picture: <paramref name="animation"/>'s one held frame.</summary>
    /// <exception cref="ArgumentException"><paramref name="animation"/>'s segment is not one held frame.</exception>
    internal Animation<TAnimation> Still(TAnimation animation) => segments.Beats(animation) is [AnimationBeat<TFrame, TCue>.HeldFrame]
        ? new([segments.Whole(animation)], AnimationEnd.Holds)
        : throw new ArgumentException($"Segment {animation} is not a still picture's one held frame.", nameof(animation));

    /// <summary>The animation that plays every beat of <paramref name="animation"/> once, then ends as <paramref name="end"/> says.</summary>
    internal Animation<TAnimation> Through(TAnimation animation, AnimationEnd end) => new([segments.Whole(animation)], end);

    /// <summary>The animation that plays every beat of <paramref name="animation"/> over and over.</summary>
    internal Animation<TAnimation> Looping(TAnimation animation) => Animation<TAnimation>.Looping([segments.Whole(animation)]);

    /// <summary>
    /// The animation that plays every beat of <paramref name="lead"/> once, then every beat of <paramref name="cycle"/> over
    /// and over (#2414).
    /// </summary>
    internal Animation<TAnimation> LeadingInto(TAnimation lead, TAnimation cycle) =>
        Animation<TAnimation>.LeadingInto([segments.Whole(lead)], [segments.Whole(cycle)]);

    /// <summary>The phase that plays every beat of <paramref name="segment"/> once.</summary>
    internal AnimationPhase<TAnimation> Whole(TAnimation segment) => segments.Whole(segment);

    /// <summary>The phase that plays every beat of <paramref name="segment"/> <paramref name="plays"/> times, counting its plays.</summary>
    internal AnimationPhase<TAnimation> Repeated(TAnimation segment, int plays) => segments.Repeated(segment, plays);
}
