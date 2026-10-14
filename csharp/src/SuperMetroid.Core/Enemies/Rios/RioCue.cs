namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One cue of the Rio's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct RioCue(RioCueKind Kind);
