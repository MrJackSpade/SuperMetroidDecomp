using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's $A7:CE96/$88:E449 HDMA owner and its separately latched display configuration.</summary>
/// <remarks>One mutable instance belongs to the encounter. Setup progress and deletion persist for its lifetime; the separate wavy-scroll owner and palette colors are not modified here.</remarks>
public sealed class PhantoonBlendingState
{
    /// <summary>Number of initial HDMA passes consumed before Phantoon's blend pre-instruction becomes active.</summary>
    private int _setupCalls;
    /// <summary>Permanent deletion state after the mouth control byte requests removal of this HDMA owner.</summary>
    private bool _deleted;
    /// <summary>Current HDMA-pass result: room default, $04 with BG2 hidden, or $1A with BG2 added on the subscreen; not necessarily the displayed result.</summary>
    public LayerBlendingConfiguration Configuration { get; private set; } = LayerBlendingConfiguration.Unconfigured;
    /// <summary>Configuration copied at the last display latch and consumed by rendering, unaffected by subsequent <see cref="Step"/> calls until latched again.</summary>
    public LayerBlendingConfiguration DisplayedConfiguration { get; private set; } = LayerBlendingConfiguration.Unconfigured;
    /// <summary>The BG2 mosaic block size in screen pixels accepted by NMI, before this frame's enemy AI changes it; one means no quantization.</summary>
    public int DisplayedMosaicBlockSize { get; private set; } = 1;

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
        if (boss.SemiTransparent)
            Configuration = LayerBlendingConfiguration.PhantoonSemiTransparent;
        else if ((byte)boss.Parts[boss.Mouth.SlotIndex].BlendingControl is 0 or PhantoonBlendingRomData.DeleteControl)
        {
            Configuration = LayerBlendingConfiguration.PhantoonHidden;
            // The pre-instruction advances sleep to delete with timer one, so the
            // instruction handler removes this owner on this same call.
            _deleted = (byte)boss.Parts[boss.Mouth.SlotIndex].BlendingControl == PhantoonBlendingRomData.DeleteControl;
        }
    }

    /// <summary>Copies live blending and the supplied BG2 mosaic size to displayed state at the accepted-NMI boundary, without advancing the HDMA owner.</summary>
    /// <param name="mosaicBlockSize">BG2 mosaic block size in screen pixels, 1..16; one disables quantization.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mosaicBlockSize"/> is outside 1..16.</exception>
    public void LatchDisplay(int mosaicBlockSize = 1)
    {
        if (mosaicBlockSize is < 1 or > MaximumMosaicBlockSize)
            throw new ArgumentOutOfRangeException(nameof(mosaicBlockSize), mosaicBlockSize, "Mosaic blocks are one through sixteen pixels.");
        DisplayedConfiguration = Configuration;
        DisplayedMosaicBlockSize = mosaicBlockSize;
    }

    /// <summary>The largest mosaic block, in screen pixels.</summary>
    private const int MaximumMosaicBlockSize = 16;
}
