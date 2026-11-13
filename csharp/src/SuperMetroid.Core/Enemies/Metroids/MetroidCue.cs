namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>One cue of the Metroid's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct MetroidCue(MetroidCueKind Kind);
