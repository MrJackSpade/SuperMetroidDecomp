namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>One cue of Owtch's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct OwtchCue(OwtchCueKind Kind);
