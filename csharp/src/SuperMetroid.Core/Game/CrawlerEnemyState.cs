using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A3 function pointer stored in a creepy-crawly enemy's native variable F.</summary>
public enum CrawlerEnemyFunction : ushort
{
    /// <summary>$A3:E08A: return-only initial orange-Zoomer state, awaiting the animation command that installs a horizontal or vertical crawling function.</summary>
    HZoomerInstructionPending = 0xe08a,
    /// <summary>$A3:E091, <c>Function_HZoomer_CrawlingVertically</c>: maintains wall contact and traverses vertical edges, reversing its tangent toward Samus while attached.</summary>
    HZoomerCrawlingVertically = 0xe091,
    /// <summary>$A3:E168, <c>Function_HZoomer_CrawlingHorizontally</c>: maintains floor or ceiling contact and traverses edges, reversing its horizontal tangent toward Samus while attached.</summary>
    HZoomerCrawlingHorizontally = 0xe168,
    /// <summary>$A3:E6C1: return-only shared-crawler initial state, awaiting the surface animation's command to install the crawling function.</summary>
    InstructionPending = 0xe6c1,
    /// <summary>$A3:E6C8: shared wall-crawling routine that probes horizontal attachment, advances vertically, and rotates the surface animation at inside or outside corners.</summary>
    CrawlingVertically = 0xe6c8,
    /// <summary>$A3:E785: shared detachment fall with half-pixel downward acceleration until the whole velocity reaches four pixels per update; landing clears fall motion and restores the saved attached function.</summary>
    Falling = 0xe785,
    /// <summary>$A3:E7F2: shared floor/ceiling-crawling routine that probes vertical attachment, advances horizontally with slope adjustment, and rotates the surface animation at corners.</summary>
    CrawlingHorizontally = 0xe7f2,
}

/// <summary>
/// Typed debugger view of the shared wall-crawler state used by Zoomer, Zeela, Sova, Viola,
/// Sciser, Zero, and their variants. Variables A/B/F remain physically owned by the common
/// enemy slot; the four fields after parameter two model the native parallel WRAM tables.
/// </summary>
public sealed class CrawlerEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new crawler enemy state instance.</summary>
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
