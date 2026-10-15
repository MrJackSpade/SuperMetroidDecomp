namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Reachable third-phase neck function pointers at `$A9:C330-$C3EE`.</summary>
public enum MotherBrainPhase3NeckPhase
{
    /// <summary>No phase-three neck sub-function is active.</summary>
    Inactive,
    /// <summary>Tracks the ordinary phase-three head posture without recoil displacement.</summary>
    Normal,
    /// <summary>Initializes recovery from an ordinary weapon-hit neck recoil.</summary>
    SetupRecoilRecovery,
    /// <summary>Interpolates the recoiled neck back to its normal combat posture.</summary>
    RecoilRecovery,
    /// <summary>Initializes the stronger recoil caused by Samus's Hyper Beam.</summary>
    SetupHyperBeamRecoil,
    /// <summary>Runs the Hyper Beam recoil displacement and recovery timing.</summary>
    HyperBeamRecoil,
}
