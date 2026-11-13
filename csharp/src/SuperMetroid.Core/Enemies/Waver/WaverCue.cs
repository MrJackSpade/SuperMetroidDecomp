namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>One cue of Waver's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct WaverCue(WaverCueKind Kind);
