namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The species registry, built once per enemy system: which factory creates the entity for each definition. A
/// definition no species owns binds an <see cref="UnownedEnemyEntity"/>.
/// </summary>
internal sealed class EnemySpeciesRegistry
{
    /// <summary>The factory of every converted definition.</summary>
    private readonly Dictionary<EnemyDefinitionId, IEnemyEntityFactory> _factories = [];

    /// <summary>The shared participation every entity composes.</summary>
    private readonly EnemyCommonParticipation _common;

    /// <summary>Indexes <paramref name="factories"/> by definition, rejecting a definition owned twice.</summary>
    /// <param name="common">The shared participation every entity composes.</param>
    /// <param name="factories">The species' factories.</param>
    /// <exception cref="InvalidOperationException">Two factories own one definition.</exception>
    internal EnemySpeciesRegistry(EnemyCommonParticipation common, IEnumerable<IEnemyEntityFactory> factories)
    {
        _common = Ensure.NotNull(common);
        foreach (IEnemyEntityFactory factory in Ensure.NotNull(factories))
        foreach (EnemyDefinitionId definition in factory.Definitions)
        {
            if (!_factories.TryAdd(definition, factory))
                throw new InvalidOperationException(
                    $"Enemy definition ${(int)definition:X4} has two factories: {_factories[definition].GetType().Name} and {factory.GetType().Name}.");
        }
    }

    /// <summary>Creates the entity for the occupant <paramref name="slot"/> now holds.</summary>
    /// <param name="slot">The slot, already holding its new identity.</param>
    internal IEnemyEntity Create(RoomEnemySlot slot)
    {
        Ensure.NotNull(slot);
        return _factories.TryGetValue(slot.EnemyDefinitionPointer, out IEnemyEntityFactory? factory)
            ? factory.Create(slot, _common)
            : new UnownedEnemyEntity(slot, _common);
    }
}
