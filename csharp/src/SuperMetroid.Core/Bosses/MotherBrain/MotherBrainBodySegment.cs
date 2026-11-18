namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's body animations, in authored order, which her body runs.</summary>
public enum MotherBrainBodySegment
{
    /// <summary>Mother Brain body animation: walking forwards, really fast.</summary>
    WalkForwardReallyFast,
    /// <summary>Mother Brain body animation: walking forwards, fast.</summary>
    WalkForwardFast,
    /// <summary>Mother Brain body animation: walking forwards, medium.</summary>
    WalkForwardMedium,
    /// <summary>Mother Brain body animation: walking forwards, slow.</summary>
    WalkForwardSlow,
    /// <summary>Mother Brain body animation: walking forwards, really slow.</summary>
    WalkForwardReallySlow,
    /// <summary>Mother Brain body animation: walking backwards, slow.</summary>
    WalkBackwardSlow,
    /// <summary>Mother Brain body animation: walking backwards, really fast.</summary>
    WalkBackwardReallyFast,
    /// <summary>Mother Brain body animation: walking backwards, fast.</summary>
    WalkBackwardFast,
    /// <summary>Mother Brain body animation: walking backwards, medium.</summary>
    WalkBackwardMedium,
    /// <summary>Mother Brain body animation: walking backwards, really slow.</summary>
    WalkBackwardReallySlow,
    /// <summary>Mother Brain body animation: crouch and then stand up.</summary>
    CrouchAndThenStandUp,
    /// <summary>Mother Brain body animation: standing up after crouching, slow.</summary>
    StandAfterCrouchSlow,
    /// <summary>Mother Brain body animation: standing up after crouching, fast.</summary>
    StandAfterCrouchFast,
    /// <summary>Mother Brain body animation: standing up after leaning down.</summary>
    StandAfterLeaning,
    /// <summary>Mother Brain body animation: leaning down.</summary>
    LeanDown,
    /// <summary>Mother Brain body animation: crouched.</summary>
    Crouched,
    /// <summary>Mother Brain body animation: crouch, slow.</summary>
    CrouchSlow,
    /// <summary>Mother Brain body animation: crouch, fast.</summary>
    CrouchFast,
    /// <summary>The red hand beam's start: she takes the beam's pose and draws her arm back, which starts the hand-beam animation.</summary>
    HandBeam,
    /// <summary>
    /// One charge of the hand beam at her elbow, with her arm drawn back: the charge particle and the energy spark, each
    /// shown with the arm. The hand beam charges four times.
    /// </summary>
    HandBeamCharge,
    /// <summary>The hand beam's firing: her arm flings out as the beam fires and returns, and she stands before the next phase.</summary>
    HandBeamFire,
}
