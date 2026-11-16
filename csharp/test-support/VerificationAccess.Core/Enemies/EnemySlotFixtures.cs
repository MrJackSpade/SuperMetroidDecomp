namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The species registry for slots a fixture builds outside any enemy system, and the real species a fixture binds when
/// its check needs an occupant but none of the occupant's own behaviour (#2010).
/// </summary>
internal static class EnemySlotFixtures
{
    /// <summary>
    /// The species a check binds for an occupant whose own behaviour it does not exercise: the Tourian entrance statue,
    /// a real species with no main behaviour. Binding it runs nothing; its initialization reads its placement's statue.
    /// </summary>
    internal const EnemyDefinitionId StandIn = EnemyDefinitionId.TourianStatue;

    /// <summary>
    /// A registry with no species and no enemy system, for a slot a check never binds: binding any definition fails,
    /// since no species owns it, and any shared reaction the slot is asked for fails loudly.
    /// </summary>
    internal static EnemySpeciesRegistry Registry { get; } = new(
        new EnemyCommonParticipation(() => throw new InvalidOperationException(
            "A standalone fixture slot has no enemy system to share reactions with.")), []);
}
