namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One cue of the Yapping Maw's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct YappingMawCue(YappingMawCueKind Kind);
