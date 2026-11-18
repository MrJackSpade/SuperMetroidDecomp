using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The state of the Wrecked Ship ghost: its behaviour, its flicker and its bobbing.</summary>
public sealed class WreckedShipGhostEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal WreckedShipGhostEnemyState(WreckedShipGhostAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WreckedShipGhostSegment> Playback { get; set; }

    /// <summary>The current behaviour.</summary>
    public WreckedShipGhostAction Action { get; internal set; }

    /// <summary>Initial/reappearance delay, flicker interval, or visible lifetime, depending on <see cref="Action"/>.</summary>
    public int PhaseTimer { get; internal set; }

    /// <summary>
    /// The flicker step that starts when the current one's <see cref="PhaseTimer"/> runs out (#2451); none once the last step
    /// has started, whose end ends the flicker. Each appearance and each visible spell starts it at the first step.
    /// </summary>
    public WreckedShipGhostFlickerStep? NextFlickerStep { get; internal set; } = WreckedShipGhostFlickerStep.First;

    /// <summary>
    /// The room Y about which the visible ghost oscillates, where it appeared: 64 pixels above a rising Samus near a
    /// room's top it can lie a few pixels above the room's origin, and the bob compares with it the short way, as
    /// <see cref="CoordinateAxis.Precedes"/> compares rows.
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
    /// <summary>The inclusive lower corner of the nearly-still box (previous position -1).</summary>
    public Position16<RoomPixels> SamusLowerBound { get; internal set; }
    /// <summary>The exclusive upper corner of the nearly-still box (previous position +1).</summary>
    public Position16<RoomPixels> SamusUpperBound { get; internal set; }

    /// <summary>64-frame timer while Samus remains nearly still.</summary>
    public int StablePositionTimer { get; internal set; }

    /// <summary>16-frame timer while Samus keeps one direction.</summary>
    public int StableDirectionTimer { get; internal set; }

    /// <summary>
    /// The ghost's fading colours and the target its fade moves toward (#1512, #1552): the compatibility value the ghost
    /// owns, whose step results end its brightening and fades. A ghost starts with its colours darkened.
    /// </summary>
    public WreckedShipGhostFade Fade { get; internal set; } = WreckedShipGhostFade.Dark;
}
