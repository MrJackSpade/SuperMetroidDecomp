using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>The state of an Owtch: its patrol speeds, its patrol state and how deep it is buried.</summary>
public sealed class OwtchEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<OwtchSegment> Playback { get; set; }

    /// <summary>
    /// The patrol speed in pixels per update, the spawn settings' speed: added walking right and negated walking left.
    /// </summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>The patrol state, which also indexes the state table.</summary>
    public OwtchBehaviorState Behavior { get; internal set; }

    /// <summary>
    /// How many pixels the Owtch has sunk below its walking height, zero through sixteen while it sinks; set directly
    /// by initialization and the sinking steps. Rising counts it down through <see cref="RiseOnePixel"/>, which can
    /// wrap it.
    /// </summary>
    public int BurialDepth
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, WrappingCounter.LargestCount, value);
    }

    /// <summary>
    /// Counts one rising pixel off the burial depth and reports whether it reached zero. An
    /// Owtch that starts rising unburied wraps to 65535, as the game's decrement does, and keeps rising that many
    /// more updates.
    /// </summary>
    internal bool RiseOnePixel()
    {
        BurialDepth = WrappingCounter.DecrementCount(BurialDepth);
        return BurialDepth == 0;
    }

    /// <summary>The underground wait countdown.</summary>
    public int UndergroundTimer { get; internal set; }

    /// <summary>
    /// The left end of the patrol, the spawn X less the travel distance with
    /// room-coordinate wraparound; only its X is meaningful. Positions compare with it by the wrapped spatial
    /// difference.
    /// </summary>
    public Position16<RoomPixels> MinimumPosition { get; internal set; }

    /// <summary>
    /// The right end of the patrol, the spawn X plus the travel distance with
    /// room-coordinate wraparound; only its X is meaningful. Positions compare with it by the wrapped spatial
    /// difference.
    /// </summary>
    public Position16<RoomPixels> MaximumPosition { get; internal set; }
}
