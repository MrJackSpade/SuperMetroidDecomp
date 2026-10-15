namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One cue of the Yapping Maw's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct YappingMawCue(YappingMawCueKind Kind);
