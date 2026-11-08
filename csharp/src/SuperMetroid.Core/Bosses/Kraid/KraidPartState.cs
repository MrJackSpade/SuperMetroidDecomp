using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// What every one of Kraid's parts runs: its typed program, the function it dispatches and the function timer that
/// installs its next function. Each part kind keeps its own state on top of this in its own type: his body
/// (<see cref="KraidBodyState"/>), his arm (<see cref="KraidArmState"/>), each lint (<see cref="KraidLintState"/>), his
/// foot (<see cref="KraidFootState"/>) and each fingernail (<see cref="KraidNailState"/>). The values the part's record
/// keeps are emptied with the record when the part is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public abstract class KraidPartState
{
    /// <summary>Where the part's typed program resumes; it replaces the instruction pointer.</summary>
    internal AnimationProgramCursor<KraidPartProgram> Cursor { get; set; }

    /// <summary>
    /// Gets the function a part's function timer installs when it expires. The original game keeps the foot's in the
    /// same word as its think timer (<see cref="KraidFootState.ThinkTimer"/>); the foot never reads one after writing the
    /// other (it writes its think timer whenever it starts a second-phase walk and its next function whenever it starts a
    /// timed phase), so each is held on its own.
    /// </summary>
    public KraidAiFunction NextFunction { get; internal set; }

    /// <summary>The function the part dispatches.</summary>
    public KraidAiFunction Function { get; internal set; }

    /// <summary>The updates before the part's function timer installs <see cref="NextFunction"/>.</summary>
    public int FunctionTimer { get; internal set; }

    /// <summary>Empties the values the game keeps in the part's common record: its function and function timer, and the part kind's own.</summary>
    internal virtual void ClearWithRecord()
    {
        Function = KraidAiFunction.None;
        FunctionTimer = 0;
    }
}
