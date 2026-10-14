namespace SuperMetroid.Core.Game;

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

    /// <summary>Unsigned fractional falling Y velocity in 1/65536 pixel per AI update, separate from attached 8.8 motion; gravity adds $8000 with carry and landing clears it. Native $7E:7802,x.</summary>
    public ushort FallingYSubvelocity { get; internal set; }
    /// <summary>Signed two's-complement whole-pixel falling Y velocity per AI update, paired with <see cref="FallingYSubvelocity"/> as signed 16.16 and cleared on landing. Native $7E:7804,x.</summary>
    public ushort FallingYVelocity { get; internal set; }
    /// <summary>Attached bank-$A3 AI target saved when an earthquake or repeated outside turns start a fall; restored on terrain landing for either shared crawlers or orange Zoomer. Native $7E:7806,x.</summary>
    public CrawlerEnemyFunction NonFallingFunction { get; internal set; }
    /// <summary>Count of successive updates whose attachment probe misses the surface; contact resets it, and the fourth miss selects falling rather than another outside-corner turn. Native $7E:7808,x.</summary>
    public ushort ConsecutiveTurnCounter { get; internal set; }
}
