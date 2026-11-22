namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The species registry for slots a fixture builds outside any enemy system, and the real species a fixture binds when
/// its check needs an occupant but none of the occupant's own behaviour (#2010).
/// </summary>
internal static class EnemyPlaceFixtures
{
    /// <summary>
    /// The species a check binds for an occupant whose own behaviour it does not exercise: the glass tube's crack, a real
    /// species that needs nothing from its room and has no behaviour of its own (no main behaviour, no animation, an
    /// initialization that does nothing), so binding it in any room runs nothing but the shared reactions its definition
    /// names (#2420).
    /// </summary>
    internal const EnemyDefinitionId StandIn = EnemyDefinitionId.NoobTubeCrack;

    /// <summary>
    /// A registry with no species and no enemy system, for a slot a check never binds: binding any definition fails,
    /// since no species owns it, and any shared reaction the slot is asked for fails loudly.
    /// </summary>
    internal static EnemySpeciesRegistry Registry { get; } = new global::SuperMetroid.Core.Enemies.EnemySpeciesRegistry(new EnemyCommonParticipation(() => throw new InvalidOperationException(
            "A standalone fixture slot has no enemy system to share reactions with.")), (global::System.Collections.Generic.IEnumerable<global::SuperMetroid.Core.Enemies.IEnemyEntityFactory>)([]));
}
