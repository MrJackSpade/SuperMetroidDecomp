using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// The state of the Wrecked Ship ghost: its behaviour, a countdown for each of its timed phases (#2512), its flicker and its
/// bobbing.
/// </summary>
public sealed class WreckedShipGhostEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal WreckedShipGhostEnemyState(WreckedShipGhostAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WreckedShipGhostSegment> Playback { get; set; }

    /// <summary>The current behaviour.</summary>
    public WreckedShipGhostAction Action { get; internal set; }

    /// <summary>
    /// The hidden updates left before the ghost watches Samus (<see cref="WreckedShipGhostAction.InitialInvisibleDelay"/>):
    /// its first delay once placed, and its reappearance delay after each vanishing. The update it runs out on starts the
    /// watch.
    /// </summary>
    public UpdateCountdown InvisibleDelay { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The updates left before the flicker's next step starts while the ghost brightens and fades in (#2512): the update it
    /// runs out on starts the step. Zero once the last step has run out, when the flicker has ended.
    /// </summary>
    public UpdateCountdown FlickerTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The flicker step that starts when <see cref="FlickerTimer"/> runs out (#2451); none once the last step has started,
    /// whose end ends the flicker. Each appearance starts the flicker before its first step. The timer is the running step's
    /// time and this the step after it, two facts rather than one choice encoded twice (#2560): a running flicker always has
    /// a time left, and only its last step has no step after it.
    /// </summary>
    public WreckedShipGhostFlickerStep? NextFlickerStep { get; internal set; }

    /// <summary>Whether the appearance's flicker still runs: its last step has not yet run out.</summary>
    internal bool IsFlickering => FlickerTimer.IsRunning;

    /// <summary>
    /// The updates left of the ghost's visible bob (<see cref="WreckedShipGhostAction.BobbingWhileVisible"/>): the update it
    /// runs out on starts the ghost vanishing.
    /// </summary>
    public UpdateCountdown VisibleTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The room Y about which the visible ghost oscillates, where it appeared: above a rising Samus near a
    /// room's top it can lie a few pixels above the room's origin, and the bob compares with it the short way, as
    /// <see cref="DistanceAcrossRoomEdge.LiesBefore"/> compares rows.
    /// </summary>
    public int BobbingOriginY { get; internal set; }

    /// <summary>
    /// The visible ghost's vertical speed each update, accelerating toward
    /// <see cref="BobbingOriginY"/>.
    /// </summary>
    public SignedSixteenSixteen VerticalVelocity { get; internal set; }

    /// <summary>How Samus has been moving on each axis, which picks where around her the ghost appears.</summary>
    public SamusMotion SamusMotion { get; internal set; }

    /// <summary>Samus's previous whole-pixel position.</summary>
    public Position16<RoomPixels> PreviousSamusPosition { get; internal set; }
    /// <summary>The inclusive lower corner of the nearly-still box: the previous position less <see cref="WreckedShipGhostAppearanceDefinitions.StillMargin"/>.</summary>
    public Position16<RoomPixels> SamusLowerBound { get; internal set; }
    /// <summary>The exclusive upper corner of the nearly-still box: the previous position plus <see cref="WreckedShipGhostAppearanceDefinitions.StillMargin"/>.</summary>
    public Position16<RoomPixels> SamusUpperBound { get; internal set; }

    /// <summary>Counts down from <see cref="WreckedShipGhostAppearanceDefinitions.StillPositionDuration"/> while Samus remains nearly still.</summary>
    public UpdateCountdown StablePositionTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>Counts down from <see cref="WreckedShipGhostAppearanceDefinitions.SteadyFacingDuration"/> while Samus keeps one direction.</summary>
    public UpdateCountdown StableDirectionTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// The ghost's fading colours and the target its fade moves toward (#1512, #1552): the compatibility value the ghost
    /// owns, whose step results end its brightening and fades. A ghost starts with its colours darkened.
    /// </summary>
    public WreckedShipGhostFade Fade { get; internal set; } = WreckedShipGhostFade.Dark;
}
