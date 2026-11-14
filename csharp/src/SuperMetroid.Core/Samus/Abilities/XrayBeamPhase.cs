namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The six phases of the X-ray beam.</summary>
public enum XrayBeamPhase
{
    /// <summary>State zero, no beam yet; after setup, held dash starts widening and released dash starts restoration.</summary>
    NoBeam = 0,
    /// <summary>State one, the beam widening; accelerates the 1/65536-step angular half-width until it clamps to ten of the 256 angle units per turn.</summary>
    Widening = 1,
    /// <summary>State two, the full beam; held dash permits aiming, with Up taking priority over Down, and releasing dash starts restoration.</summary>
    Full = 2,
    /// <summary>State three: the beam is deactivating; waits for artwork queue room to restore the first saved background half.</summary>
    RestoreFirstHalf = 3,
    /// <summary>State four: the beam is still deactivating; waits for artwork queue room to restore the second saved background half before cleanup.</summary>
    RestoreSecondHalf = 4,
    /// <summary>State five, the beam's deactivation finishing; restores controls and subsystem enables only while shared time is frozen, otherwise the stranded object remains active.</summary>
    Finish = 5,
}
