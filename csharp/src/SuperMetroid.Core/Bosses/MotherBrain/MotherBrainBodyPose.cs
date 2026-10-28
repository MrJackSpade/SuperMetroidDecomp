namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Values written by Mother Brain's body instruction opcodes at $A9:9700-$972F.</summary>
public enum MotherBrainBodyPose
{
    /// <summary>Native pose 0, written by $A9:9700; standing allows posture changes and ordinary attack/walking decisions.</summary>
    Standing = 0,
    /// <summary>Native pose 1, written by $A9:9708 while authored walking bytecode owns the body's displacement.</summary>
    Walking = 1,
    /// <summary>Native pose 2, written by $A9:9718 during crouch/uncrouch transitions; posture helpers wait for bytecode to publish an endpoint.</summary>
    CrouchingTransition = 2,
    /// <summary>Native pose 3, written by $A9:9710 at the crouched endpoint; the standing helper selects the matching uncrouch list.</summary>
    Crouched = 3,
    /// <summary>Native pose 4, written by $A9:9720 for the ordinary red hand-beam body animation.</summary>
    DeathBeam = 4,
    /// <summary>Native pose 6, written by $A9:9728 at the leaning-down endpoint; standing requires its distinct recovery list.</summary>
    LeaningDown = 6,
}
