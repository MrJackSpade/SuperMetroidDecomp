namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>One cue of the Shaktool's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct ShaktoolCue(ShaktoolCueKind Kind);
