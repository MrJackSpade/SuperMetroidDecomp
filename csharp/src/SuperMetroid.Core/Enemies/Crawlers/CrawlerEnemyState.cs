using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// The shared wall-crawler state used by Zoomer, Zeela, Sova, Viola, Sciser, Zero, and their variants. The attached
/// velocity and behaviour are values the cartridge keeps in the enemy's common record, so a kill empties them with
/// that record (<see cref="ClearWithRecord"/>); the fields after them model the native parallel WRAM tables.
/// </summary>
public sealed class CrawlerEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
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

    /// <summary>Empties the values the cartridge keeps in the enemy's common record: no velocity and no behaviour.</summary>
    internal void ClearWithRecord()
    {
        Velocity = default;
        Function = null;
        SpeedParameter = 0;
        SurfaceTableOffset = 0;
    }

    /// <summary>
    /// The authored crawl speed, parameter one: an index into the speed table, the preserve-velocity selector, or 255
    /// for the default speed after the first landing.
    /// </summary>
    public int SpeedParameter
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// The species' byte offset into the six-word surface-program tables (parameter two): the species wrapper's offset,
    /// or the authored one when the wrapper gives none.
    /// </summary>
    public int SurfaceTableOffset
    {
        get;
        internal set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }

    /// <summary>
    /// Signed 16.16 falling Y velocity per AI update, separate from attached 8.8 motion: gravity adds half a pixel per
    /// update while the whole pixels are below four, and landing clears it.
    /// </summary>
    public SnesSignedSixteenSixteen FallingYVelocity { get; internal set; }

    /// <summary>
    /// One falling update's gravity: adds half a pixel per update while the whole pixels, compared as a signed word,
    /// are below four.
    /// </summary>
    internal void ApplyFallingGravity()
    {
        if (unchecked((short)(FallingYVelocity.WholeWord - 4)) < 0)
            FallingYVelocity += SnesSignedSixteenSixteen.FromRaw(SnesSixteenSixteen.FromWhole(1) / 2);
    }
    /// <summary>Attached bank-$A3 AI target saved when an earthquake or repeated outside turns start a fall; restored on terrain landing for either shared crawlers or orange Zoomer. Native $7E:7806,x.</summary>
    public CrawlerEnemyFunction? NonFallingFunction { get; internal set; }
    /// <summary>
    /// Count of successive updates whose attachment probe misses the surface, 0 through 4 in play: contact and landing
    /// reset it, and the fourth miss selects falling rather than another outside-corner turn. Native $7E:7808,x. The
    /// setter accepts the whole word range so the turn routines' wrapped increment is preserved exactly.
    /// </summary>
    public int ConsecutiveTurnCounter
    {
        get;
        internal set => field = checked((ushort)value);
    }
}
