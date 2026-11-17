namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>
/// The escaping Dachora's typed animation segments (#641): its laps each way at either tide and its run for the escape, in authored order.
/// Their steps are <see cref="EscapeDachoraAnimations"/>.
/// </summary>
internal enum EscapeDachoraSegment
{
    /// <summary>One stride running left at low tide, stepping left with each pose, which ends a low-tide lap.</summary>
    RunningAroundLowTideLeft,
    /// <summary>
    /// One stride running right at low tide, stepping right with each pose, which ends a low-tide lap.
    /// </summary>
    RunningAroundLowTideRight,
    /// <summary>One quicker stride running left at high tide, stepping left with each pose, which ends a lap.</summary>
    RunningAroundHighTideLeft,
    /// <summary>
    /// One quicker stride running right at high tide, stepping right with each pose, which ends a lap.
    /// </summary>
    RunningAroundHighTideRight,
    /// <summary>A pause in its running pose facing left before it turns to run for the escape.</summary>
    RunningForEscape,
    /// <summary>
    /// A long pause in its running pose facing right, then running right with a stride that quickens, stepping right
    /// with each pose.
    /// </summary>
    RunningForEscapeAccelerating,
    /// <summary>Running right for the escape at its quickest, a new pose and a step right every update.</summary>
    RunningForEscapeMaximumSpeed,
}
