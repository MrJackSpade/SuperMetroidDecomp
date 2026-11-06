namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Exact values used by the six-entry X-ray dispatcher.</summary>
public enum XrayBeamPhase
{
    /// <summary>State zero, <c>HandleXrayScope_State0_NoBeam</c>; after setup, held dash starts widening and released dash starts restoration.</summary>
    NoBeam = 0,
    /// <summary>State one, <c>HandleXrayScope_State1_BeamIsWidening</c>; accelerates the 16.16 angular half-width until it clamps to ten of the 256 angle units per turn.</summary>
    Widening = 1,
    /// <summary>State two, <c>HandleXrayScope_State2_FullBeam</c>; held dash permits aiming, with Up taking priority over Down, and releasing dash starts restoration.</summary>
    Full = 2,
    /// <summary>State three: the beam is deactivating; waits for artwork queue room to restore the first saved background half.</summary>
    RestoreFirstHalf = 3,
    /// <summary>State four: the beam is still deactivating; waits for artwork queue room to restore the second saved background half before cleanup.</summary>
    RestoreSecondHalf = 4,
    /// <summary>State five, <c>HandleXrayScope_State5_DeactivateBeam_Finish</c>; restores controls and subsystem enables only while shared time is frozen, otherwise the stranded object remains active.</summary>
    Finish = 5,
}
