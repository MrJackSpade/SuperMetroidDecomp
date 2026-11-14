using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The state of a Yard. Names follow what each value does.</summary>
public sealed class YardEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<YardSegment> Playback { get; set; }

    /// <summary>
    /// Signed 1/256-pixel horizontal and vertical crawl velocity, each axis used either tangentially or as a normal-axis
    /// surface probe according to its movement.
    /// </summary>
    public EightEightVector<RoomPixels> CrawlingVelocity { get; internal set; }

    /// <summary>
    /// Airborne animation facing, selected from the eight surface directions.
    /// </summary>
    public YardAirborneFacing AirborneFacingDirection { get; internal set; }

    /// <summary>The hidden-shell animation for the current orientation; none, the empty-behaviour sentinel, suppresses the look-at hiding transition.</summary>
    internal YardSegment? HidingAnimation { get; set; }

    /// <summary>The missing-surface probes in a row that drop Yard.</summary>
    internal const int MissedProbesBeforeFalling = 4;

    /// <summary>
    /// Consecutive missing-surface probe count, 0 through 4, reset on surface contact and landing; the fourth failure
    /// drops Yard instead of installing another outside-corner turn.
    /// </summary>
    public int ConsecutiveTurnCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, MissedProbesBeforeFalling, value);
    }

    /// <summary>
    /// movement step, which may lag a direction cue by one update and therefore owns dispatch
    /// independently of <see cref="Direction"/>.
    /// </summary>
    public YardMovement? Movement { get; internal set; }

    /// <summary>
    /// The airborne Yard's velocity in pixels per update. Gravity adds an eighth of a pixel to its fall, its sideways
    /// speed slows by a sixteenth of a pixel toward rest while it meets nothing, and a collision reverses each part as the
    /// compatibility services describe; a downward collision lands it while it falls less than three pixels an update.
    /// </summary>
    public FixedVector<RoomPixels> AirborneVelocity { get; internal set; }
    /// <summary>Slope/turn cooldown counter, 0 through 15, reset by adjustment or turning; an unadjusted crawl call whose increment reaches 16 reenables transitions without storing that increment.</summary>
    public int TurnTransitionDisableCounter { get; internal set; }
    /// <summary>Flag selecting the turn-disabled corner-probe definitions, set after slope alignment, a turn, or landing and cleared by the crawl cooldown.</summary>
    public bool TurnTransitionDisabled { get; internal set; }
    /// <summary>The placement's crawl speed, restored after ordinary touch in place of the aggressive post-kick speed.</summary>
    public SignedEightEight IdleCrawlSpeed { get; internal set; }
    /// <summary>The live crawl speed: the placement's, or the aggressive speed while crawling aggressively after a landing.</summary>
    public SignedEightEight CrawlSpeed { get; internal set; }
    /// <summary>The live surface direction; only direction cues or turns publish it.</summary>
    public YardDirection Direction { get; internal set; }

    /// <summary>
    /// The side of its block the Yard holds, which signs the crawl velocity the shared crawler reset gives it after a
    /// landing.
    /// </summary>
    internal CrawlerSurfaceOrientation BlockSide { get; set; }
    /// <summary>Mutually exclusive behavior 0 normal crawling, 1 aggressive crawling, 2 hiding, 3 dropped, 4 kicked, or 5 beam-launched, controlling look-at, solidity, touch, and airborne decisions.</summary>
    public YardBehavior Behavior { get; internal set; }
}
