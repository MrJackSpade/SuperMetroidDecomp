namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One cue of the Rinkas' animations, with the operands it carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct RinkaCue(RinkaCueKind Kind);
