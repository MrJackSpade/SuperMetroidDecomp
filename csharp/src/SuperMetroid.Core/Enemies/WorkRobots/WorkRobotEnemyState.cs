using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The state of a powered Work Robot (#2179): the way it faces, its laser cooldown and its fall. An unpowered robot keeps
/// <see cref="UnpoweredWorkRobotEnemyState"/> instead.
/// </summary>
public sealed class WorkRobotEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WorkRobotSegment> Playback { get; set; }

    /// <summary>The way the robot faces, which its walking steps and ledges set and its lasers fly (#1845).</summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>The wrapping countdown that gates the next laser attempt.</summary>
    public WrappingCountdown LaserCooldown { get; internal set; }

    /// <summary>
    /// The robot's signed 1/65536-pixel falling speed in pixels per update: it starts at rest, and gravity accumulates it
    /// with the fraction's overflow into the whole part and wraparound on every update nothing holds the robot up.
    /// </summary>
    public SignedSixteenSixteen FallVelocity { get; internal set; }
}
