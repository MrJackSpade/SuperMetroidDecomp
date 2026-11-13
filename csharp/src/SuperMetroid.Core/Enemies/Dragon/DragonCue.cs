namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>One cue of Dragon's animations: what it does.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct DragonCue(DragonCueKind Kind);
