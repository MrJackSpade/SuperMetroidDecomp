using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// The state of a Zoa, with its launch speed stage. “Shooting” is the game's name for the creature launching itself
/// horizontally; it does not create a separate enemy projectile.
/// </summary>
public sealed class ZoaEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal ZoaEnemyState(ZoaAction action) => Action = action;

    /// <summary>
    /// Whole-pixel population position captured at initialization and restored when a launch ends offscreen. Only the
    /// whole pixels are reset: the subpixel fractions accumulated in flight survive.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>
    /// Whether the Zoa faces right: toward Samus as it wakes, rising and then shooting that way; at rest, and again once a
    /// shot ends off screen, it faces left (#2145).
    /// </summary>
    public bool FacesRight { get; internal set; }

    /// <summary>
    /// The current AI behaviour, initialized to wait and transitioned through rise, horizontal launch, and offscreen
    /// return to wait.
    /// </summary>
    public ZoaAction Action { get; internal set; }

    /// <summary>The launch's speed in pixels per frame: at rest, then each speed the shooting animation's cues select.</summary>
    public SignedSixteenSixteen LaunchSpeed { get; internal set; }

    /// <summary>Where Zoa's typed animation (#641) resumes.</summary>
    internal AnimationPlayback<ZoaSegment> Playback { get; set; }
}
