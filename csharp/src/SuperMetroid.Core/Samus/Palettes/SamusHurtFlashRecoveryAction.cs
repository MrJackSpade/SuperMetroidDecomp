namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Counter-forty audio recovery selected by native movement/grapple state.</summary>
public enum SamusHurtFlashRecoveryAction
{
    /// <summary>No recovery sound or deferred charging request was selected for the current movement/grapple state.</summary>
    None,
    /// <summary>$91:D94C's Samus command $1C selects the translated ordinary spin/wall-jump sound, library one $31, with queue threshold nine.</summary>
    SpinJumpSound,
    /// <summary>The spin-recovery command selects library-one Space Jump sound $3E from Samus's pose or wall-jump animation frame.</summary>
    SpaceJumpSound,
    /// <summary>The spin-recovery command selects library-one Screw Attack sound $33 from Samus's pose or wall-jump animation frame.</summary>
    ScrewAttackSound,
    /// <summary>$91:D944 queues library-one sound $06 with threshold nine while Grapple is firing, connected, wall-grabbing, or releasing a wall grab.</summary>
    GrappleSound,
    /// <summary>$91:D938 sets the resume-charging latch when the flare count is at least $10 and X is held; the post-draw consumer may later queue sound $41.</summary>
    ResumeChargingBeamRequested,
}
