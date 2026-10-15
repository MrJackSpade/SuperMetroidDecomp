namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>One cue of the Shaktool's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct ShaktoolCue(ShaktoolCueKind Kind);
