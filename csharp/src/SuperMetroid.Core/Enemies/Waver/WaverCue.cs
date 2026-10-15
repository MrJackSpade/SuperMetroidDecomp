namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>One cue of Waver's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct WaverCue(WaverCueKind Kind);
