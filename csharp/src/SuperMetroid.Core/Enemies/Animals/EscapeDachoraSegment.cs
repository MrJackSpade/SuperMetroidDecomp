namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Dachora's typed animation segments (#641): its laps each way at either tide and its run for the escape, in authored order.
/// Their steps are <see cref="EscapeDachoraAnimations"/>.
/// </summary>
internal enum EscapeDachoraSegment
{
    /// <summary>The running around low tide left animation.</summary>
    RunningAroundLowTideLeft,
    /// <summary>The running around low tide right animation.</summary>
    RunningAroundLowTideRight,
    /// <summary>The running around high tide left animation.</summary>
    RunningAroundHighTideLeft,
    /// <summary>The running around high tide right animation.</summary>
    RunningAroundHighTideRight,
    /// <summary>The running for escape animation.</summary>
    RunningForEscape,
    /// <summary>The running for escape accelerating animation.</summary>
    RunningForEscapeAccelerating,
    /// <summary>The running for escape maximum speed animation.</summary>
    RunningForEscapeMaximumSpeed,
}
