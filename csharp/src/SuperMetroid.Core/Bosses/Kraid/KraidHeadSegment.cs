namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The runs of Kraid's head animations (#2399). His attack roar plays three, its opening, its wide-open hold and its
/// closing, at which his growth resumes it; the eye glow and the death cry are one run each. At an animation's start his
/// mouth tests find the pose <see cref="KraidHeadAnimations.ReadsAt"/> names for it (#1886, #1887).
/// </summary>
internal enum KraidHeadSegment
{
    /// <summary>The attack roar's opening: the mouth opens through three stages, then roars.</summary>
    Roar,
    /// <summary>The attack roar's wide-open hold, after which growth resumes it when the head shows no closing pose next.</summary>
    RoarOpenHold,
    /// <summary>The attack roar's closing: the mouth closes through three stages, at any of which growth resumes the roar.</summary>
    RoarClosing,
    /// <summary>The eye glow after a body hit: the mouth opens one stage and closes.</summary>
    EyeGlow,
    /// <summary>The death cry: the mouth opens through every stage and stays open.</summary>
    Death,
}
