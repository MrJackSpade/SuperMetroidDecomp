namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The phase Draygon's body runs each update, holding what that phase needs (#2554): a swoop's phases hold the path the
/// rightward setup planned, and a goop pass's phases hold the swoop setup the pass leads into, so no phase reads a plan it
/// might not have. Each phase runs itself (<see cref="DraygonPhaseSteps"/>); <see cref="Phase"/> names it for what reads the
/// fight from outside.
/// </summary>
internal abstract class DraygonPhaseStep
{
    /// <summary>Names the step by the phase it runs.</summary>
    /// <param name="phase">The phase the step runs.</param>
    private protected DraygonPhaseStep(DraygonPhase phase) => Phase = phase;

    /// <summary>The phase the step runs.</summary>
    internal DraygonPhase Phase { get; }

    /// <summary>Runs the phase for one update.</summary>
    /// <param name="update">Draygon's behaviour, encounter, Samus and frame counter this update.</param>
    internal abstract void Run(DraygonPhaseUpdate update);

    /// <inheritdoc/>
    public override string ToString() => Phase.ToString();
}
