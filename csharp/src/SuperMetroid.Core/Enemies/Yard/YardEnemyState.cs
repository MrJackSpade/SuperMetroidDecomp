using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>The state of a Yard. Names follow what each value does.</summary>
public sealed class YardEnemyState
{
    /// <summary>Starts the Yard holding still, as it is placed in the room.</summary>
    internal YardEnemyState() => Motion = YardMotion.HoldingStill;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<YardSegment> Playback { get; set; }

    /// <summary>
    /// The crawl velocity in pixels per update, each axis used either along the surface or as a probe into it according
    /// to its movement.
    /// </summary>
    public PixelVelocity<RoomPixels> CrawlingVelocity { get; internal set; }

    /// <summary>
    /// Airborne animation facing, selected from the eight surface directions.
    /// </summary>
    public HorizontalDirection AirborneFacingDirection { get; internal set; }

    /// <summary>The animation the Yard hides in its shell with when Samus looks at it, for its current surface; none where it does not hide.</summary>
    internal YardSegment? HidingAnimation { get; set; }

    /// <summary>The missing-surface probes in a row that drop Yard.</summary>
    internal const int MissedProbesBeforeFalling = 4;

    /// <summary>
    /// Consecutive missing-surface probe count, reset on surface contact and landing; the failure past
    /// <see cref="MissedProbesBeforeFalling"/> drops Yard instead of installing another outside-corner turn.
    /// </summary>
    public int ConsecutiveTurnCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, MissedProbesBeforeFalling, value);
    }

    /// <summary>
    /// The Yard's movement step, which may lag a direction cue by one update and so is chosen independently of
    /// <see cref="Direction"/>: the movement its <see cref="Motion"/> is.
    /// </summary>
    public YardMovement Movement => Motion.Movement;

    /// <summary>How the Yard moves, holding what its movement needs: an airborne Yard holds the animation it lands with.</summary>
    internal YardMotion Motion { get; set; }

    /// <summary>
    /// The way the Yard crawls while its <see cref="Movement"/> is <see cref="YardMovement.Crawling"/>: the surface and way
    /// of the crawl its animation last began (#2278), which may lag <see cref="Direction"/> by one update.
    /// </summary>
    public YardDirection Crawl { get; internal set; }

    /// <summary>
    /// The airborne Yard's velocity in pixels per update. Gravity adds to its fall, its sideways speed slows toward rest
    /// while it meets nothing, and a collision reverses each part as the compatibility services describe; a downward
    /// collision lands it while it falls slower than its landing limit.
    /// </summary>
    public FixedVector<RoomPixels> AirborneVelocity { get; internal set; }
    /// <summary>
    /// The slope-free crawl passes before the Yard may turn at a corner again, the one that frees it included (#2450): a
    /// slope alignment, a turn or a landing starts <see cref="YardMotionDefinitions.TurnSuppressionPasses"/> of them, each
    /// unadjusted crawl pass counts one, and once they are over it may turn.
    /// </summary>
    public UpdateCountdown TurnSuppressionPassesLeft { get; private set; } = UpdateCountdown.Ended();

    /// <summary>Whether the Yard may not yet turn at a corner, crawling past it instead (<see cref="TurnSuppressionPassesLeft"/>).</summary>
    internal bool TurnsSuppressed => TurnSuppressionPassesLeft.IsRunning;

    /// <summary>Starts a whole run of slope-free passes before the Yard may turn again.</summary>
    internal void SuppressTurns() => TurnSuppressionPassesLeft = UpdateCountdown.Lasting(YardMotionDefinitions.TurnSuppressionPasses);

    /// <summary>The live crawl speed: the placement's, or the aggressive speed while crawling aggressively after a landing.</summary>
    public SignedEightEight CrawlSpeed { get; internal set; }
    /// <summary>The live surface direction; only direction cues or turns publish it.</summary>
    public YardDirection Direction { get; internal set; }

    /// <summary>
    /// The side of its block the Yard holds, which signs the crawl velocity the shared crawler reset gives it after a
    /// landing.
    /// </summary>
    internal CrawlerSurfaceOrientation BlockSide { get; set; }
    /// <summary>Whether the Yard crawls normally or aggressively, hides, has been dropped, kicked or launched by a beam, which decides how it watches Samus, whether it is solid, how it meets her and how it flies.</summary>
    public YardBehavior Behavior { get; internal set; }
}
