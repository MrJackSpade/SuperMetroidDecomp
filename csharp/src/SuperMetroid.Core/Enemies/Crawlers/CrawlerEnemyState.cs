using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The shared wall-crawler state used by Zoomer, Zeela, Sova, Viola, Sciser, Zero, and their variants.
/// </summary>
public sealed class CrawlerEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<CrawlerSegment> Playback { get; set; }

    /// <summary>
    /// Signed 8.8 motion. On horizontal surfaces X is the tangent and Y the floor/ceiling-probe
    /// direction; on vertical surfaces Y is the tangent and X the wall-probe direction.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The current crawling or falling behaviour; animation cues install the surface-specific one.
    /// </summary>
    public CrawlerEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// Whether the crawler was placed without a crawl speed, so it keeps its velocity until its first landing and takes
    /// the default speed then.
    /// </summary>
    public bool TakesDefaultSpeedOnLanding { get; internal set; }

    /// <summary>
    /// The family whose animations the crawler switches between when it turns a corner: the species' own, or the one
    /// its population entry names when the species gives none.
    /// </summary>
    public CrawlerAnimationFamily SurfaceFamily { get; internal set; }

    /// <summary>
    /// Signed 16.16 falling Y velocity per AI update, separate from attached 8.8 motion: gravity adds half a pixel per
    /// update while the whole pixels are below four, and landing clears it.
    /// </summary>
    public SignedSixteenSixteen FallingYVelocity { get; internal set; }

    /// <summary>
    /// One falling update's gravity: adds half a pixel per update while the whole pixels, compared as signed,
    /// are below four.
    /// </summary>
    internal void ApplyFallingGravity()
    {
        if (FallingYVelocity.WholePixels < 4)
            FallingYVelocity += SignedSixteenSixteen.FromParts(0, 32768);
    }
    /// <summary>Attached AI target saved when an earthquake or repeated outside turns start a fall; restored on terrain landing for either shared crawlers or orange Zoomer.</summary>
    public CrawlerEnemyFunction? NonFallingFunction { get; internal set; }
    /// <summary>
    /// Count of successive updates whose attachment probe misses the surface, 0 through 4 in play: contact and landing
    /// reset it, and the fourth miss selects falling rather than another outside-corner turn. The
    /// setter accepts every count to 65535 so the turns' wrapped increment is preserved exactly.
    /// </summary>
    public int ConsecutiveTurnCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, WrappingCounter.LargestCount, value);
    }
}
