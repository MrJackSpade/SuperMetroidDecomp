namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The Work Robot's laser programs (#641), one per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="WorkRobotLaserAnimations"/>.
/// </summary>
internal enum WorkRobotLaserSegment
{
    /// <summary>The initial animation.</summary>
    Initial,
    /// <summary>The loop animation.</summary>
    Loop,
}
