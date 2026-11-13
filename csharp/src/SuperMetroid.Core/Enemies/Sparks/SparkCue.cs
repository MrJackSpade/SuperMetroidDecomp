namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>One cue of the Wrecked Ship Spark's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct SparkCue(SparkCueKind Kind);
