namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>One cue of Owtch's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct OwtchCue(OwtchCueKind Kind);
