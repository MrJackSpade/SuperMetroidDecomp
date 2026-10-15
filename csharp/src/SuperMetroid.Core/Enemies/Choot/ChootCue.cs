namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One cue of Choot's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct ChootCue(ChootCueKind Kind);
