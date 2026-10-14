namespace SuperMetroid.Core.Game;

/// <summary>Exact `$0A7A` values used by the six-entry bank-$88 X-ray dispatcher.</summary>
public enum XrayBeamPhase : ushort
{
    /// <summary>State zero, native <c>HandleXrayScope_State0_NoBeam</c> at <c>$88:8732</c>; after setup, held dash starts widening and released dash starts restoration.</summary>
    NoBeam = 0,
    /// <summary>State one, native <c>HandleXrayScope_State1_BeamIsWidening</c> at <c>$88:8754</c>; accelerates the 16.16 angular half-width until it clamps to ten of the 256 angle units per turn.</summary>
    Widening = 1,
    /// <summary>State two, native <c>HandleXrayScope_State2_FullBeam</c> at <c>$88:87AB</c>; held dash permits aiming, with Up taking priority over Down, and releasing dash starts restoration.</summary>
    Full = 2,
    /// <summary>State three, native <c>HandleXrayScope_State3_DeactivateBeam_RestoreBG2_FirstHalf</c> at <c>$88:8934</c>; waits for VRAM queue room to restore the first saved BG2 half.</summary>
    RestoreFirstHalf = 3,
    /// <summary>State four, native <c>HandleXrayScope_State4_DeactivateBeam_RestoreBG2_SecondHalf</c> at <c>$88:89BA</c>; waits for VRAM queue room to restore the second saved BG2 half before cleanup.</summary>
    RestoreSecondHalf = 4,
    /// <summary>State five, native <c>HandleXrayScope_State5_DeactivateBeam_Finish</c> at <c>$88:8A08</c>; restores controls and subsystem enables only while shared time is frozen, otherwise the stranded object remains active.</summary>
    Finish = 5,
}
