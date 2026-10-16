namespace SuperMetroid.Core.Enemies;

/// <summary>The species registry for slots a fixture builds outside any enemy system.</summary>
internal static class EnemySlotFixtures
{
    /// <summary>
    /// A registry with no species and no enemy system: binding works, and any shared reaction the fixture's slot is asked
    /// for fails loudly, since nothing registered one.
    /// </summary>
    internal static EnemySpeciesRegistry Registry { get; } = new(
        new EnemyCommonParticipation(() => throw new InvalidOperationException(
            "A standalone fixture slot has no enemy system to share reactions with.")), []);
}
