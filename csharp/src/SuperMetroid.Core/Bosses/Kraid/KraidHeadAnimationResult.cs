namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>What one step of Kraid's head animation did to the pose his head shows.</summary>
internal enum KraidHeadAnimationResult
{
    /// <summary>The animation reached its next pose, which now shows for its duration.</summary>
    PoseShown,

    /// <summary>The animation has ended; it stays on its last step and the shown pose stays.</summary>
    AnimationEnded,
}
