using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// One family of cinematic actors' animations (#641, #1600, #2197): the segments they play, the animation each actor
/// starts, and the cues they publish, which only that family's owner acts on. A family names its own segments, frames
/// and cues, so no owner is handed a cue another family's animations publish.
/// </summary>
/// <typeparam name="TAnimation">The family's animation identity: each segment it plays, and each animation an actor starts.</typeparam>
/// <typeparam name="TFrame">What the family's actors show.</typeparam>
/// <typeparam name="TCue">The cues the family's animations publish to their owner.</typeparam>
internal sealed class CinematicAnimations<TAnimation, TFrame, TCue>
    where TAnimation : struct, Enum
    where TFrame : struct
    where TCue : notnull
{
    /// <summary>The animation each actor of the family starts, by the animation it is named for.</summary>
    private readonly Dictionary<TAnimation, Animation<TAnimation>> starts;

    /// <summary>
    /// Creates the family's animations: <paramref name="segments"/> are the beats each segment plays, and
    /// <paramref name="startingAt"/> writes, in the family's plays, the animation an actor starting each of them plays (#2358):
    /// each phase a whole segment, its length read from the segment's beats.
    /// </summary>
    /// <exception cref="ArgumentException">An animation plays a segment the family does not hold.</exception>
    internal CinematicAnimations(IReadOnlyDictionary<TAnimation, AnimationBeat<TFrame, TCue>[]> segments,
        Func<CinematicPlays<TAnimation, TFrame, TCue>, IReadOnlyDictionary<TAnimation, Animation<TAnimation>>> startingAt)
    {
        Ensure.NotNull(segments);
        Ensure.NotNull(startingAt);
        Segments = new AnimationSegments<TAnimation, TFrame, TCue>(segments, segments.Keys);
        IReadOnlyDictionary<TAnimation, Animation<TAnimation>> starting = startingAt(new CinematicPlays<TAnimation, TFrame, TCue>(Segments));
        foreach ((TAnimation start, Animation<TAnimation> animation) in starting)
        {
            foreach (AnimationPhase<TAnimation> phase in animation.Phases)
            {
                if (!segments.ContainsKey(phase.Segment))
                    throw new ArgumentException($"Animation {start} plays segment {phase.Segment}, which the family does not hold.", nameof(startingAt));
            }
        }
        starts = starting.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    /// <summary>The segments the family's animations play.</summary>
    internal AnimationSegments<TAnimation, TFrame, TCue> Segments { get; }

    /// <summary>The animations an actor of the family can start, each by the animation it is named for.</summary>
    internal IReadOnlyCollection<TAnimation> Starts => starts.Keys;

    /// <summary>The animation an actor starting <paramref name="animation"/> plays.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No actor of the family starts <paramref name="animation"/>.</exception>
    internal Animation<TAnimation> StartingWith(TAnimation animation) => starts.TryGetValue(animation, out Animation<TAnimation>? started)
        ? started
        : throw new ArgumentOutOfRangeException(nameof(animation), animation, "No actor of this cinematic family starts this animation.");
}
