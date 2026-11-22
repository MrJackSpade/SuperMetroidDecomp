namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The species registry: which factory creates the entity for each definition. An enemy system's registry is made before
/// the room's places, which reach it from their creation, and the room's species, composed over those places, add
/// their factories to it, covering every definition the room can bind (<see cref="AddRoomSpecies"/>).
/// </summary>
internal sealed class EnemySpeciesRegistry : IEnemySpeciesSource
{
    /// <inheritdoc/>
    EnemySpeciesRegistry IEnemySpeciesSource.Species => this;

    /// <summary>The factory of every definition added.</summary>
    private Dictionary<EnemyDefinitionId, IEnemyEntityFactory> Factories { get; } = [];

    /// <summary>The shared participation the entities of every definition added compose.</summary>
    private Dictionary<EnemyDefinitionId, EnemyCommonParticipation> Participations { get; } = [];

    /// <summary>Makes a room's registry, to which its species add their factories once they are composed.</summary>
    internal EnemySpeciesRegistry()
    {
    }

    /// <summary>Indexes <paramref name="factories"/> by definition, rejecting a definition owned twice.</summary>
    /// <exception cref="InvalidOperationException">Two factories own one definition.</exception>
    private void AddFactories(EnemyCommonParticipation common, IEnumerable<IEnemyEntityFactory> factories)
    {
        Ensure.NotNull(common);
        foreach (IEnemyEntityFactory factory in Ensure.NotNull(factories))
        foreach (EnemyDefinitionId definition in factory.Creations.Keys)
        {
            if (!Factories.TryAdd(definition, factory))
                throw new InvalidOperationException(
                    $"Enemy definition {definition} has two factories: {Factories[definition].GetType().Name} and {factory.GetType().Name}.");
            Participations.Add(definition, common);
        }
    }

    /// <summary>
    /// Adds the room's species: every definition the room can place has exactly one factory. A definition whose owner
    /// needs scenery the room lacks (<paramref name="withoutScenery"/>: the statue room's statue, the turning floor's
    /// elevator door) has none, and placing it fails at its placement.
    /// </summary>
    /// <param name="common">The shared participation every entity composes.</param>
    /// <param name="factories">The species' factories.</param>
    /// <param name="withoutScenery">The definitions whose owners the room's scenery does not let it build.</param>
    /// <exception cref="InvalidOperationException">
    /// Two factories own one definition, none owns a bindable one the room's scenery allows, or one owns a definition the
    /// room's scenery does not allow.
    /// </exception>
    internal void AddRoomSpecies(EnemyCommonParticipation common, IEnumerable<IEnemyEntityFactory> factories,
        IReadOnlyCollection<EnemyDefinitionId> withoutScenery)
    {
        Ensure.NotNull(withoutScenery);
        AddFactories(common, factories);
        foreach (EnemyDefinitionId definition in withoutScenery)
        {
            if (Factories.ContainsKey(definition))
                throw new InvalidOperationException($"Enemy definition {definition} has a factory although the room's scenery builds none.");
        }
        var unowned = RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Id)
            .Where(definition => !Factories.ContainsKey(definition) && !withoutScenery.Contains(definition))
            .ToList();
        if (unowned.Count != 0)
            throw new InvalidOperationException(
                $"Bindable enemy definitions have no factory: {string.Join(", ", unowned)}.");
    }


    /// <summary>Creates the entity for <paramref name="occupant"/>, which <paramref name="place"/> now holds, and its setup, through its species' factory.</summary>
    /// <param name="occupant">The occupant just installed.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <exception cref="InvalidOperationException">No species owns the occupant's definition.</exception>
    internal EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        Ensure.NotNull(occupant);
        Ensure.NotNull(place);
        if (!Factories.TryGetValue(occupant.DefinitionId, out IEnemyEntityFactory? factory))
            throw new InvalidOperationException($"No enemy species owns definition {occupant.DefinitionId}.");
        return factory.Creations[occupant.DefinitionId](occupant, place, Participations[occupant.DefinitionId]);
    }
}
