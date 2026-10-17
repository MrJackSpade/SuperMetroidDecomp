using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>
/// Typed debugger view of the shared wall-crawler state used by Zoomer, Zeela, Sova, Viola,
/// Sciser, Zero, and their variants. Variables A/B/F remain physically owned by the common
/// enemy slot; the four fields after parameter two model the native parallel WRAM tables.
/// </summary>
public sealed class CrawlerEnemyState
{
    /// <summary>Common enemy slot backing attached velocity and AI function variables.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Binds the crawler view to its existing enemy slot.</summary>
    internal CrawlerEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<CrawlerProgram> Program { get; set; }

    /// <summary>
    /// Signed two's-complement 8.8 motion words. On horizontal surfaces X is the tangent and Y
    /// the floor/ceiling-probe direction; on vertical surfaces Y is the tangent and X the
    /// wall-probe direction. Native <c>Crawler.XVelocity</c>/<c>YVelocity</c> at $0FA8,x/$0FAA,x,
    /// projected through common variables A and B.
    /// </summary>
    public EightEightVector<RoomPixels> Velocity
    {
        get => new(new SnesSignedEightEight(_slot.VariableA), new SnesSignedEightEight(_slot.VariableB));
        internal set
        {
            _slot.VariableA = value.X.RawValue;
            _slot.VariableB = value.Y.RawValue;
        }
    }

    /// <summary>Current bank-$A3 indirect crawling or falling target; animation commands install the surface-specific target. Native <c>Crawler.function</c> at $0FB2,x, projected through common variable F.</summary>
    public CrawlerEnemyFunction Function
    {
        get => (CrawlerEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
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
    public CrawlerEnemyFunction NonFallingFunction { get; internal set; }
    /// <summary>
    /// Count of successive updates whose attachment probe misses the surface, 0 through 4 in play: contact and landing
    /// reset it, and the fourth miss selects falling rather than another outside-corner turn. Native $7E:7808,x. The
    /// setter accepts the whole word range so the turn routines' wrapped increment is preserved exactly.
    /// </summary>
    public int ConsecutiveTurnCounter
    {
        get => _consecutiveTurnCounter;
        internal set => _consecutiveTurnCounter = checked((ushort)value);
    }

    /// <summary>Backing count for <see cref="ConsecutiveTurnCounter"/>, always a valid word value.</summary>
    private int _consecutiveTurnCounter;
}
