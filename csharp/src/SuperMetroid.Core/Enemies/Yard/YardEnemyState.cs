using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The state of a Yard. Names follow what each value does.</summary>
public sealed class YardEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<YardProgram> Playback { get; set; }

    /// <summary>
    /// Signed 8.8 horizontal and vertical crawl velocity, each axis used either tangentially or as a normal-axis
    /// surface probe according to the movement function.
    /// </summary>
    public EightEightVector<RoomPixels> CrawlingVelocity { get; internal set; }

    /// <summary>
    /// Airborne animation facing, selected from the eight surface-direction records.
    /// </summary>
    public YardAirborneFacing AirborneFacingDirection { get; internal set; }

    /// <summary>The hidden-shell animation for the current orientation; none, the empty-behaviour sentinel, suppresses the look-at hiding transition.</summary>
    internal YardProgram? HidingProgram { get; set; }

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
    /// movement dispatcher function, which may lag a direction cue by one update and therefore owns dispatch
    /// independently of <see cref="Direction"/>.
    /// </summary>
    public YardMovementFunction? MovementFunction { get; internal set; }

    /// <summary>
    /// Extra values zero through three: signed 16.16 airborne velocity in pixels per AI update (Y fraction/whole,
    /// then X fraction/whole). Gravity adds 0.125 to Y, horizontal speed decays by 0.0625 toward zero
    /// when not colliding, and collisions negate each part separately; downward collision lands when the Y whole part
    /// is 0..2.
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
    /// <summary>Parallel flag: set by a horizontal airborne collision and cleared by a vertical bounce; a record of bounce state rather than a per-frame event.</summary>
    public bool BouncedHorizontally { get; internal set; }
}
