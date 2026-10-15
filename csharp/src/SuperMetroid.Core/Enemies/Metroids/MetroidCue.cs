namespace SuperMetroid.Core.Enemies.Metroids;

/// <summary>One cue of the Metroid's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct MetroidCue(MetroidCueKind Kind);
