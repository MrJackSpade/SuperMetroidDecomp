using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The state of Work Robot: the laser velocity is a signed 1/256-pixel value and the falling speed a signed 1/65536-pixel value.
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

    /// <summary>
    /// The signed 1/256-pixel velocity, in pixels per update, given to newly fired robot lasers; only its X is meaningful, and
    /// its sign is the robot's facing.
    /// </summary>
    public EightEightVector<RoomPixels> LaserVelocity { get; internal set; }

    /// <summary>The wrapping countdown that gates the next laser attempt.</summary>
    public WrappingCountdown LaserCooldown { get; internal set; }

    /// <summary>The whole-pixel room position saved during a temporary ledge probe.</summary>
    public Position16<RoomPixels> BackupPosition { get; internal set; }

    /// <summary>
    /// An unpowered robot's signed 1/65536-pixel falling speed in pixels per update, which gravity accumulates with the
    /// fraction's overflow into the whole part and wraparound.
    /// </summary>
    public SignedSixteenSixteen FallVelocity { get; internal set; }
}
