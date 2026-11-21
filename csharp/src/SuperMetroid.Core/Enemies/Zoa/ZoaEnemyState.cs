using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// The state of a Zoa, with its launch speed stage. “Shooting” is the game's name for the creature launching itself
/// horizontally; it does not create a separate enemy projectile.
/// </summary>
public sealed partial class ZoaEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal ZoaEnemyState(ZoaAction action) => Action = action;

    /// <summary>
    /// The point the Zoa rose from, in whole pixels, captured at initialization; a launch that ends off screen places it
    /// back there (<see cref="FixedPosition{TSpace}.WithWhole"/>).
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>
    /// The way the Zoa faces: toward Samus as it wakes, rising and then shooting that way; at rest, and again once a shot
    /// ends off screen, it faces left (#2145).
    /// </summary>
    public HorizontalDirection Facing { get; internal set; }

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
