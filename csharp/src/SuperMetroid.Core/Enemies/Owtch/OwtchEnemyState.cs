using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>The state of an Owtch, with its three extended words.</summary>
public sealed class OwtchEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<OwtchProgram> Program { get; set; }

    /// <summary>
    /// The signed 16.16 rightward patrol speed in pixels per update, taken from the shared linear-speed table; the
    /// record keeps its fraction in variable 0 and its whole pixels in variable 1.
    /// </summary>
    public SignedSixteenSixteen RightVelocity { get; internal set; }

    /// <summary>
    /// The signed 16.16 leftward patrol speed in pixels per update, the negative half of the same linear-speed record;
    /// the record keeps its fraction in variable 2 and its whole pixels in variable 3.
    /// </summary>
    public SignedSixteenSixteen LeftVelocity { get; internal set; }

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
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// Counts one rising pixel off the burial depth and reports whether it reached zero. An
    /// Owtch that starts rising unburied wraps to 65535, as the game's decrement does, and keeps rising that many
    /// more updates.
    /// </summary>
    internal bool RiseOnePixel()
    {
        BurialDepth = WrappingCounter.DecrementWord(BurialDepth);
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
