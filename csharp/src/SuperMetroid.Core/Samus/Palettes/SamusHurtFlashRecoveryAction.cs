namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>Counter-forty audio recovery selected by movement/grapple state.</summary>
public enum SamusHurtFlashRecoveryAction
{
    /// <summary>No recovery sound or deferred charging request was selected for the current movement/grapple state.</summary>
    None,
    /// <summary>Spin recovery selects the ordinary spin/wall-jump sound, with queue threshold nine.</summary>
    SpinJumpSound,
    /// <summary>Spin recovery selects the Space Jump sound from Samus's pose or wall-jump animation frame.</summary>
    SpaceJumpSound,
    /// <summary>Spin recovery selects the Screw Attack sound from Samus's pose or wall-jump animation frame.</summary>
    ScrewAttackSound,
    /// <summary>Queues the grapple sound with threshold nine while Grapple is firing, connected, wall-grabbing, or releasing a wall grab.</summary>
    GrappleSound,
    /// <summary>Sets the resume-charging latch when the flare count is at least sixteen and X is held; the post-draw consumer may later queue the resume-charging sound.</summary>
    ResumeChargingBeamRequested,
}
