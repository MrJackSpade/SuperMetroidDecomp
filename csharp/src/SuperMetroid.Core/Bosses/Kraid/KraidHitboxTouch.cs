namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>How one of Kraid's arm or foot rectangles reacts when Samus touches it.</summary>
internal enum KraidHitboxTouch
{
    /// <summary>The raised arm pushes Samus back and arms the belly lint.</summary>
    Arm = 1,
    /// <summary>Ordinary contact: the lowered or lunging arm, the arm's body component, and the foot.</summary>
    Background,
}
