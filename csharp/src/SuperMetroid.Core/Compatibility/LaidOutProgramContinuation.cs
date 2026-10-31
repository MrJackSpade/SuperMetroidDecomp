namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a frame read past an animation program's end reaches (#1473). The cartridge lays a kind of program out one after
/// another in one bank and reads a frame as the program's start plus the frame, so a frame past a program's end reads
/// the program laid out after it, shifted by the distance between their starts.
/// </summary>
/// <remarks>
/// Consulted when Samus's animation frame names no step of its pose's program. Evidence: the Mother Brain glitch
/// movie holds the right-facing drained standing pose at frame 11 on update 1947; that program occupies frames 0-7
/// and the left-facing drained standing program starts eight frames later, so the cartridge plays the left program's
/// frame 3. A target a later program names is shifted back by the distance (the cartridge's repeats step back by a
/// count); a restart is absolute and is not.
/// </remarks>
/// <typeparam name="TProgram">The kind of program.</typeparam>
[CompatibilityService("a frame past an animation program's end reads the program the cartridge lays out after it")]
public sealed class LaidOutProgramContinuation<TProgram>
    where TProgram : class
{
    /// <summary>A program and the position of its first frame in the cartridge's layout of its kind of program.</summary>
    /// <param name="Program">The program.</param>
    /// <param name="Start">Where its first frame lies in the layout.</param>
    public readonly record struct Entry(TProgram Program, int Start);

    /// <summary>Where a frame read past a program's end lands: a later program, its frame, and how far its frames are shifted.</summary>
    /// <param name="Program">The program laid out after the one read past.</param>
    /// <param name="Frame">The frame of <paramref name="Program"/> the read reaches.</param>
    /// <param name="Shift">
    /// How many frames later <paramref name="Program"/> starts than the program read past, which a frame target the later
    /// program names is offset by when the read is interpreted for the earlier program.
    /// </param>
    public readonly record struct Read(TProgram Program, int Frame, int Shift);

    /// <summary>Each program's position in the layout.</summary>
    private readonly IReadOnlyList<Entry> _layout;

    /// <summary>Keeps <paramref name="layout"/>, every program with its start, in layout order.</summary>
    /// <exception cref="ArgumentException">The starts do not ascend.</exception>
    public LaidOutProgramContinuation(IReadOnlyList<Entry> layout)
    {
        Ensure.NotNull(layout);
        for (int index = 1; index < layout.Count; index++)
        {
            if (layout[index].Start < layout[index - 1].Start)
                throw new ArgumentException($"Laid-out program {index} starts before the one before it.", nameof(layout));
        }
        _layout = layout;
    }

    /// <summary>
    /// Where a read at <paramref name="frame"/> of <paramref name="program"/>, past its end, lands; null when nothing is
    /// laid out after it.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="program"/> is not in the layout.</exception>
    public Read? ReadPast(TProgram program, int frame)
    {
        int start = -1;
        foreach (Entry laidOut in _layout)
        {
            if (start < 0)
            {
                if (ReferenceEquals(laidOut.Program, program))
                    start = laidOut.Start;
                continue;
            }
            if (laidOut.Start > start)
            {
                int shift = laidOut.Start - start;
                return frame < shift ? null : new Read(laidOut.Program, frame - shift, shift);
            }
        }
        return start < 0
            ? throw new ArgumentException("The program is not in the layout.", nameof(program))
            : null;
    }
}
