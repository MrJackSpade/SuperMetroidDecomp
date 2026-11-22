namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres door's animations: the event it signals.</summary>
/// <param name="Kind">What the cue signals.</param>
internal readonly partial record struct CeresDoorCue(CeresDoorCueKind Kind);
