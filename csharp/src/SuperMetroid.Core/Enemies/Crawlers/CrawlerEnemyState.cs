using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The shared wall-crawler state used by Zoomer, Zeela, Sova, Viola, Sciser, Zero, and their variants.
/// </summary>
public sealed class CrawlerEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<CrawlerSegment> Playback { get; set; }

    /// <summary>
    /// Signed 1/256-pixel motion. On horizontal surfaces X is the tangent and Y the floor/ceiling-probe
    /// direction; on vertical surfaces Y is the tangent and X the wall-probe direction.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The current crawling or falling behaviour; animation cues install the surface-specific one.
    /// </summary>
    public CrawlerAction? Action { get; internal set; }

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
    /// Signed 1/65536-pixel falling Y velocity per AI update, separate from attached 1/256-pixel motion: gravity adds half a pixel per
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
    public CrawlerAction? NonFallingAction { get; internal set; }
    /// <summary>The number of successive missed surface probes at which the crawler stops turning and falls.</summary>
    internal const int MissedProbesBeforeFalling = 4;

    /// <summary>
    /// Count of successive updates whose attachment probe misses the surface, 0 through
    /// <see cref="MissedProbesBeforeFalling"/>: contact and landing reset it, and the fourth miss selects falling rather
    /// than another outside-corner turn.
    /// </summary>
    public int ConsecutiveTurnCounter
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }

    /// <summary>
    /// Counts one more missed surface probe and reports whether the crawler has now missed enough in a row to fall
    /// instead of turning the outside corner.
    /// </summary>
    internal bool MissedSurfaceTooOften()
    {
        ConsecutiveTurnCounter++;
        return ConsecutiveTurnCounter >= MissedProbesBeforeFalling;
    }
}
