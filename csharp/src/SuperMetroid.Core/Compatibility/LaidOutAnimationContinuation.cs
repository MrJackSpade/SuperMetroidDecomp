namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a frame read past an animation's end, or on a cue's operand, reaches (#1473, #1474, #1535). The
/// game lays a kind of animation out one after another and reads a frame as the animation's start plus the frame, so
/// a frame past an animation's end reads the animation laid out after it, shifted by the distance between their starts, and a
/// frame on the place of a cue's operand reads that operand as a step of its own. The import pipeline decodes
/// that layout once into each animation's successor and that shift, and the step each operand place reads; no place in the
/// layout reaches the game.
/// </summary>
/// <remarks>
/// Consulted when Samus's animation frame names no step of its pose's animation, each time her animation step is read.
/// Evidence: the Mother Brain glitch movie holds the right-facing drained standing pose at frame 11 on update 1947; that
/// animation occupies frames 0-7 and the left-facing drained standing animation starts eight frames later, so the original
/// game plays the left animation's frame 3. A target a later animation names is shifted back by the distance (the original
/// game's repeats step back by a count); a restart is absolute and is not.
/// <para>
/// The same reading decides what the Baby's Hyper Beam freeze and its rainbow stop do to a Samus the drain has let go of
/// (#1535). Both set her animation frame without looking at her pose, to the frames the drained crouch's freeze and
/// stand-up start at, and a crouch, morph, stand or turn animation ends long before them, so her next animation steps read
/// the animations laid out after hers, operands included. The right-facing morph at the rainbow stop reads the left-facing
/// morph's operands as delays into the glitch-ball morphing animation's transition and becomes the right-facing glitch
/// ball; the left-facing stand and both unmorphs frozen for the Hyper Beam reach the Draygon-held morphing animation's
/// transition and become the Draygon-held ball; other reads end in the ordinary poses a later animation's transition names.
/// The transition poses whose animations these reads run through are never held themselves: their animations run under the
/// pose that read into them.
/// </para>
/// <para>
/// The typed animations whose gameplay reads which segment a resting animation stands in (#1600) keep their segments' layout in
/// the same terms: a segment that runs on into the segment authored after it, directly after its last beat, has that segment as
/// its successor, starting as many beats later, so a play resting after that beat reads the next segment, as the game's
/// animation position then rests at its entry.
/// </para>
/// </remarks>
/// <typeparam name="TAnimation">
/// The identity of an animation. Samus's animations are named by a pose that animates by each, so the identity keeps its
/// equality when a session is restored.
/// </typeparam>
/// <typeparam name="TStep">A step of an animation, which an operand's place reads.</typeparam>
[CompatibilityService("a frame past an animation program's end reads the program laid out after it, and a frame on an instruction's operand reads the operand as a step")]
public sealed class LaidOutAnimationContinuation<TAnimation, TStep>
    where TAnimation : notnull
    where TStep : class
{
    /// <summary>The animation laid out after another, and how many frames later it starts.</summary>
    /// <param name="Animation">The animation laid out next.</param>
    /// <param name="Shift">How many frames after the earlier animation's first frame it starts, one or more.</param>
    public readonly record struct Successor(TAnimation Animation, int Shift);

    /// <summary>Where a frame read past an animation's end lands: a later animation, its frame, and how far its frames are shifted.</summary>
    /// <param name="Animation">The animation laid out after the one read past.</param>
    /// <param name="Frame">The frame of <paramref name="Animation"/> the read reaches.</param>
    /// <param name="Shift">
    /// How many frames later <paramref name="Animation"/> starts than the animation read past, which a frame target the later
    /// animation names is offset by when the read is interpreted for the earlier animation.
    /// </param>
    public readonly record struct Read(TAnimation Animation, int Frame, int Shift);

    /// <summary>Each animation's successor, or none for the last animation laid out.</summary>
    private readonly IReadOnlyDictionary<TAnimation, Successor?> _successors;

    /// <summary>The step each animation's operand places read, by frame in the animation's own numbering.</summary>
    private readonly IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<int, TStep>> _operandSteps;

    /// <summary>Keeps every animation's successor, the last animation laid out having none, and its operand places' steps.</summary>
    /// <exception cref="ArgumentException">
    /// A successor starts no later than the animation before it, or the operand steps name an animation outside the layout or a
    /// frame before an animation's start.
    /// </exception>
    public LaidOutAnimationContinuation(IReadOnlyDictionary<TAnimation, Successor?> successors,
        IReadOnlyDictionary<TAnimation, IReadOnlyDictionary<int, TStep>> operandSteps)
    {
        Ensure.NotNull(successors);
        Ensure.NotNull(operandSteps);
        foreach (KeyValuePair<TAnimation, Successor?> pair in successors)
        {
            if (pair.Value is { Shift: <= 0 })
                throw new ArgumentException($"The program laid out after {pair.Key} does not start after it.", nameof(successors));
        }
        foreach (KeyValuePair<TAnimation, IReadOnlyDictionary<int, TStep>> pair in operandSteps)
        {
            if (!successors.ContainsKey(pair.Key))
                throw new ArgumentException($"Operand steps name program {pair.Key}, which is not in the layout.", nameof(operandSteps));
            if (pair.Value.Keys.Any(frame => frame < 0))
                throw new ArgumentException($"Operand steps of {pair.Key} name a frame before its start.", nameof(operandSteps));
        }
        _successors = successors;
        _operandSteps = operandSteps;
    }

    /// <summary>
    /// Where a read at <paramref name="frame"/> of <paramref name="animation"/>, past its end, lands; null when nothing is
    /// laid out after it or the frame falls short of the next animation's start.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="animation"/> is not in the layout.</exception>
    public Read? ReadPast(TAnimation animation, int frame) =>
        !_successors.TryGetValue(animation, out Successor? successor)
            ? throw new ArgumentException($"Program {animation} is not in the layout.", nameof(animation))
            : successor is not { } next || frame < next.Shift
                ? null
                : new Read(next.Animation, frame - next.Shift, next.Shift);

    /// <summary>
    /// The step <paramref name="frame"/> of <paramref name="animation"/> reads when it lands on a cue's operand, in
    /// the animation's own frame numbering; null when the frame is no operand's place of the animation.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="animation"/> is not in the layout.</exception>
    public TStep? OperandStep(TAnimation animation, int frame) =>
        !_successors.ContainsKey(animation)
            ? throw new ArgumentException($"Program {animation} is not in the layout.", nameof(animation))
            : _operandSteps.TryGetValue(animation, out IReadOnlyDictionary<int, TStep>? steps) && steps.TryGetValue(frame, out TStep? step)
                ? step
                : null;
}
