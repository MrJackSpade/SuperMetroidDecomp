using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// A phase of a goop pass, holding the swoop setup the pass leads into once it leaves the screen: a leftward pass leads into
/// the rightward setup, which plans a new path; a rightward pass leads into the leftward setup over the path last planned.
/// </summary>
internal sealed class DraygonGoopStep : DraygonPhaseStep
{
    /// <summary>What the phase does each update of the pass.</summary>
    private readonly Action<DraygonBehavior, DraygonEnemyState, SamusState, int, DraygonPhaseStep> _run;

    /// <summary>Builds the phase of a pass leading into <paramref name="exit"/>.</summary>
    /// <param name="phase">The phase the step runs.</param>
    /// <param name="exit">The swoop setup the pass leads into.</param>
    /// <param name="run">What the phase does each update of the pass.</param>
    internal DraygonGoopStep(DraygonPhase phase, DraygonPhaseStep exit,
        Action<DraygonBehavior, DraygonEnemyState, SamusState, int, DraygonPhaseStep> run)
        : base(phase)
    {
        Exit = exit;
        _run = run;
    }

    /// <summary>The swoop setup the pass leads into once it leaves the screen.</summary>
    internal DraygonPhaseStep Exit { get; }

    /// <inheritdoc/>
    internal override void Run(DraygonBehavior draygon, DraygonEnemyState state, SamusState samus, int frameCounter) =>
        _run(draygon, state, samus, frameCounter, Exit);
}
