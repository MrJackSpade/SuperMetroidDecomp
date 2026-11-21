using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// The phase Draygon's body runs each update, holding what that phase needs (#2554): a swoop's phases hold the path the
/// rightward setup planned, and a goop pass's phases hold the swoop setup the pass leads into, so no phase reads a plan it
/// might not have. Each phase runs itself; <see cref="Phase"/> names it for what reads the fight from outside.
/// </summary>
internal abstract class DraygonPhaseStep
{
    /// <summary>Names the step by the phase it runs.</summary>
    /// <param name="phase">The phase the step runs.</param>
    private protected DraygonPhaseStep(DraygonPhase phase) => Phase = phase;

    /// <summary>The phase the step runs.</summary>
    internal DraygonPhase Phase { get; }

    /// <summary>Runs the phase for one update.</summary>
    /// <param name="draygon">Draygon's behaviour, whose components run the phase.</param>
    /// <param name="state">Draygon's encounter.</param>
    /// <param name="samus">Samus, whom the fight reads and moves.</param>
    /// <param name="frameCounter">The frame counter Draygon times his turrets and smoke by.</param>
    internal abstract void Run(DraygonBehavior draygon, DraygonEnemyState state, SamusState samus, int frameCounter);

    /// <inheritdoc/>
    public override string ToString() => Phase.ToString();
}
