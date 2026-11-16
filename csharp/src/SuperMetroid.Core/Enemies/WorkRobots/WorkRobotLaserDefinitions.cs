namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The wait between a WorkRobot's laser shots, where its lasers leave it and how fast they fly.</summary>
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

    /// <summary>How much longer a WorkRobot waits to fire after it turns at a wall or a ledge, in updates.</summary>
    internal const int TurnDelay = 8;

    /// <summary>How much longer a WorkRobot waits to fire after a shot strikes it, in updates.</summary>
    internal const int RecoilDelay = 64;

    /// <summary>How fast a WorkRobot's laser flies the way the robot faces, two pixels an update.</summary>
    internal static SignedEightEight Speed { get; } = SignedEightEight.FromPixels(2);
}
