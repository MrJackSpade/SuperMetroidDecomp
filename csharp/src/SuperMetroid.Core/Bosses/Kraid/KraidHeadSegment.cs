namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's three head animations. At an animation's start his mouth tests find the pose
/// <see cref="KraidHeadAnimations.ReadsAt"/> names for it (#1886, #1887).
/// </summary>
internal enum KraidHeadSegment
{
    /// <summary>The attack roar: the mouth opens through three stages, roars, holds wide open, then closes.</summary>
    Roar,
    /// <summary>The eye glow after a body hit: the mouth opens one stage and closes.</summary>
    EyeGlow,
    /// <summary>The death cry: the mouth opens through every stage and stays open.</summary>
    Death,
}
