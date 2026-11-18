namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The runs of steps the powered Work Robots' glow plays (#2380). Their beats are <see cref="WorkRobotGlowAnimations"/>.</summary>
internal enum WorkRobotGlowSegment
{
    /// <summary>
    /// The glow's brightest red crossing the robots' red parts and back: held at each end, passing quickly between.
    /// </summary>
    Glowing,

    /// <summary>
    /// The glow restarting (#2451): the place the glow's last step shows, for the one update a powered robot's setup gives
    /// it before the glow's first step.
    /// </summary>
    Restarting,
}
