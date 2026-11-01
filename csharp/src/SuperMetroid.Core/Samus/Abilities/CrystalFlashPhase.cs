namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Named substitutes for Crystal Flash's three handler addresses.</summary>
public enum CrystalFlashPhase
{
    /// <summary>No Crystal Flash movement handler is installed; its palette handler may still await the separate restoration pass.</summary>
    Inactive,
    /// <summary>SamusMovementHandler_CrystalFlash_RaiseSamus_GenerateBubble: raises the whole Y word two pixels for ten calls, then captures the raised center and requests the bubble effect.</summary>
    Raising,
    /// <summary>SamusMovementHandler_CrystalFlash_DecrementAmmo: consumes missiles, supers, then power bombs, restoring 50 energy per decrement on every eighth frame.</summary>
    DrainingAmmo,
    /// <summary>SamusMovementHandler_CrystalFlash_Finish: increments Y one pixel whenever it differs from the retained raised word and waits for standing movement type before releasing input locks and requesting palette restoration.</summary>
    Finishing,
}
