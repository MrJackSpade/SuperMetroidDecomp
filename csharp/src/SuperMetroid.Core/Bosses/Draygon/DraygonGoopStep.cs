namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// A phase of a goop pass, holding the swoop setup the pass leads into once it leaves the screen: a leftward pass leads into
/// the rightward setup, which plans a new path; a rightward pass leads into the leftward setup over the path last planned.
/// </summary>
/// <param name="phase">The phase the step runs.</param>
/// <param name="exit">The swoop setup the pass leads into.</param>
internal abstract partial class DraygonGoopStep(DraygonPhase phase, DraygonPhaseStep exit) : DraygonPhaseStep(phase)
{
    /// <summary>The swoop setup the pass leads into once it leaves the screen.</summary>
    internal DraygonPhaseStep Exit { get; } = exit;
}
