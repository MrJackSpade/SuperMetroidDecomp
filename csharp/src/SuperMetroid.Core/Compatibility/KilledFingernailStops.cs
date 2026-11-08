namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a Kraid fingernail's launch reads of its sibling after the sibling was killed (#1543): a nail at rest. Each of
/// Kraid's two fingernails launches upward or downward by the sign of its sibling's last vertical speed. A sibling shot,
/// bombed or touched to death has stopped: until it respawns, and from its respawn until its own next launch, it gives
/// the launch no speed, so the launch goes upward.
/// </summary>
/// <remarks>
/// Consulted once per launch, as a fingernail's flight starts. Evidence: the launch reads the vertical speed held in the
/// other fingernail's enemy record without testing whether a fingernail still occupies it; a kill clears that record,
/// and a respawned fingernail's setup leaves the cleared speed in place until it launches.
/// </remarks>
[CompatibilityService("a fingernail launching after its sibling was killed reads the sibling as stopped")]
public static class KilledFingernailStops
{
    /// <summary>The sibling's vertical speed a launch reads: its last speed, or a stopped nail's once it was killed.</summary>
    /// <typeparam name="TSpeed">A fingernail's vertical speed.</typeparam>
    /// <param name="lastSpeed">The speed the sibling last moved at.</param>
    /// <param name="stopped">The speed of a nail at rest.</param>
    /// <param name="siblingKilled">Whether the sibling has been killed since it was last set up.</param>
    public static TSpeed SiblingSpeed<TSpeed>(TSpeed lastSpeed, TSpeed stopped, bool siblingKilled) =>
        siblingKilled ? stopped : lastSpeed;
}
