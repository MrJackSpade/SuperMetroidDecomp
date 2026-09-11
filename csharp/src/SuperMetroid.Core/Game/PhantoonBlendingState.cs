namespace SuperMetroid.Core.Game;

/// <summary>Phantoon's $A7:CE96/$88:E449 HDMA owner and its separately latched display configuration.</summary>
public sealed class PhantoonBlendingState
{
    private int _setupCalls;
    private bool _deleted;
    public LayerBlendingConfiguration Configuration { get; private set; }
    public LayerBlendingConfiguration DisplayedConfiguration { get; private set; }

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

    public void LatchDisplay() => DisplayedConfiguration = Configuration;
}
