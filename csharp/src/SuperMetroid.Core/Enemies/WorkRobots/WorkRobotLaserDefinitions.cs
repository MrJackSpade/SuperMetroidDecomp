namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The random wait between a WorkRobot's laser shots.</summary>
internal static class WorkRobotLaserDefinitions
{
    /// <summary>
    /// The random field that adds zero to thirty-one updates to a firing WorkRobot's laser cooldown, before the fixed
    /// sixteen: the sample modulo 32.
    /// </summary>
    internal static readonly RandomSampleField CooldownJitter = new(0, 5);
}
