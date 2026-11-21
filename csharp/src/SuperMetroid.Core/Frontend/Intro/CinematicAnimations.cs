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
        : this(Ensure.NotNull(segments).ToDictionary(pair => pair.Key, pair => pair.Value.ToArray<IHidingAnimationBeat<TFrame, TCue>>()), startingAt)
    {
    }

    /// <summary>
    /// Creates the animations of a family whose actors hide for stretches (#2558): <paramref name="segments"/> are the beats
    /// each segment plays, hidden stretches among them, and <paramref name="startingAt"/> writes the animation an actor
    /// starting each of them plays, as the other constructor's does.
    /// </summary>
    /// <exception cref="ArgumentException">An animation plays a segment the family does not hold.</exception>
    internal CinematicAnimations(IReadOnlyDictionary<TAnimation, IHidingAnimationBeat<TFrame, TCue>[]> segments,
        Func<CinematicPlays<TAnimation, TFrame, TCue>, IReadOnlyDictionary<TAnimation, Animation<TAnimation>>> startingAt)
    {
        Ensure.NotNull(segments);
        Ensure.NotNull(startingAt);
        Segments = new HidingAnimationSegments<TAnimation, TFrame, TCue>(segments, segments.Keys.ToHashSet());
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

    /// <summary>
    /// Creates the family's animations as <see cref="CinematicAnimations{TAnimation, TFrame, TCue}(IReadOnlyDictionary{TAnimation,
    /// AnimationBeat{TFrame, TCue}[]}, Func{CinematicPlays{TAnimation, TFrame, TCue}, IReadOnlyDictionary{TAnimation, Animation{TAnimation}}})"/>
    /// does, with <paramref name="stills"/> besides (#2557): each a segment that shows its one picture for good, and the still
    /// animation an actor starting it plays, so a family of still pictures lists each picture once.
    /// </summary>
    /// <exception cref="ArgumentException">A still is also one of <paramref name="segments"/> or of the animations <paramref name="startingAt"/> writes.</exception>
    internal CinematicAnimations(IReadOnlyDictionary<TAnimation, TFrame> stills,
        IReadOnlyDictionary<TAnimation, IHidingAnimationBeat<TFrame, TCue>[]> segments,
        Func<CinematicPlays<TAnimation, TFrame, TCue>, IReadOnlyDictionary<TAnimation, Animation<TAnimation>>> startingAt)
        : this(WithStills(stills, segments), plays => WithStillStarts(stills, plays, Ensure.NotNull(startingAt)(plays)))
    {
    }

    /// <summary>The family of still and authored segments, the authored ones none of which hides the actor.</summary>
    /// <exception cref="ArgumentException">A still is also one of <paramref name="segments"/> or of the animations <paramref name="startingAt"/> writes.</exception>
    internal CinematicAnimations(IReadOnlyDictionary<TAnimation, TFrame> stills,
        IReadOnlyDictionary<TAnimation, AnimationBeat<TFrame, TCue>[]> segments,
        Func<CinematicPlays<TAnimation, TFrame, TCue>, IReadOnlyDictionary<TAnimation, Animation<TAnimation>>> startingAt)
        : this(stills, Ensure.NotNull(segments).ToDictionary(pair => pair.Key, pair => pair.Value.ToArray<IHidingAnimationBeat<TFrame, TCue>>()), startingAt)
    {
    }

    /// <summary>The segments the family's animations play.</summary>
    internal HidingAnimationSegments<TAnimation, TFrame, TCue> Segments { get; }

    /// <summary>The animation an actor starting <paramref name="animation"/> plays.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No actor of the family starts <paramref name="animation"/>.</exception>
    internal Animation<TAnimation> StartingWith(TAnimation animation) => starts.TryGetValue(animation, out Animation<TAnimation>? started)
        ? started
        : throw new ArgumentOutOfRangeException(nameof(animation), animation, "No actor of this cinematic family starts this animation.");

    /// <summary><paramref name="segments"/> with each of <paramref name="stills"/> as a segment holding its one picture for good.</summary>
    private static Dictionary<TAnimation, IHidingAnimationBeat<TFrame, TCue>[]> WithStills(IReadOnlyDictionary<TAnimation, TFrame> stills,
        IReadOnlyDictionary<TAnimation, IHidingAnimationBeat<TFrame, TCue>[]> segments)
    {
        var all = new Dictionary<TAnimation, IHidingAnimationBeat<TFrame, TCue>[]>(Ensure.NotNull(segments));
        foreach ((TAnimation still, TFrame picture) in Ensure.NotNull(stills))
            all.Add(still, [new AnimationBeat<TFrame, TCue>.HeldFrame(picture)]);
        return all;
    }

    /// <summary><paramref name="starting"/> with each of <paramref name="stills"/> started as its still animation.</summary>
    private static Dictionary<TAnimation, Animation<TAnimation>> WithStillStarts(IReadOnlyDictionary<TAnimation, TFrame> stills,
        CinematicPlays<TAnimation, TFrame, TCue> plays, IReadOnlyDictionary<TAnimation, Animation<TAnimation>> starting)
    {
        var all = new Dictionary<TAnimation, Animation<TAnimation>>(Ensure.NotNull(starting));
        foreach (TAnimation still in stills.Keys)
            all.Add(still, plays.Still(still));
        return all;
    }
}
