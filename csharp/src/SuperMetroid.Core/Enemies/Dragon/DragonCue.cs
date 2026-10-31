namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>One cue of Dragon's programs, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct DragonCue(DragonCueKind Kind);
