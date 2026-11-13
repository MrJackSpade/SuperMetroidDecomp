namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>One cue of the Wrecked Ship Spark's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct SparkCue(SparkCueKind Kind);
