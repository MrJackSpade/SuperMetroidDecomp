using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>A phase of Draygon's that runs from his state alone.</summary>
internal sealed class DraygonPlainStep : DraygonPhaseStep
{
    /// <summary>What the phase does each update.</summary>
    private readonly Action<DraygonBehavior, DraygonEnemyState, SamusState, int> _run;

    /// <summary>Builds the phase from what it does each update.</summary>
    /// <param name="phase">The phase the step runs.</param>
    /// <param name="run">What the phase does each update.</param>
    internal DraygonPlainStep(DraygonPhase phase, Action<DraygonBehavior, DraygonEnemyState, SamusState, int> run)
        : base(phase) => _run = run;

    /// <inheritdoc/>
    internal override void Run(DraygonBehavior draygon, DraygonEnemyState state, SamusState samus, int frameCounter) =>
        _run(draygon, state, samus, frameCounter);
}
