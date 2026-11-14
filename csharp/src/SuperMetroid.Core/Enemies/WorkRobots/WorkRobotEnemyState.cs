using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The state of Work Robot: the way it faces, its laser cooldown and, unpowered, its fall.
/// </summary>
public sealed class WorkRobotEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WorkRobotSegment> Playback { get; set; }

    /// <summary>
    /// True only when the unpowered robot definition saw the area boss defeated during initialization. That definition can therefore
    /// own a deactivated state before Phantoon is defeated.
    /// </summary>
    public bool Powered { get; internal set; }

    /// <summary>The way the robot faces, which its walking steps and ledges set and its lasers fly (#1845).</summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>The wrapping countdown that gates the next laser attempt.</summary>
    public WrappingCountdown LaserCooldown { get; internal set; }

    /// <summary>
    /// An unpowered robot's signed 1/65536-pixel falling speed in pixels per update, which gravity accumulates with the
    /// fraction's overflow into the whole part and wraparound.
    /// </summary>
    public SignedSixteenSixteen FallVelocity { get; internal set; }
}
