namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>One cue of the Evir's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct EvirCue(EvirCueKind Kind);
