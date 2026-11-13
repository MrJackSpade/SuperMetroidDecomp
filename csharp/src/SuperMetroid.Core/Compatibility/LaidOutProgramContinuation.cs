namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a frame read past an animation program's end, or on a cue's operand, reaches (#1473, #1474, #1535). The
/// game lays a kind of program out one after another and reads a frame as the program's start plus the frame, so
/// a frame past a program's end reads the program laid out after it, shifted by the distance between their starts, and a
/// frame on the place of a cue's operand reads that operand as a step of its own. The import pipeline decodes
/// that layout once into each program's successor and that shift, and the step each operand place reads; no place in the
/// layout reaches the game.
/// </summary>
/// <remarks>
/// Consulted when Samus's animation frame names no step of its pose's program, each time her animation step is read.
/// Evidence: the Mother Brain glitch movie holds the right-facing drained standing pose at frame 11 on update 1947; that
/// program occupies frames 0-7 and the left-facing drained standing program starts eight frames later, so the original
/// game plays the left program's frame 3. A target a later program names is shifted back by the distance (the original
/// game's repeats step back by a count); a restart is absolute and is not.
/// <para>
/// The same reading decides what the Baby's Hyper Beam freeze and its rainbow stop do to a Samus the drain has let go of
/// (#1535). Both set her animation frame without looking at her pose, to the frames the drained crouch's freeze and
/// stand-up start at, and a crouch, morph, stand or turn program ends long before them, so her next animation steps read
/// the programs laid out after hers, operands included. The right-facing morph at the rainbow stop reads the left-facing
/// morph's operands as delays into the glitch-ball morphing program's transition and becomes the right-facing glitch
/// ball; the left-facing stand and both unmorphs frozen for the Hyper Beam reach the Draygon-held morphing program's
/// transition and become the Draygon-held ball; other reads end in the ordinary poses a later program's transition names.
/// The transition poses whose programs these reads run through are never held themselves: their programs run under the
/// pose that read into them.
/// </para>
/// <para>
/// The typed animations whose gameplay reads which list a resting animation stands in (#1600) keep their lists' layout in
/// the same terms: a list that runs on into the list authored after it, directly after its last beat, has that list as
/// its successor, starting as many beats later, so a play resting after that beat reads the next list, as the game's
/// animation position then rests at its entry.
/// </para>
/// </remarks>
/// <typeparam name="TProgram">
/// The identity of a program. Samus's programs are named by a pose that animates by each, so the identity keeps its
/// equality when a session is restored.
/// </typeparam>
/// <typeparam name="TStep">A step of a program, which an operand's place reads.</typeparam>
[CompatibilityService("a frame past an animation program's end reads the program laid out after it, and a frame on an instruction's operand reads the operand as a step")]
public sealed class LaidOutProgramContinuation<TProgram, TStep>
    where TProgram : notnull
    where TStep : class
{
    /// <summary>The program laid out after another, and how many frames later it starts.</summary>
    /// <param name="Program">The program laid out next.</param>
    /// <param name="Shift">How many frames after the earlier program's first frame it starts, one or more.</param>
    public readonly record struct Successor(TProgram Program, int Shift);

    /// <summary>Where a frame read past a program's end lands: a later program, its frame, and how far its frames are shifted.</summary>
    /// <param name="Program">The program laid out after the one read past.</param>
    /// <param name="Frame">The frame of <paramref name="Program"/> the read reaches.</param>
    /// <param name="Shift">
    /// How many frames later <paramref name="Program"/> starts than the program read past, which a frame target the later
    /// program names is offset by when the read is interpreted for the earlier program.
    /// </param>
    public readonly record struct Read(TProgram Program, int Frame, int Shift);

    /// <summary>Each program's successor, or none for the last program laid out.</summary>
    private readonly IReadOnlyDictionary<TProgram, Successor?> _successors;

    /// <summary>The step each program's operand places read, by frame in the program's own numbering.</summary>
    private readonly IReadOnlyDictionary<TProgram, IReadOnlyDictionary<int, TStep>> _operandSteps;

    /// <summary>Keeps every program's successor, the last program laid out having none, and its operand places' steps.</summary>
    /// <exception cref="ArgumentException">
    /// A successor starts no later than the program before it, or the operand steps name a program outside the layout or a
    /// frame before a program's start.
    /// </exception>
    public LaidOutProgramContinuation(IReadOnlyDictionary<TProgram, Successor?> successors,
        IReadOnlyDictionary<TProgram, IReadOnlyDictionary<int, TStep>> operandSteps)
    {
        Ensure.NotNull(successors);
        Ensure.NotNull(operandSteps);
        foreach (KeyValuePair<TProgram, Successor?> pair in successors)
        {
            if (pair.Value is { Shift: <= 0 })
                throw new ArgumentException($"The program laid out after {pair.Key} does not start after it.", nameof(successors));
        }
        foreach (KeyValuePair<TProgram, IReadOnlyDictionary<int, TStep>> pair in operandSteps)
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
    /// Where a read at <paramref name="frame"/> of <paramref name="program"/>, past its end, lands; null when nothing is
    /// laid out after it or the frame falls short of the next program's start.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="program"/> is not in the layout.</exception>
    public Read? ReadPast(TProgram program, int frame) =>
        !_successors.TryGetValue(program, out Successor? successor)
            ? throw new ArgumentException($"Program {program} is not in the layout.", nameof(program))
            : successor is not { } next || frame < next.Shift
                ? null
                : new Read(next.Program, frame - next.Shift, next.Shift);

    /// <summary>
    /// The step <paramref name="frame"/> of <paramref name="program"/> reads when it lands on a cue's operand, in
    /// the program's own frame numbering; null when the frame is no operand's place of the program.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="program"/> is not in the layout.</exception>
    public TStep? OperandStep(TProgram program, int frame) =>
        !_successors.ContainsKey(program)
            ? throw new ArgumentException($"Program {program} is not in the layout.", nameof(program))
            : _operandSteps.TryGetValue(program, out IReadOnlyDictionary<int, TStep>? steps) && steps.TryGetValue(frame, out TStep? step)
                ? step
                : null;
}
