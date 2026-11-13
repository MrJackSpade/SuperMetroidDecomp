namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>One cue of Waver's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct WaverCue(WaverCueKind Kind);
