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

    /// <summary>
    /// Builds the entity for <paramref name="occupant"/>, which <paramref name="place"/> now holds, spawned on another
    /// enemy's behalf or rebuilt by a respawn, through its species' factory.
    /// </summary>
    /// <param name="occupant">The occupant just installed.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <exception cref="InvalidOperationException">No species owns the occupant's definition.</exception>
    internal EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place) =>
        FactoryOf(occupant).Create(occupant, Ensure.NotNull(place), _common);

    /// <summary>
    /// Builds the entity for <paramref name="occupant"/>, which the room's population load placed in
    /// <paramref name="place"/>, through its species' factory.
    /// </summary>
    /// <param name="occupant">The occupant just installed.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="surroundings">The room, Samus, controller and camera the population load sets the enemy up in.</param>
    /// <exception cref="InvalidOperationException">No species owns the occupant's definition.</exception>
    internal EnemyOccupant CreateLoaded(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyPopulationSurroundings surroundings) =>
        FactoryOf(occupant).CreateLoaded(occupant, Ensure.NotNull(place), _common, Ensure.NotNull(surroundings));

    /// <summary>
    /// Builds the entity for <paramref name="occupant"/>, spawned in <paramref name="place"/> joining
    /// <paramref name="encounter"/>, through its species' factory.
    /// </summary>
    /// <param name="occupant">The occupant just installed.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="encounter">The encounter the spawned enemy joins.</param>
    /// <typeparam name="TEncounter">The encounter's type.</typeparam>
    /// <exception cref="InvalidOperationException">
    /// No species owns the occupant's definition, or its species' enemies are never spawned joining a
    /// <typeparamref name="TEncounter"/>.
    /// </exception>
    internal EnemyOccupant CreateJoining<TEncounter>(RoomEnemyOccupant occupant, RoomEnemyPlace place, TEncounter encounter)
        where TEncounter : class =>
        FactoryOf(occupant) is IEnemyJoiningFactory<TEncounter> joining
            ? joining.CreateJoining(occupant, Ensure.NotNull(place), _common, Ensure.NotNull(encounter))
            : throw new InvalidOperationException($"Enemy {occupant.DefinitionId} is never spawned joining a {typeof(TEncounter).Name}.");

    /// <summary>The factory of <paramref name="occupant"/>'s definition.</summary>
    /// <exception cref="InvalidOperationException">No species owns the occupant's definition.</exception>
    private IEnemyEntityFactory FactoryOf(RoomEnemyOccupant occupant) =>
        Factories.TryGetValue(Ensure.NotNull(occupant).DefinitionId, out IEnemyEntityFactory? factory)
            ? factory
            : throw new InvalidOperationException($"No enemy species owns definition {occupant.DefinitionId}.");
}
