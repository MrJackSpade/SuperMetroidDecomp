namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The phases of the X-ray beam, once its warm-up is over.</summary>
public enum XrayBeamPhase
{
    /// <summary>No beam yet; the beam's first step widens it while Run is held and starts its closing once Run is released.</summary>
    NoBeam,
    /// <summary>The beam widening; accelerates the 1/65536-step angular half-width until it clamps to ten of the 256 angle units per turn.</summary>
    Widening,
    /// <summary>The full beam; held Run permits aiming, with Up taking priority over Down, and releasing Run starts the beam's closing.</summary>
    Full,
    /// <summary>The beam closing after Run's release: time stays frozen and Samus held for the <see cref="XrayClosing"/>'s updates.</summary>
    Closing,
    /// <summary>The beam's last step; releases Samus and restores controls and subsystem enables only while shared time is frozen, otherwise the stranded beam remains active.</summary>
    Finish,
}
