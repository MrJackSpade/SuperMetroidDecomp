namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's body programs, in authored order. The body enemy slot
/// runs them, and so does the head slot, which keeps <see cref="InitialDummy"/> (its frame being its hitbox) for life.
/// </summary>
public enum MotherBrainBodyProgram
{
    /// <summary>Mother Brain body animation script: walking forwards, really fast.</summary>
    WalkForwardReallyFast,
    /// <summary>Mother Brain body animation script: walking forwards, fast.</summary>
    WalkForwardFast,
    /// <summary>Mother Brain body animation script: walking forwards, medium.</summary>
    WalkForwardMedium,
    /// <summary>Mother Brain body animation script: walking forwards, slow.</summary>
    WalkForwardSlow,
    /// <summary>Mother Brain body animation script: walking forwards, really slow.</summary>
    WalkForwardReallySlow,
    /// <summary>Mother Brain body animation script: walking backwards, slow.</summary>
    WalkBackwardSlow,
    /// <summary>Mother Brain body animation script: walking backwards, really fast.</summary>
    WalkBackwardReallyFast,
    /// <summary>Mother Brain body animation script: walking backwards, fast.</summary>
    WalkBackwardFast,
    /// <summary>Mother Brain body animation script: walking backwards, medium.</summary>
    WalkBackwardMedium,
    /// <summary>Mother Brain body animation script: walking backwards, really slow.</summary>
    WalkBackwardReallySlow,
    /// <summary>Mother Brain body animation script: crouch and then stand up.</summary>
    CrouchAndThenStandUp,
    /// <summary>Mother Brain body animation script: standing up after crouching, slow.</summary>
    StandAfterCrouchSlow,
    /// <summary>Mother Brain body animation script: standing up after crouching, fast.</summary>
    StandAfterCrouchFast,
    /// <summary>Mother Brain body animation script: standing up after leaning down.</summary>
    StandAfterLeaning,
    /// <summary>Mother Brain body animation script: leaning down.</summary>
    LeanDown,
    /// <summary>Mother Brain body animation script: crouched.</summary>
    Crouched,
    /// <summary>Mother Brain body animation script: crouch, slow.</summary>
    CrouchSlow,
    /// <summary>Mother Brain body animation script: crouch, fast.</summary>
    CrouchFast,
    /// <summary>The red hand-beam body program.</summary>
    HandBeam,
    /// <summary>Shared by the body and head slots.</summary>
    /// <remarks>Its zero duration is the game's: the timer wraps and the dummy frame stays for good.</remarks>
    InitialDummy,
}
