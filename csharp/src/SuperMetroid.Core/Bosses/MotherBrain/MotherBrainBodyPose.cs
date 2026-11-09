namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Values written by Mother Brain's body program cues.</summary>
public enum MotherBrainBodyPose
{
    /// <summary>Pose 0; standing allows posture changes and ordinary attack/walking decisions.</summary>
    Standing = 0,
    /// <summary>Pose 1, while authored walking bytecode owns the body's displacement.</summary>
    Walking = 1,
    /// <summary>Pose 2, during crouch/uncrouch transitions; posture helpers wait for bytecode to publish an endpoint.</summary>
    CrouchingTransition = 2,
    /// <summary>Pose 3, at the crouched endpoint; the standing helper selects the matching uncrouch list.</summary>
    Crouched = 3,
    /// <summary>Pose 4, for the ordinary red hand-beam body animation.</summary>
    DeathBeam = 4,
    /// <summary>Pose 6, at the leaning-down endpoint; standing requires its distinct recovery list.</summary>
    LeaningDown = 6,
}
