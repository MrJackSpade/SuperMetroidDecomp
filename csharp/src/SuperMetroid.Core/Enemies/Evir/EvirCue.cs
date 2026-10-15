namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>One cue of the Evir's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct EvirCue(EvirCueKind Kind);
