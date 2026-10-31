namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's body programs, declared in the order the cartridge authors them. The body enemy slot
/// runs them, and so does the head slot, which keeps <see cref="InitialDummy"/> (its frame being its hitbox) for life.
/// </summary>
public enum MotherBrainBodyProgram
{
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_ReallyFast</c>.</summary>
    WalkForwardReallyFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Fast</c>.</summary>
    WalkForwardFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Medium</c>.</summary>
    WalkForwardMedium,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_Slow</c>.</summary>
    WalkForwardSlow,
    /// <summary><c>InstList_MotherBrainBody_WalkingForwards_ReallySlow</c>.</summary>
    WalkForwardReallySlow,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_Slow</c>.</summary>
    WalkBackwardSlow,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_ReallyFast</c>.</summary>
    WalkBackwardReallyFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_Fast</c>.</summary>
    WalkBackwardFast,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_Medium</c>.</summary>
    WalkBackwardMedium,
    /// <summary><c>InstList_MotherBrainBody_WalkingBackwards_ReallySlow</c>.</summary>
    WalkBackwardReallySlow,
    /// <summary><c>InstList_MotherBrainBody_CrouchAndThenStandUp</c>.</summary>
    CrouchAndThenStandUp,
    /// <summary><c>InstList_MotherBrainBody_StandingUpAfterCrouching_Slow</c>.</summary>
    StandAfterCrouchSlow,
    /// <summary><c>InstList_MotherBrainBody_StandingUpAfterCrouching_Fast</c>.</summary>
    StandAfterCrouchFast,
    /// <summary><c>InstList_MotherBrainBody_StandingUpAfterLeaningDown</c>.</summary>
    StandAfterLeaning,
    /// <summary><c>InstList_MotherBrainBody_LeaningDown</c>.</summary>
    LeanDown,
    /// <summary><c>InstList_MotherBrainBody_Crouched</c>.</summary>
    Crouched,
    /// <summary><c>InstList_MotherBrainBody_Crouch_Slow</c>.</summary>
    CrouchSlow,
    /// <summary><c>InstList_MotherBrainBody_Crouch_Fast</c>.</summary>
    CrouchFast,
    /// <summary>The red hand-beam body program.</summary>
    HandBeam,
    /// <summary><c>InstList_MotherBrainHead_InitialDummy</c>, shared by the body and head slots.</summary>
    /// <remarks>Its zero duration is the cartridge's: the timer wraps and the dummy frame stays for good.</remarks>
    InitialDummy,
}
