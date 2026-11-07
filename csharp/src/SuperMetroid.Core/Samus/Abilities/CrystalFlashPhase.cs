namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Named substitutes for Crystal Flash's three handler addresses.</summary>
public enum CrystalFlashPhase
{
    /// <summary>No Crystal Flash movement handler is installed; its palette handler may still await the separate restoration pass.</summary>
    Inactive,
    /// <summary>Samus rises: her whole Y position goes up two pixels for ten calls, then the raised center is captured and the bubble effect is requested.</summary>
    Raising,
    /// <summary>The ammunition drains: missiles, supers, then power bombs are consumed, restoring 50 energy per decrement on every eighth frame.</summary>
    DrainingAmmo,
    /// <summary>Samus settles: Y increases by one pixel whenever it differs from the retained raised position, and the phase waits for the standing movement type before releasing input locks and requesting palette restoration.</summary>
    Finishing,
}
