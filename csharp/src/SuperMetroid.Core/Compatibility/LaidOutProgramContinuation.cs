namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a frame read past an animation program's end reaches (#1473, #1474). The original game lays a kind of program out
/// one after another and reads a frame as the program's start plus the frame, so a frame past a program's end reads the
/// program laid out after it, shifted by the distance between their starts. The import pipeline decodes that layout once
/// into each program's successor and that shift; no place in the layout reaches the game.
/// </summary>
/// <remarks>
/// Consulted when Samus's animation frame names no step of its pose's program, each time her animation step is read.
/// Evidence: the Mother Brain glitch movie holds the right-facing drained standing pose at frame 11 on update 1947; that
/// program occupies frames 0-7 and the left-facing drained standing program starts eight frames later, so the original
/// game plays the left program's frame 3. A target a later program names is shifted back by the distance (the original
/// game's repeats step back by a count); a restart is absolute and is not.
/// </remarks>
/// <typeparam name="TProgram">
/// The identity of a program. Samus's programs are named by a pose that animates by each, so the identity keeps its
/// equality when a session is restored.
/// </typeparam>
[CompatibilityService("a frame past an animation program's end reads the program laid out after it")]
public sealed class LaidOutProgramContinuation<TProgram>
    where TProgram : notnull
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

    /// <summary>Keeps every program's successor; the last program laid out has none.</summary>
    /// <exception cref="ArgumentException">A successor starts no later than the program before it.</exception>
    public LaidOutProgramContinuation(IReadOnlyDictionary<TProgram, Successor?> successors)
    {
        Ensure.NotNull(successors);
        foreach (KeyValuePair<TProgram, Successor?> pair in successors)
        {
            if (pair.Value is { Shift: <= 0 })
                throw new ArgumentException($"The program laid out after {pair.Key} does not start after it.", nameof(successors));
        }
        _successors = successors;
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
}
