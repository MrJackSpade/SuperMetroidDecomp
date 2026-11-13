namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One cue of the Ceres door's animations: what it does, with the values it acts on.</summary>
/// <param name="Kind">What the cue does.</param>
/// <param name="Target">The segment a branch continues at.</param>
internal readonly record struct CeresDoorCue(CeresDoorCueKind Kind, CeresDoorSegment Target = default);
