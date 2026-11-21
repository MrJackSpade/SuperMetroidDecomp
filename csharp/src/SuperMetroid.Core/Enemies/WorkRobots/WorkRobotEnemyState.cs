using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The state of a powered Work Robot (#2179): the way it faces, its laser cooldown and its fall. An unpowered robot keeps
/// <see cref="UnpoweredWorkRobotEnemyState"/> instead.
/// </summary>
public sealed partial class WorkRobotEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WorkRobotSegment> Playback { get; set; }

    /// <summary>The way the robot faces, which its walking steps and ledges set and its lasers fly (#1845).</summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>
    /// The wait before the robot's next laser (#2380): its steps and laser tries count it down, stopping at zero, and a try
    /// at zero fires and starts a new wait. A turn at a wall or ledge and each shot or bomb that makes the robot recoil
    /// lengthen it, and a lengthening carried past 65535 wraps to a short wait, as the game's did. That wrap is reachable in
    /// play, so the wait stays a wrapping countdown: every recoiling hit adds <see cref="WorkRobotLaserDefinitions.RecoilDelay"/>, and nothing bounds the hits a powered robot
    /// takes (a beam without plasma leaves it no invincibility between hits), so a thousand or so shots landing faster than its
    /// steps count them off carry it over the top and the robot fires sooner.
    /// </summary>
    public WrappingCountdown LaserCooldown { get; internal set; }

    /// <summary>
    /// The robot's falling speed each update: it starts at rest, and gravity raises it every update nothing
    /// holds the robot up.
    /// </summary>
    public SignedSixteenSixteen FallVelocity { get; internal set; }
}
