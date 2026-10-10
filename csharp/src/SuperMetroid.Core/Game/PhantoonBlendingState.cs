namespace SuperMetroid.Core.Game;

/// <summary>Phantoon's $A7:CE96/$88:E449 HDMA owner and its separately latched display configuration.</summary>
/// <remarks>One mutable instance belongs to the encounter. Setup progress and deletion persist for its lifetime; the separate wavy-scroll owner and palette colors are not modified here.</remarks>
public sealed class PhantoonBlendingState
{
    /// <summary>Counts the initial HDMA passes that keep the room's blending configuration before Phantoon's mouth control takes effect.</summary>
    private int _setupCalls;
    /// <summary>Records that the HDMA owner reached native deletion control, after which later passes stop changing its blending result.</summary>
    private bool _deleted;
    /// <summary>Current HDMA-pass result: room default, $04 with BG2 hidden, or $1A with BG2 added on the subscreen; not necessarily the displayed result.</summary>
    public LayerBlendingConfiguration Configuration { get; private set; }
    /// <summary>Configuration copied at the last display latch and consumed by rendering, unaffected by subsequent <see cref="Step"/> calls until latched again.</summary>
    public LayerBlendingConfiguration DisplayedConfiguration { get; private set; }
    /// <summary>The MOSAIC shadow value accepted by NMI, before this frame's enemy AI changes it.</summary>
    public byte DisplayedMosaic { get; private set; }

    /// <summary>
    /// Advances one ordinary-gameplay HDMA pass, resetting to the room default even during the two setup calls or after deletion.
    /// Thereafter $4000 selects semi-transparency; otherwise mouth control $00 hides BG2, $FF hides it and permanently deletes this owner, and other low bytes leave the room default.
    /// </summary>
    /// <param name="boss">Encounter state read without mutation or retention; its mouth must exist when the active, non-semitransparent branch reads its control byte.</param>
    /// <param name="roomDefault">This pass's room blending configuration, not the previous pass's result.</param>
    /// <remarks>Bit $4000 takes precedence over deletion control. This advances setup/deletion state but does not publish display state, write CGRAM, or update the boss.</remarks>
    public void Step(PhantoonEnemyState boss, LayerBlendingConfiguration roomDefault)
    {
        // The outer HDMA handler resets configuration from the room every pass.
        // A nonzero control byte is a no-write branch, not "retain yesterday's mode".
        Configuration = roomDefault;
        if (_deleted) return;
        if (_setupCalls < PhantoonBlendingRomData.SetupCalls) { _setupCalls++; return; }
        if ((boss.SemiTransparencyLayerFlags & PhantoonBlendingRomData.SemiTransparentBit) != 0)
            Configuration = LayerBlendingConfiguration.PhantoonSemiTransparent;
        else if ((byte)boss.Mouth!.Parameter1 is 0 or PhantoonBlendingRomData.DeleteControl)
        {
            Configuration = LayerBlendingConfiguration.PhantoonHidden;
            // The pre-instruction advances sleep to delete with timer one, so the
            // instruction handler removes this owner on this same call.
            _deleted = (byte)boss.Mouth.Parameter1 == PhantoonBlendingRomData.DeleteControl;
        }
    }

    /// <summary>Copies live blending and the supplied MOSAIC byte to displayed state at the accepted-NMI boundary, without advancing the HDMA owner.</summary>
    /// <param name="mosaic">Raw $2106 shadow byte; BG2 enable is bit 1 and the high nibble encodes block size minus one (1..16 pixels). Zero disables mosaic.</param>
    public void LatchDisplay(byte mosaic = 0)
    {
        DisplayedConfiguration = Configuration;
        DisplayedMosaic = mosaic;
    }
}
