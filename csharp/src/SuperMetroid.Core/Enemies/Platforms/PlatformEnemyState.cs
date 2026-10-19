using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The state of one Tripper or Kamer platform: its patrol speeds, the height it rises back to, its vertical
/// acceleration, and the movement and art it has selected. The resting height, the speeds and the acceleration index
/// are values the cartridge keeps in the enemy's common record, so they are emptied with that record when the enemy
/// is killed (<see cref="ClearWithRecord"/>); the rest outlive the kill.
/// </summary>
public sealed class PlatformEnemyState
{
    /// <summary>Where the typed animation (#641) resumes.</summary>
    internal AnimationProgramCursor<PlatformProgram> Program { get; set; }

    /// <summary>
    /// The height a depressed platform rises back to, one pixel below its population position; only Y is meaningful.
    /// Rising stops once the platform's whole Y lies above it, measured as the wrapped signed difference, so a height
    /// that wrapped past the room's top edge stops the platform as the cartridge does.
    /// </summary>
    public Position16<RoomPixels> RestingHeight { get; internal set; }

    /// <summary>Signed 16.16 room-pixel displacement per horizontal actor update while patrolling right, from the positive half of the platform's linear-speed record.</summary>
    public SnesSignedSixteenSixteen RightSpeed { get; internal set; }

    /// <summary>Signed 16.16 room-pixel displacement per horizontal actor update while patrolling left, from the negative half of the platform's linear-speed record.</summary>
    public SnesSignedSixteenSixteen LeftSpeed { get; internal set; }

    /// <summary>
    /// Record index into the shared quadratically increasing enemy speeds, from zero to
    /// <see cref="MaximumYSpeedTableIndex"/>: it starts at zero, rises by one per accelerating update and is clamped
    /// at the maximum, so it never leaves that range.
    /// </summary>
    public int YSpeedTableIndex { get; internal set; }

    /// <summary>The rider-selected vertical movement, recomputed each actor update before horizontal then vertical motion.</summary>
    public PlatformVerticalMovement YMovement { get; internal set; }

    /// <summary>The patrol direction: initialized from population parameter 1 and changed by wall collisions or animation cues.</summary>
    public PlatformHorizontalMovement XMovement { get; internal set; }

    /// <summary>Whether moving art has been installed for the current vertical phase.</summary>
    public bool VerticallyMovingArtInstalled { get; internal set; }

    /// <summary>Whether vertically-still art has been installed for the current vertical phase.</summary>
    public bool VerticallyStillArtInstalled { get; internal set; }

    /// <summary>The inclusive cap on <see cref="YSpeedTableIndex"/>, from population parameter 2's high byte (0 through 255).</summary>
    public int MaximumYSpeedTableIndex { get; internal set; }

    /// <summary>The previous actor update's rider-selected vertical movement; a change restarts the acceleration.</summary>
    public PlatformVerticalMovement PreviousYMovement { get; internal set; }

    /// <summary>True only for Kamer, the suspensor platform, whose art differs from Tripper's.</summary>
    public bool IsSuspensorPlatform { get; internal set; }

    /// <summary>
    /// Empties the values the cartridge keeps in the enemy's common record, when a kill empties that record: the
    /// resting height, both speeds and the acceleration index read as zero.
    /// </summary>
    internal void ClearWithRecord()
    {
        RestingHeight = Position16<RoomPixels>.Zero;
        RightSpeed = SnesSignedSixteenSixteen.Zero;
        LeftSpeed = SnesSignedSixteenSixteen.Zero;
        YSpeedTableIndex = 0;
    }
}
