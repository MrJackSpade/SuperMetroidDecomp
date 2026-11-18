namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// An independent reader of a routine a native cinematic list installs (#2489): the generic handler runs it on every update
/// of the actor, before the list's timer, and its updates raise cues of the family's own.
/// </summary>
/// <typeparam name="TCue">The family's cue.</typeparam>
internal interface INativeCinematicRoutine<TCue>
    where TCue : notnull
{
    /// <summary>Runs the routine for one update and returns the cues it raised.</summary>
    IReadOnlyList<TCue> Step();
}
