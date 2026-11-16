namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a frame that is no step of its own animation reads, and where a frame named for one animation lands in another
/// (#1473, #1474, #1535, #2048). The game lays a kind of animation out one after another and reads a frame as the
/// animation's start plus the frame, so a frame on a value one of an animation's cues carries reads that value as a step of
/// its own, a frame past an animation's end reads a frame of an animation laid out after it, and a frame a path writes for
/// one animation lands, on any other, wherever that number falls in it. The import pipeline decodes that layout once into
/// named outcomes: the step each cue value's frame reads, the frame of the later animation each frame read past an
/// animation reaches, and the frame each named place lands on in every other animation. No place in the layout reaches the
/// game.
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
/// The transition poses whose animations these reads run through are never held themselves: their animations run under the
/// pose that read into them.
/// </para>
/// </remarks>
/// <typeparam name="TAnimation">
/// The identity of an animation. Samus's animations are named by a pose that animates by each, so the identity keeps its
/// equality when a session is restored.
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

    /// <summary>The frame each named place lands on in each animation it does not belong to.</summary>
    private readonly IReadOnlyDictionary<TPlace, IReadOnlyDictionary<TAnimation, TFrame>> _landings;

    /// <summary>Keeps every animation's cue-value steps and read-past outcomes and every place's landings.</summary>
    /// <param name="cueValueSteps">Every animation of the layout, with the step each of its cue values' frames reads.</param>
    /// <param name="readsPast">What each frame read past an animation's end reaches, for animations of the layout.</param>
    /// <param name="landings">The frame each named place lands on in animations of the layout.</param>
    /// <exception cref="ArgumentException">An outcome or landing names an animation outside the layout.</exception>
    public FramesBeyondAnimation(IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<TFrame, TStep>> cueValueSteps,
        IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<TFrame, Read>> readsPast,
        IReadOnlyDictionary<TPlace, IReadOnlyDictionary<TAnimation, TFrame>> landings)
    {
        Ensure.NotNull(cueValueSteps);
        Ensure.NotNull(readsPast);
        Ensure.NotNull(landings);
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
        _readsPast = readsPast;
        _landings = landings;
    }

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
    /// What <paramref name="frame"/> of <paramref name="animation"/>, read past the animation's end, reaches; null where the
    /// read reaches no step, which fails the game's animation.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="animation"/> is not in the layout.</exception>
    public Read? ReadPast(TAnimation animation, TFrame frame) =>
        !_cueValueSteps.ContainsKey(animation)
            ? throw new ArgumentException($"Animation {animation} is not in the layout.", nameof(animation))
            : _readsPast.TryGetValue(animation, out IReadOnlyDictionary<TFrame, Read>? reads) && reads.TryGetValue(frame, out Read? read)
                ? read
                : null;

    /// <summary>The frame of <paramref name="animation"/> that <paramref name="place"/>, named for another animation, lands on.</summary>
    /// <exception cref="InvalidDataException">The installation holds no landing of the place in the animation.</exception>
    public TFrame Landing(TPlace place, TAnimation animation) =>
        _landings.TryGetValue(place, out IReadOnlyDictionary<TAnimation, TFrame>? frames) && frames.TryGetValue(animation, out TFrame? frame)
            ? frame
            : throw new InvalidDataException($"Place {place} has no installed landing in animation {animation}.");
}
