using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The state of the Wrecked Ship ghost, with its extension words.</summary>
public sealed class WreckedShipGhostEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<WreckedShipGhostProgram> Program { get; set; }

    /// <summary>The current behaviour.</summary>
    public WreckedShipGhostAiFunction? Function { get; internal set; }

    /// <summary>Initial/reappearance delay, flicker interval, or visible lifetime, depending on <see cref="Function"/>.</summary>
    public int PhaseTimer { get; internal set; }

    /// <summary>
    /// The flicker-duration table's byte offset, two per interval, 0 through 65535. It starts at two, steps by two with
    /// each interval and returns to zero at the table's terminator; its second bit selects whether the interval just
    /// started shows the ghost. An offset past the table's seventeen intervals is rejected before it can grow further.
    /// </summary>
    public int FlickerTableOffset
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// The position about which the visible ghost oscillates; only its Y is meaningful. Positions
    /// compare with it by the wrapped spatial difference.
    /// </summary>
    public Position16<RoomPixels> BobbingOrigin { get; internal set; }

    /// <summary>
    /// The visible ghost's signed 16.16 vertical speed in pixels
    /// per update, accelerating toward <see cref="BobbingOrigin"/> with the game's carry and wraparound.
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
    /// The ghost's fading colours and the target its fade moves toward (#1512): the compatibility value whose step
    /// results end the ghost's brightening and fades.
    /// </summary>
    public WreckedShipGhostFade Fade { get; } = new();
}
