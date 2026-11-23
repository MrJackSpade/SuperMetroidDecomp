namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's body animations, in authored order, which her body runs.</summary>
public enum MotherBrainBodySegment
{
    /// <summary>One forward stride from standing, through each leg's step to her legs together, moving her body on with each step: her fastest forward walk.</summary>
    WalkForwardReallyFast,
    /// <summary>The same forward stride, each frame held longer than her fastest walk's.</summary>
    WalkForwardFast,
    /// <summary>The same forward stride at her middle pace.</summary>
    WalkForwardMedium,
    /// <summary>The same forward stride, each frame held longer than her middle pace's.</summary>
    WalkForwardSlow,
    /// <summary>The same forward stride at her slowest pace.</summary>
    WalkForwardReallySlow,
    /// <summary>One backward stride from her legs together back to standing, each frame held longer than her middle pace's.</summary>
    WalkBackwardSlow,
    /// <summary>The same backward stride: her fastest backward walk.</summary>
    WalkBackwardReallyFast,
    /// <summary>The same backward stride, each frame held longer than her fastest backward walk's.</summary>
    WalkBackwardFast,
    /// <summary>The same backward stride at her middle pace.</summary>
    WalkBackwardMedium,
    /// <summary>The same backward stride at her slowest pace.</summary>
    WalkBackwardReallySlow,
    /// <summary>Her body lowering into a crouch and rising straight back to standing, slowly.</summary>
    CrouchAndThenStandUp,
    /// <summary>Her body rising from its crouch through uncrouching and leaning down to standing, slowly.</summary>
    StandAfterCrouchSlow,
    /// <summary>The same rise from her crouch to standing, each frame held half as long.</summary>
    StandAfterCrouchFast,
    /// <summary>Her body rising from leaning down to standing.</summary>
    StandAfterLeaning,
    /// <summary>Her body lowering from standing to leaning down, which it then holds.</summary>
    LeanDown,
    /// <summary>Her body holding its crouch.</summary>
    Crouched,
    /// <summary>Her body lowering from standing into a crouch, slowly.</summary>
    CrouchSlow,
    /// <summary>Her body dropping quickly from standing into a crouch, leaning and uncrouching.</summary>
    CrouchFast,
    /// <summary>The red hand beam's start: she takes the beam's pose and draws her arm back, which starts the hand-beam animation.</summary>
    HandBeam,
    /// <summary>
    /// One charge of the hand beam at her elbow, with her arm drawn back: the charge particle and the energy spark, each
    /// shown with the arm. The hand beam charges <see cref="MotherBrainAnimations.HandBeamCharges"/> times over.
    /// </summary>
    HandBeamCharge,
    /// <summary>The hand beam's firing: her arm flings out as the beam fires and returns, and she stands before the next phase.</summary>
    HandBeamFire,
}
