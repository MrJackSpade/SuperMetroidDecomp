namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>One cue of Oum's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct MaridiaLargeSnailCue(MaridiaLargeSnailCueKind Kind);
