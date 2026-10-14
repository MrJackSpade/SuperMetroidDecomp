namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's body programs, declared in the order the cartridge authors them in bank $A9. The body enemy slot
/// runs them, and so does the head slot, which keeps <see cref="InitialDummy"/> (its frame being its hitbox) for life.
/// </summary>
public enum MotherBrainBodyProgram
{
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_ReallyFast</c> (<c>$A9:9730</c>).</summary>
    WalkForwardReallyFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Fast</c> (<c>$A9:976A</c>).</summary>
    WalkForwardFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Medium</c> (<c>$A9:97A4</c>).</summary>
    WalkForwardMedium,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Slow</c> (<c>$A9:97DE</c>).</summary>
    WalkForwardSlow,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_ReallySlow</c> (<c>$A9:9818</c>).</summary>
    WalkForwardReallySlow,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_Slow</c> (<c>$A9:9852</c>).</summary>
    WalkBackwardSlow,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_ReallyFast</c> (<c>$A9:988C</c>).</summary>
    WalkBackwardReallyFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_Fast</c> (<c>$A9:98C6</c>).</summary>
    WalkBackwardFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_Medium</c> (<c>$A9:9900</c>).</summary>
    WalkBackwardMedium,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_ReallySlow</c> (<c>$A9:993A</c>).</summary>
    WalkBackwardReallySlow,
    /// <summary><c>InstList_MotherBrainBody_CrouchAndThenStandUp</c> (<c>$A9:9974</c>).</summary>
    CrouchAndThenStandUp,
    /// <summary><c>InstList_MotherBrainBody_StandingUpAfterCrouching_Slow</c> (<c>$A9:99AA</c>).</summary>
    StandAfterCrouchSlow,
    /// <summary><c>InstList_MotherBrainBody_StandingUpAfterCrouching_Fast</c> (<c>$A9:99C6</c>).</summary>
    StandAfterCrouchFast,
    /// <summary><c>InstList_MotherBrainBody_StandingUpAfterLeaningDown</c> (<c>$A9:99E2</c>).</summary>
    StandAfterLeaning,
    /// <summary><c>InstList_MotherBrainBody_LeaningDown</c> (<c>$A9:99F2</c>).</summary>
    LeanDown,
    /// <summary><c>InstList_MotherBrainBody_Crouched</c> (<c>$A9:9A02</c>).</summary>
    Crouched,
    /// <summary><c>InstList_MotherBrainBody_Crouch_Slow</c> (<c>$A9:9A0A</c>).</summary>
    CrouchSlow,
    /// <summary><c>InstList_MotherBrainBody_Crouch_Fast</c> (<c>$A9:9A26</c>).</summary>
    CrouchFast,
    /// <summary>The red hand-beam body program (<c>$A9:9A42</c>).</summary>
    HandBeam,
    /// <summary><c>InstList_MotherBrainHead_InitialDummy</c> (<c>$A9:9C13</c>), shared by the body and head slots.</summary>
    InitialDummy,
}
