namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The wait between a WorkRobot's laser shots and where its lasers leave it.</summary>
internal static class WorkRobotLaserDefinitions
{
    /// <summary>The fixed part of a firing WorkRobot's laser cooldown, in updates, to which <see cref="CooldownJitter"/> adds.</summary>
    internal const int CooldownBase = 16;

    /// <summary>How far ahead of the robot, in the way it faces, its lasers leave, in pixels.</summary>
    internal const int MuzzleAhead = 4;

    /// <summary>How far above the robot its lasers leave, in pixels.</summary>
    internal const int MuzzleAbove = 16;

    /// <summary>
    /// The random field that adds zero to thirty-one updates to a firing WorkRobot's laser cooldown, before the fixed
    /// <see cref="CooldownBase"/>: the sample modulo 32.
    /// </summary>
    internal static readonly RandomSampleField CooldownJitter = new(0, 5);
}
