namespace SuperMetroid.Core.Game;

/// <summary>Named substitutes for Crystal Flash's three bank-$90 handler addresses.</summary>
public enum CrystalFlashPhase
{
    /// <summary>No Crystal Flash movement handler is installed; its palette handler may still await the separate restoration pass.</summary>
    Inactive,
    /// <summary>$90:D678, SamusMovementHandler_CrystalFlash_RaiseSamus_GenerateBubble: raises the whole Y word two pixels for ten calls, then captures the raised center and requests bubble HDMA.</summary>
    Raising,
    /// <summary>$90:D6CE, SamusMovementHandler_CrystalFlash_DecrementAmmo: consumes missiles, supers, then power bombs, restoring 50 energy per decrement on every eighth accepted NMI.</summary>
    DrainingAmmo,
    /// <summary>$90:D75B, SamusMovementHandler_CrystalFlash_Finish: increments Y one pixel whenever it differs from the retained raised word and waits for standing movement type before releasing input locks and requesting palette restoration.</summary>
    Finishing,
}
