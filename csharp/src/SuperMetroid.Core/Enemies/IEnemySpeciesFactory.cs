namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A species owner that builds the enemies of the definitions it owns (#2554): through its entry's settings when the
/// population tells it something (<see cref="IEnemySettingsFactory{TSettings}"/>), by its definition when it tells it
/// nothing (<see cref="IEnemyEntityFactory"/>), or as the head of a group whose parts the room lists right after it
/// (<see cref="IEnemyGroupFactory"/>, <see cref="IEnemySettingsGroupFactory{TSettings}"/>).
/// </summary>
internal interface IEnemySpeciesFactory
{
    /// <summary>The definitions whose enemies this factory builds; each definition has exactly one owner.</summary>
    IReadOnlyList<EnemyDefinitionId> Definitions { get; }

    /// <summary>Records in <paramref name="registry"/> how the factory builds its enemies.</summary>
    /// <param name="registry">The room's species registry.</param>
    void RegisterIn(EnemySpeciesRegistry registry);
}
