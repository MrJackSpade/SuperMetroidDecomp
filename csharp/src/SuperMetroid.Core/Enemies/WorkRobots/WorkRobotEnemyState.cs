using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The state of bank-$A8's Work Robot: the laser velocity is a signed 8.8 value and the falling speed a signed 16.16
/// value. The values the cartridge keeps in the enemy's common record are emptied with it when the enemy is killed
/// (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class WorkRobotEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<WorkRobotProgram> Program { get; set; }

    /// <summary>
    /// True only when definition $E8FF saw the area's boss bit during initialization. Definition $E8FF can therefore
    /// own a deactivated state before Phantoon is defeated.
    /// </summary>
    public bool Powered { get; internal set; }

    /// <summary>
    /// The signed 8.8 velocity, in pixels per update, given to newly fired robot lasers; only its X is meaningful, and
    /// its sign is the robot's facing.
    /// </summary>
    public EightEightVector<RoomPixels> LaserVelocity { get; internal set; }

    /// <summary>The wrapping countdown that gates the next laser attempt.</summary>
    public NativeCountdown LaserCooldown { get; internal set; }

    /// <summary>The whole-pixel room position saved during a temporary ledge probe.</summary>
    public Position16<RoomPixels> BackupPosition { get; internal set; }

    /// <summary>
    /// Variables E (fraction) and F (whole pixels): an unpowered robot's signed 16.16 falling speed in pixels per
    /// update, which gravity accumulates with the cartridge's carry and wraparound.
    /// </summary>
    public SnesSignedSixteenSixteen FallVelocity { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        LaserVelocity = default;
        LaserCooldown = NativeCountdown.Start(0);
        BackupPosition = Position16<RoomPixels>.Zero;
        FallVelocity = SnesSignedSixteenSixteen.Zero;
    }
}
