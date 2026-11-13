namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>One cue of the Shaktool's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct ShaktoolCue(ShaktoolCueKind Kind);
