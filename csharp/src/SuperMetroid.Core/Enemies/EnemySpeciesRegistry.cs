namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The species registry: which factory creates the entity for each definition. An enemy system's registry covers every
/// definition its room can bind (<see cref="ForRoom"/>).
/// </summary>
internal sealed class EnemySpeciesRegistry
{
    /// <summary>The factory of every definition the room can bind.</summary>
    private Dictionary<EnemyDefinitionId, IEnemyEntityFactory> Factories { get; } = [];

    /// <summary>The shared participation every entity composes.</summary>
    private readonly EnemyCommonParticipation _common;

    /// <summary>Indexes <paramref name="factories"/> by definition, rejecting a definition owned twice.</summary>
    /// <param name="common">The shared participation every entity composes.</param>
    /// <param name="factories">The species' factories.</param>
    /// <exception cref="InvalidOperationException">Two factories own one definition.</exception>
    private EnemySpeciesRegistry(EnemyCommonParticipation common, IEnumerable<IEnemyEntityFactory> factories)
    {
        _common = Ensure.NotNull(common);
        foreach (IEnemyEntityFactory factory in Ensure.NotNull(factories))
        foreach (EnemyDefinitionId definition in factory.Definitions)
        {
            if (!Factories.TryAdd(definition, factory))
                throw new InvalidOperationException(
                    $"Enemy definition {definition} has two factories: {Factories[definition].GetType().Name} and {factory.GetType().Name}.");
        }
    }

    /// <summary>
    /// The registry of a room's enemy system: every definition the room can place has exactly one factory. A definition
    /// whose owner needs scenery the room lacks (<paramref name="withoutScenery"/>: the statue room's statue, the turning
    /// floor's elevator door) has none, and placing it fails at its placement.
    /// </summary>
    /// <param name="common">The shared participation every entity composes.</param>
    /// <param name="factories">The species' factories.</param>
    /// <param name="withoutScenery">The definitions whose owners the room's scenery does not let it build.</param>
    /// <exception cref="InvalidOperationException">
    /// Two factories own one definition, none owns a bindable one the room's scenery allows, or one owns a definition the
    /// room's scenery does not allow.
    /// </exception>
    internal static EnemySpeciesRegistry ForRoom(EnemyCommonParticipation common, IEnumerable<IEnemyEntityFactory> factories,
        IReadOnlyCollection<EnemyDefinitionId> withoutScenery)
    {
        Ensure.NotNull(withoutScenery);
        var registry = new EnemySpeciesRegistry(common, factories);
        foreach (EnemyDefinitionId definition in withoutScenery)
        {
            if (registry.Factories.ContainsKey(definition))
                throw new InvalidOperationException($"Enemy definition {definition} has a factory although the room's scenery builds none.");
        }
        var unowned = RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Id)
            .Where(definition => !registry.Factories.ContainsKey(definition) && !withoutScenery.Contains(definition))
            .ToList();
        if (unowned.Count != 0)
            throw new InvalidOperationException(
                $"Bindable enemy definitions have no factory: {string.Join(", ", unowned)}.");
        return registry;
    }

    /// <summary>Creates the entity for the occupant <paramref name="place"/> now holds, and its setup, through its species' factory.</summary>
    /// <param name="place">The place, already holding its new identity.</param>
    /// <exception cref="InvalidOperationException">No species owns the place's definition.</exception>
    internal EnemyOccupant Create(RoomEnemyPlace place)
    {
        Ensure.NotNull(place);
        if (!Factories.TryGetValue(place.DefinitionId, out IEnemyEntityFactory? factory))
            throw new InvalidOperationException($"No enemy species owns definition {place.DefinitionId}.");
        return factory.Create(place, _common);
    }
}
