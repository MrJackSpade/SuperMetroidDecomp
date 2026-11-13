namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One cue of Choot's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct ChootCue(ChootCueKind Kind);
