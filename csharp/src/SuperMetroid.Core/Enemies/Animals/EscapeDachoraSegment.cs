namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Dachora's typed programs (#641), one per animation a spawn, branch or resumed sleep enters, in authored order.
/// Their steps are <see cref="EscapeDachoraAnimations"/>.
/// </summary>
internal enum EscapeDachoraSegment
{
    /// <summary>The running around low tide animation.</summary>
    RunningAroundLowTide,
    /// <summary>The running around low tide left animation.</summary>
    RunningAroundLowTideLeft,
    /// <summary>The running around low tide right animation.</summary>
    RunningAroundLowTideRight,
    /// <summary>The running around high tide animation.</summary>
    RunningAroundHighTide,
    /// <summary>The running around high tide left animation.</summary>
    RunningAroundHighTideLeft,
    /// <summary>The running around high tide left loop animation.</summary>
    RunningAroundHighTideLeftLoop,
    /// <summary>The running around high tide right animation.</summary>
    RunningAroundHighTideRight,
    /// <summary>The running around high tide right loop animation.</summary>
    RunningAroundHighTideRightLoop,
    /// <summary>The running for escape animation.</summary>
    RunningForEscape,
    /// <summary>The running for escape accelerating animation.</summary>
    RunningForEscapeAccelerating,
    /// <summary>The running for escape maximum speed animation.</summary>
    RunningForEscapeMaximumSpeed,
}
