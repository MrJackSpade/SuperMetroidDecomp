using System.Diagnostics.CodeAnalysis;
namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a frame that is no step of its own animation reads, and where a frame named for one animation lands in another
/// (#1473, #1474, #1535, #2048, #2395). The game lays a kind of animation out one after another and reads a frame as the
/// animation's start plus the frame, so a frame on a value one of an animation's cues carries reads that value as a step of
/// its own, a frame past an animation's end reads a frame of an animation laid out after it, and a frame a path writes for
/// one animation lands, on any other, wherever that number falls in it. The import pipeline decodes that layout once into
/// named outcomes: the step each cue value's frame reads, the frame each frame past a segment's cue goes on to, the frame
/// of the later animation each frame read past an animation reaches, and the frame each named place lands on in every
/// other animation. No place in the layout reaches the game.
/// </summary>
/// <remarks>
/// Consulted when Samus's animation frame names no step of its pose's animation, each time her animation step is read.
/// Evidence: the Mother Brain glitch movie holds the right-facing drained standing pose on its animation's fourth frame past
/// its end on update 1947, which reads the left-facing drained standing animation's fourth frame, laid out after it. A cue
/// read there that names a frame of the later animation moves the earlier one to the frame that reads it (the game's
/// repeats step back by a count); a restart is the earlier animation's own and moves it to its own start.
/// <para>
/// The same reading decides what the Baby's Hyper Beam freeze and its rainbow stop do to a Samus the drain has let go of
/// (#1535). Both set her animation frame without looking at her pose, to the frames the drained crouch's freeze and
/// stand-up start at, and a crouch, morph, stand or turn animation ends long before them, so her next animation steps read
/// the animations laid out after hers, cue values included. The right-facing morph at the rainbow stop reads the left-facing
/// morph's cue values as delays into the glitch-ball morphing animation's transition and becomes the right-facing glitch
/// ball; the left-facing stand and both unmorphs frozen for the Hyper Beam reach the Draygon-held morphing animation's
/// transition and become the Draygon-held ball; other reads end in the ordinary poses a later animation's transition names.
/// The animations these reads run through belong to no pose Samus holds: they run under the pose that read into them.
/// </para>
/// </remarks>
/// <typeparam name="TAnimation">
/// The identity of an animation. Samus's animations each have a name of their own, whatever poses animate by them, so an
/// animation only a read past an earlier one reaches needs no pose to name it, and the identity keeps its equality when a
/// session is restored.
/// </typeparam>
/// <typeparam name="TFrame">A named frame of an animation.</typeparam>
/// <typeparam name="TPlace">The name of a frame paths write onto, or compare with, whatever animation they meet.</typeparam>
/// <typeparam name="TStep">A step of an animation, which a cue value's frame reads.</typeparam>
[CompatibilityService("a frame past an animation's end reads a frame of an animation laid out after it, a frame on a value one of its cues carries reads that value as a step, and a frame named for one animation lands where its number falls in another")]
public sealed class FramesBeyondAnimation<TAnimation, TFrame, TPlace, TStep>
    where TAnimation : notnull
    where TFrame : notnull
    where TPlace : notnull
    where TStep : class
{
    /// <summary>What a frame read past its animation's end reaches.</summary>
    /// <param name="Animation">The animation laid out after the one read past, whose frame names the step read.</param>
    /// <param name="Frame">The frame of <paramref name="Animation"/> the read reaches: one of its steps or a value one of its cues carries.</param>
    /// <param name="Moves">
    /// For each frame of <paramref name="Animation"/> the cue read there names, the frame of the animation read past that the
    /// cue moves it to; empty when the step read names no frame.
    /// </param>
    public sealed record Read(TAnimation Animation, TFrame Frame, IReadOnlyDictionary<TFrame, TFrame> Moves);

    /// <summary>The step each frame on a value one of an animation's cues carries reads, by animation and frame.</summary>
    private readonly IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<TFrame, TStep>> _cueValueSteps;

    /// <summary>What each frame read past an animation's end reaches, by animation and frame.</summary>
    private readonly IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<TFrame, Read>> _readsPast;

    /// <summary>The frame each frame on or past the cue ending one of an animation's segments goes on to, by animation and frame.</summary>
    private readonly IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<TFrame, TFrame>> _framesAfterCues;

    /// <summary>The frame each named place lands on in each animation it does not belong to.</summary>
    private readonly IReadOnlyDictionary<TPlace, IReadOnlyDictionary<TAnimation, TFrame>> _landings;

    /// <summary>
    /// The ticks the timer starts from on each frame a pose start or a cue can move an animation to that holds no delay of
    /// the animation's own, by animation and frame.
    /// </summary>
    private readonly IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<TFrame, int>> _delaysRead;

    /// <summary>Keeps every animation's cue-value steps, frames after cues and read-past outcomes and every place's landings.</summary>
    /// <param name="cueValueSteps">Every animation of the layout, with the step each of its cue values' frames reads.</param>
    /// <param name="framesAfterCues">
    /// For animations of the layout, the frame each frame on or past the cue ending one of its segments other than its last
    /// goes on to (<see cref="FrameAfterCue"/>).
    /// </param>
    /// <param name="readsPast">What each frame read past an animation's end reaches, for animations of the layout.</param>
    /// <param name="landings">The frame each named place lands on in animations of the layout.</param>
    /// <param name="delaysRead">
    /// For animations of the layout, the ticks the timer starts from on each frame a pose start or a cue can move the
    /// animation to that holds no delay of its own (<see cref="DelayRead"/>).
    /// </param>
    /// <exception cref="ArgumentException">An outcome, landing or delay read names an animation outside the layout.</exception>
    public FramesBeyondAnimation(IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<TFrame, TStep>> cueValueSteps,
        IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<TFrame, TFrame>> framesAfterCues,
        IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<TFrame, Read>> readsPast,
        IReadOnlyDictionary<TPlace, IReadOnlyDictionary<TAnimation, TFrame>> landings,
        IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<TFrame, int>> delaysRead)
    {
        Ensure.NotNull(cueValueSteps);
        Ensure.NotNull(framesAfterCues);
        Ensure.NotNull(readsPast);
        Ensure.NotNull(landings);
        Ensure.NotNull(delaysRead);
        if (delaysRead.Keys.Any(animation => !cueValueSteps.ContainsKey(animation)))
            throw new ArgumentException("Delays read name an animation outside the layout.", nameof(delaysRead));
        if (framesAfterCues.Keys.Any(animation => !cueValueSteps.ContainsKey(animation)))
            throw new ArgumentException("Frames after cues name an animation outside the layout.", nameof(framesAfterCues));
        foreach (KeyValuePair<TAnimation, IReadOnlyDictionary<TFrame, Read>> pair in readsPast)
        {
            if (!cueValueSteps.ContainsKey(pair.Key))
                throw new ArgumentException($"Reads past animation {pair.Key}, which is not in the layout.", nameof(readsPast));
            if (pair.Value.Values.Any(read => !cueValueSteps.ContainsKey(read.Animation)))
                throw new ArgumentException($"A read past animation {pair.Key} reaches an animation outside the layout.", nameof(readsPast));
        }
        foreach (KeyValuePair<TPlace, IReadOnlyDictionary<TAnimation, TFrame>> pair in landings)
        {
            if (pair.Value.Keys.Any(animation => !cueValueSteps.ContainsKey(animation)))
                throw new ArgumentException($"Place {pair.Key} lands in an animation outside the layout.", nameof(landings));
        }
        _cueValueSteps = cueValueSteps;
        _framesAfterCues = framesAfterCues;
        _readsPast = readsPast;
        _landings = landings;
        _delaysRead = delaysRead;
    }

    /// <summary>
    /// The ticks the timer starts from when a pose starts <paramref name="animation"/> on <paramref name="frame"/>, or a cue
    /// moves it there, where the frame holds no delay of the animation's own: the game starts the timer from the frame's byte
    /// whatever it holds, a value a cue carries, a cue or a byte of an animation laid out after it, and the import decoded
    /// that byte once.
    /// </summary>
    /// <exception cref="InvalidDataException">The installation holds no delay read at this frame.</exception>
    public int DelayRead(TAnimation animation, TFrame frame) =>
        _delaysRead.TryGetValue(animation, out IReadOnlyDictionary<TFrame, int>? frames) && frames.TryGetValue(frame, out int ticks)
            ? ticks
            : throw new InvalidDataException($"Animation {animation} has no installed delay read at {frame}.");

    /// <summary>
    /// The step <paramref name="frame"/> of <paramref name="animation"/> reads when it lands on a value one of the
    /// animation's cues carries; null when the frame is no cue value's.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="animation"/> is not in the layout.</exception>
    public TStep? CueValueStep(TAnimation animation, TFrame frame) =>
        !_cueValueSteps.TryGetValue(animation, out IReadOnlyDictionary<TFrame, TStep>? steps)
            ? throw new ArgumentException($"Animation {animation} is not in the layout.", nameof(animation))
            : steps.TryGetValue(frame, out TStep? step) ? step : null;

    /// <summary>
    /// The frame <paramref name="frame"/> of <paramref name="animation"/>, on or past the cue ending one of its segments
    /// other than its last, goes on to: the next value the cue carries, or the next segment's first frame once the cue's
    /// values are behind it. A cue that continues into the next segment carries none, and the animation goes on from it
    /// without this outcome.
    /// </summary>
    /// <exception cref="InvalidDataException">The installation holds no frame after this one.</exception>
    public TFrame FrameAfterCue(TAnimation animation, TFrame frame) =>
        _framesAfterCues.TryGetValue(animation, out IReadOnlyDictionary<TFrame, TFrame>? frames) && frames.TryGetValue(frame, out TFrame? after)
            ? after
            : throw new InvalidDataException($"Animation {animation} has no installed frame after {frame}.");

    /// <summary>
    /// Whether <paramref name="frame"/> of <paramref name="animation"/>, read past the animation's end, reaches a step, and
    /// what it reaches; a read that reaches no step fails the game's animation.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="animation"/> is not in the layout.</exception>
    public bool TryReadPast(TAnimation animation, TFrame frame, [NotNullWhen(true)] out Read? read)
    {
        if (!_cueValueSteps.ContainsKey(animation))
            throw new ArgumentException($"Animation {animation} is not in the layout.", nameof(animation));
        read = null;
        return _readsPast.TryGetValue(animation, out IReadOnlyDictionary<TFrame, Read>? reads) && reads.TryGetValue(frame, out read);
    }

    /// <summary>The frame of <paramref name="animation"/> that <paramref name="place"/>, named for another animation, lands on.</summary>
    /// <exception cref="InvalidDataException">The installation holds no landing of the place in the animation.</exception>
    public TFrame Landing(TPlace place, TAnimation animation) =>
        _landings.TryGetValue(place, out IReadOnlyDictionary<TAnimation, TFrame>? frames) && frames.TryGetValue(animation, out TFrame? frame)
            ? frame
            : throw new InvalidDataException($"Place {place} has no installed landing in animation {animation}.");
}
