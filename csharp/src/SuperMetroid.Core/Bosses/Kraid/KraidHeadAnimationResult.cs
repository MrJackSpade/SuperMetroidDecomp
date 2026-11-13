namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>What one update of Kraid's head animation did to the pose his head shows.</summary>
internal enum KraidHeadAnimationResult
{
    /// <summary>The head timer is cleared or still counting, so the shown pose stays.</summary>
    PoseHeld,

    /// <summary>The animation reached its next pose, which now shows for its duration.</summary>
    PoseShown,

    /// <summary>The animation has ended; its cursor stays at the end and the shown pose stays.</summary>
    AnimationEnded,
}
