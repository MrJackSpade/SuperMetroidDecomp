namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One cue of the Rinkas' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct RinkaCue(RinkaCueKind Kind);
