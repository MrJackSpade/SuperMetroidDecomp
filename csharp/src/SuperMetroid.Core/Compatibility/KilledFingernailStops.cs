namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What a Kraid fingernail's launch reads of its sibling after the sibling was killed (#1543): a nail at rest. Each of
/// Kraid's two fingernails launches upward or downward by the sign of its sibling's last vertical speed. A sibling shot,
/// bombed or touched to death has stopped: until it respawns, and from its respawn until its own next launch, it gives
/// the launch no speed, so the launch goes upward. A launch also takes the alternate spawn, waiting for the lint at the
/// room's left, only when the random draw allows it and its sibling did not take it at its own last launch; a killed
/// sibling, and a respawned one until its own next launch, reads as having taken the ordinary spawn (#1620).
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

    /// <summary>
    /// Whether a launch reads its sibling as having taken the alternate spawn: as the living sibling last launched, or not
    /// at all once it was killed. Evidence: the launch reads the sibling's alternate-spawn mark in the same enemy record
    /// without testing it, the kill clears the record, and a fingernail's setup leaves the mark cleared.
    /// </summary>
    /// <typeparam name="TSibling">A fingernail's state.</typeparam>
    /// <param name="livingSibling">The sibling's state, or null when it has been killed since it was last set up.</param>
    /// <param name="tookAlternateSpawn">Whether a living sibling took the alternate spawn at its last launch since its setup.</param>
    public static bool SiblingTookAlternateSpawn<TSibling>(TSibling? livingSibling, Func<TSibling, bool> tookAlternateSpawn)
        where TSibling : class =>
        livingSibling is not null && Ensure.NotNull(tookAlternateSpawn)(livingSibling);
}