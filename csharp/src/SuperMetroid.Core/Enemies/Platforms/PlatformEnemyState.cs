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

    /// <summary>The patrol speed in pixels per update, the spawn settings' speed: added patrolling right and negated patrolling left.</summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>
    /// Whole-mark place along the shared quadratically increasing enemy speeds, from the curve's start to
    /// <see cref="MaximumYSpeedCurvePlace"/>: it starts at the curve's start, moves on one mark per accelerating update and is clamped
    /// at the maximum, so it never leaves that range.
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlace { get; internal set; }

    /// <summary>The rider-selected vertical movement, recomputed each actor update before horizontal then vertical motion.</summary>
    public PlatformVerticalMovement YMovement { get; internal set; }

    /// <summary>The patrol direction: initialized from its spawn movement and changed by wall collisions or animation cues.</summary>
    public PlatformHorizontalMovement XMovement { get; internal set; }

    /// <summary>Whether moving art has been installed for the current vertical phase.</summary>
    public bool VerticallyMovingArtInstalled { get; internal set; }

    /// <summary>Whether vertically-still art has been installed for the current vertical phase.</summary>
    public bool VerticallyStillArtInstalled { get; internal set; }

    /// <summary>The inclusive cap on <see cref="YSpeedCurvePlace"/>: as many marks on as the spawn settings' vertical acceleration updates.</summary>
    public QuadraticSpeedCurvePlace MaximumYSpeedCurvePlace { get; internal set; }

    /// <summary>The previous actor update's rider-selected vertical movement; a change restarts the acceleration.</summary>
    public PlatformVerticalMovement PreviousYMovement { get; internal set; }

    /// <summary>True only for Kamer, the suspensor platform, whose art differs from Tripper's.</summary>
    public bool IsSuspensorPlatform { get; internal set; }
}
