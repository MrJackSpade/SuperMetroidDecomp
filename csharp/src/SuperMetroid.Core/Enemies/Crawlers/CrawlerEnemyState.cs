using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The shared wall-crawler state used by Zoomer, Zeela, Sova, Viola, Sciser, Zero, and their variants. The attached
/// velocity and behaviour are values the game keeps in the enemy's common record, so a kill empties them with
/// that record (<see cref="ClearWithRecord"/>); the fields after them model the crawler's parallel tables.
/// </summary>
public sealed class CrawlerEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<CrawlerProgram> Program { get; set; }

    /// <summary>
    /// Signed two's-complement 8.8 motion words. On horizontal surfaces X is the tangent and Y the floor/ceiling-probe
    /// direction; on vertical surfaces Y is the tangent and X the wall-probe direction.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }

    /// <summary>
    /// The current crawling or falling behaviour; animation commands install the surface-specific one. None once a
    /// kill has emptied the record.
    /// </summary>
    public CrawlerEnemyFunction? Function { get; internal set; }

    /// <summary>Empties the values the game keeps in the enemy's common record: no velocity and no behaviour.</summary>
    internal void ClearWithRecord()
    {
        Velocity = default;
        Function = null;
        TakesDefaultSpeedOnLanding = false;
        SurfaceFamily = CrawlerAnimationFamily.Shared;
    }

    /// <summary>
    /// Whether the crawler was placed without a crawl speed, so it keeps its velocity until its first landing and takes
    /// the default speed then. A kill empties it.
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
    /// One falling update's gravity: adds half a pixel per update while the whole pixels, compared as a signed word,
    /// are below four.
    /// </summary>
    internal void ApplyFallingGravity()
    {
        if (WordWrap.Signed(FallingYVelocity.WholeWord - 4) < 0)
            FallingYVelocity += SignedSixteenSixteen.FromRaw(SixteenSixteen.FromWhole(1) / 2);
    }
    /// <summary>Attached AI target saved when an earthquake or repeated outside turns start a fall; restored on terrain landing for either shared crawlers or orange Zoomer.</summary>
    public CrawlerEnemyFunction? NonFallingFunction { get; internal set; }
    /// <summary>
    /// Count of successive updates whose attachment probe misses the surface, 0 through 4 in play: contact and landing
    /// reset it, and the fourth miss selects falling rather than another outside-corner turn. The
    /// setter accepts the whole word range so the turn routines' wrapped increment is preserved exactly.
    /// </summary>
    public int ConsecutiveTurnCounter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
}
