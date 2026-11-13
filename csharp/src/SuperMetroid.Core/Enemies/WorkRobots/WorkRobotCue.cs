namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One cue of the Work Robot's animations, with its typed operands.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct WorkRobotCue(WorkRobotCueKind Kind);
