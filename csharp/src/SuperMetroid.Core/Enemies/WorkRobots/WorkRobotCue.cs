namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One cue of the Work Robot's programs, with the operands its native instruction carries.</summary>
/// <param name="Kind">What the cue does.</param>
internal readonly record struct WorkRobotCue(WorkRobotCueKind Kind);
