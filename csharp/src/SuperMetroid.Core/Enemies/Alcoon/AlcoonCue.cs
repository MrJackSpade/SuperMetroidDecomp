namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One cue of the Alcoon's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct AlcoonCue(AlcoonCueKind Kind);
