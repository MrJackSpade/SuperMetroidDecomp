namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One cue of the dead Sidehopper's and Tourian corpses' programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct DeadTourianCue(DeadTourianCueKind Kind);
