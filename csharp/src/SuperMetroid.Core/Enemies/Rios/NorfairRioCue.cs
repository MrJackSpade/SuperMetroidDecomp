namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One cue of the Norfair Rio's programs, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct NorfairRioCue(NorfairRioCueKind Kind);
