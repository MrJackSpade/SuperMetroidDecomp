namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>One cue of Dragon's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct DragonCue(DragonCueKind Kind);
