namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One cue of the dead Sidehopper's and Tourian corpses' animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct DeadTourianCue(DeadTourianCueKind Kind);
