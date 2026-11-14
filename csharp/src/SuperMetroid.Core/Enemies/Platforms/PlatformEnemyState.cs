using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// The state of one Tripper or Kamer platform: its patrol speeds, the height it rises back to, its vertical
/// acceleration, and the movement and art it has selected.
/// </summary>
public sealed class PlatformEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<PlatformSegment> Playback { get; set; }

    /// <summary>
    /// The height a depressed platform rises back to, one pixel below its population position; only Y is meaningful.
    /// Rising stops once the platform's whole Y lies above it, measured as the wrapped signed difference, so a height
    /// that wrapped past the room's top edge stops the platform as the game does.
    /// </summary>
    public Position16<RoomPixels> RestingHeight { get; internal set; }

    /// <summary>Signed 1/65536-room-pixel displacement per horizontal actor update while patrolling right: the spawn settings' speed.</summary>
    public SignedSixteenSixteen RightSpeed { get; internal set; }

    /// <summary>Signed 1/65536-room-pixel displacement per horizontal actor update while patrolling left: the spawn settings' speed, negated.</summary>
    public SignedSixteenSixteen LeftSpeed { get; internal set; }

    /// <summary>
    /// Whole-row place along the shared quadratically increasing enemy speeds, from the first row to
    /// <see cref="MaximumYSpeedCurvePlace"/>: it starts at the first row, moves on one row per accelerating update and is clamped
    /// at the maximum, so it never leaves that range.
    /// </summary>
    public EnemyQuadraticSpeedIndex YSpeedCurvePlace { get; internal set; }

    /// <summary>The rider-selected vertical movement, recomputed each actor update before horizontal then vertical motion.</summary>
    public PlatformVerticalMovement YMovement { get; internal set; }

    /// <summary>The patrol direction: initialized from its spawn movement and changed by wall collisions or animation cues.</summary>
    public PlatformHorizontalMovement XMovement { get; internal set; }

    /// <summary>Whether moving art has been installed for the current vertical phase.</summary>
    public bool VerticallyMovingArtInstalled { get; internal set; }

    /// <summary>Whether vertically-still art has been installed for the current vertical phase.</summary>
    public bool VerticallyStillArtInstalled { get; internal set; }

    /// <summary>The inclusive cap on <see cref="YSpeedCurvePlace"/>: as many rows on as the spawn settings' vertical acceleration updates.</summary>
    public EnemyQuadraticSpeedIndex MaximumYSpeedCurvePlace { get; internal set; }

    /// <summary>The previous actor update's rider-selected vertical movement; a change restarts the acceleration.</summary>
    public PlatformVerticalMovement PreviousYMovement { get; internal set; }

    /// <summary>True only for Kamer, the suspensor platform, whose art differs from Tripper's.</summary>
    public bool IsSuspensorPlatform { get; internal set; }
}
