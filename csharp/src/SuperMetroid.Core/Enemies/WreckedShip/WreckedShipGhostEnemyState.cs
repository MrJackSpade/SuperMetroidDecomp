using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The state of the Wrecked Ship ghost, with its extra values.</summary>
public sealed class WreckedShipGhostEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WreckedShipGhostProgram> Playback { get; set; }

    /// <summary>The current behaviour.</summary>
    public WreckedShipGhostAiFunction? Function { get; internal set; }

    /// <summary>Initial/reappearance delay, flicker interval, or visible lifetime, depending on <see cref="Function"/>.</summary>
    public int PhaseTimer { get; internal set; }

    /// <summary>
    /// The flicker step that starts when the current one's <see cref="PhaseTimer"/> runs out, a position in
    /// <see cref="WreckedShipGhostAppearanceDefinitions.FlickerSteps"/>; the position past the last step ends the flicker.
    /// Each appearance and each visible spell starts it at the first step.
    /// </summary>
    public int NextFlickerStep
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, WreckedShipGhostAppearanceDefinitions.FlickerSteps.Count, value);
    }

    /// <summary>
    /// The position about which the visible ghost oscillates; only its Y is meaningful. Positions
    /// compare with it by the wrapped spatial difference.
    /// </summary>
    public Position16<RoomPixels> BobbingOrigin { get; internal set; }

    /// <summary>
    /// The visible ghost's signed 16.16 vertical speed in pixels
    /// per update, accelerating toward <see cref="BobbingOrigin"/> with the fraction's overflow and wraparound.
    /// </summary>
    public SignedSixteenSixteen VerticalVelocity { get; internal set; }

    /// <summary>
    /// Samus's horizontal movement class, 0 moving left, 1 stationary or 2 moving right. With
    /// <see cref="SpawnRow"/> it selects one of the nine spawn offsets.
    /// </summary>
    public int SpawnColumn { get; internal set; }

    /// <summary>Samus's vertical movement class, 0 moving up, 1 stationary or 2 moving down.</summary>
    public int SpawnRow { get; internal set; }

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
