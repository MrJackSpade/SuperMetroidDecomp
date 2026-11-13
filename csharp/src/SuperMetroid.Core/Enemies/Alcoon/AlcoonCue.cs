namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One cue of the Alcoon's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct AlcoonCue(AlcoonCueKind Kind);
