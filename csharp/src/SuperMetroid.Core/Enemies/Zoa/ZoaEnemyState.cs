using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>
/// The state of a Zoa, with its launch speed stage. “Shooting” is the game's name for the creature launching itself
/// horizontally; it does not create a separate enemy projectile.
/// </summary>
public sealed class ZoaEnemyState
{
    /// <summary>
    /// Whole-pixel population position captured at initialization and restored when a launch ends offscreen. Only the
    /// whole pixels are reset: the subpixel fractions accumulated in flight survive.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition { get; internal set; }

    /// <summary>The animation the Zoa's behaviour asks for: rising toward Samus, then shooting with the same facing.</summary>
    public ZoaSegment RequestedAnimation { get; internal set; }

    /// <summary>
    /// The requested animation the Zoa last started, at first the left-facing shooting it rests in; asking for it again
    /// leaves the playing animation running.
    /// </summary>
    public ZoaSegment InstalledAnimation { get; internal set; }

    /// <summary>
    /// The current AI behaviour, initialized to wait and transitioned through rise, horizontal launch, and offscreen
    /// return to wait.
    /// </summary>
    public ZoaAction? Action { get; internal set; }

    /// <summary>The launch's speed in pixels per frame: at rest, then each speed the shooting animation's cues select.</summary>
    public SignedSixteenSixteen LaunchSpeed { get; internal set; }

    /// <summary>Where Zoa's typed animation (#641) resumes.</summary>
    internal AnimationPlayback<ZoaSegment> Playback { get; set; }
}
