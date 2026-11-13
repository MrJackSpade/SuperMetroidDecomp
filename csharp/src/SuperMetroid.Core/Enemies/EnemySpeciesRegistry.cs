using SuperMetroid.Core.Enemies.Inert;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The species registry: which factory creates the entity for each definition. An enemy system's registry covers every
/// definition a retail room or the respawn placeholder can bind (<see cref="ForRetailRooms"/>), except the two
/// definitions no room places (<see cref="UnportedDefinitions"/>).
/// </summary>
internal sealed class EnemySpeciesRegistry
{
    /// <summary>
    /// Definitions whose artwork rooms load but which no retail room places and no enemy spawns: Reflec and the second
    /// dead Sidehopper (the Tourian room places only the first). Their AI never runs in play, so they have no species;
    /// binding one fails loudly.
    /// </summary>
    private static readonly HashSet<EnemyDefinitionId> UnportedDefinitions = [EnemyDefinitionId.Reflec, EnemyDefinitionId.CorpseSidehopper2];

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
                    $"Enemy definition {definition} has two factories: {_factories[definition].GetType().Name} and {factory.GetType().Name}.");
        }
    }

    /// <summary>
    /// The registry of an enemy system: every definition a retail room places, and the respawn placeholder, has exactly
    /// one factory, except the definitions no room places.
    /// </summary>
    /// <param name="common">The shared participation every entity composes.</param>
    /// <param name="factories">The species' factories.</param>
    /// <exception cref="InvalidOperationException">Two factories own one definition, or none owns a bindable one.</exception>
    internal static EnemySpeciesRegistry ForRetailRooms(EnemyCommonParticipation common, IEnumerable<IEnemyEntityFactory> factories)
    {
        var registry = new EnemySpeciesRegistry(common, factories);
        var unowned = RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Id).Append(EnemyDefinitionId.Respawn)
            .Where(definition => !registry._factories.ContainsKey(definition) && !UnportedDefinitions.Contains(definition))
            .ToList();
        if (unowned.Count != 0)
            throw new InvalidOperationException(
                $"Bindable enemy definitions have no factory: {string.Join(", ", unowned)}.");
        return registry;
    }

    /// <summary>
    /// Creates the entity for the occupant <paramref name="slot"/> now holds. A definition outside the retail placements
    /// (constructed test data, or a named definition no room places) has no species: it runs only the shared
    /// participation, and any AI it names beyond the shared no-op fails at dispatch.
    /// </summary>
    /// <param name="slot">The slot, already holding its new identity.</param>
    /// <exception cref="InvalidDataException">The slot holds a definition no retail room places.</exception>
    internal IEnemyEntity Create(RoomEnemySlot slot)
    {
        Ensure.NotNull(slot);
        if (_factories.TryGetValue(slot.DefinitionId, out IEnemyEntityFactory? factory))
            return factory.Create(slot, _common);
        if (UnportedDefinitions.Contains(slot.DefinitionId))
            throw new InvalidDataException($"Enemy slot {slot.SlotIndex} holds {slot.DefinitionId}, which no retail room places and which has no species.");
        return new InertEnemyEntity(slot, _common);
    }
}
