namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>One cue of the Evir's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly partial record struct EvirCue(EvirCueKind Kind);
