namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Composes a room's species registry (#2554): each species registers the creations it supports for the definitions it
/// owns, typed by the settings each takes, so binding a population entry checks its settings against that type once.
/// </summary>
internal sealed class EnemySpeciesRegistrar
{
    /// <summary>How each definition a population places binds its entries, by the creation its species registered.</summary>
    internal Dictionary<EnemyDefinitionId, Func<RoomEnemyPlacement, PopulationEnemySpawn>> Placed { get; } = [];

    /// <summary>The species that owns each definition, for the room's ownership checks.</summary>
    internal Dictionary<EnemyDefinitionId, string> Owners { get; } = [];

    /// <summary>The species registering now.</summary>
    private string _offering = string.Empty;

    /// <summary>Registers <paramref name="factory"/>'s creations, naming it as their owner.</summary>
    /// <param name="factory">The species.</param>
    internal void Add(IEnemyEntityFactory factory)
    {
        _offering = Ensure.NotNull(factory).GetType().Name;
        factory.OfferCreations(this);
    }

    /// <summary>Registers <paramref name="create"/> as how <paramref name="definitions"/>' enemies are built wherever they are set up.</summary>
    /// <typeparam name="TSettings">The settings the creation takes.</typeparam>
    /// <param name="definitions">The definitions it builds.</param>
    /// <param name="create">The species' creation.</param>
    /// <exception cref="InvalidOperationException">A definition is owned twice.</exception>
    internal void Spawns<TSettings>(IReadOnlyList<EnemyDefinitionId> definitions, EnemySpawnCreation<TSettings> create)
        where TSettings : EnemySpawnSettings
    {
        var shared = new SpeciesSpawnCreation<TSettings>(create);
        Place(definitions, placement => placement.SpawnSettings is TSettings settings
            ? new OrdinaryEnemySpawn<TSettings>(placement, settings, shared)
            : throw Mismatch<TSettings>(placement));
    }

    /// <summary>Registers <paramref name="create"/> as how the population load builds <paramref name="definitions"/>' enemies.</summary>
    /// <typeparam name="TSettings">The settings the creation takes.</typeparam>
    /// <param name="definitions">The definitions it builds.</param>
    /// <param name="create">The species' creation.</param>
    /// <exception cref="InvalidOperationException">A definition is owned twice.</exception>
    internal void Loads<TSettings>(IReadOnlyList<EnemyDefinitionId> definitions, EnemyLoadedCreation<TSettings> create)
        where TSettings : EnemySpawnSettings
    {
        var shared = new SpeciesLoadedCreation<TSettings>(create);
        Place(definitions, placement => placement.SpawnSettings is TSettings settings
            ? new LoadedEnemySpawn<TSettings>(placement, settings, shared)
            : throw Mismatch<TSettings>(placement));
    }

    /// <summary>
    /// Records that the registering species owns <paramref name="definitions"/>, whose enemies are only spawned joining an
    /// encounter (<see cref="IEnemyJoiningFactory{TEncounter, TSettings}"/>), so no population places them.
    /// </summary>
    /// <param name="definitions">The definitions it owns.</param>
    /// <exception cref="InvalidOperationException">A definition is owned twice.</exception>
    internal void Joins(IReadOnlyList<EnemyDefinitionId> definitions)
    {
        foreach (EnemyDefinitionId definition in Ensure.NotNull(definitions))
            Own(definition);
    }

    /// <summary>Records <paramref name="bind"/> as how an enemy of <paramref name="definitions"/> placed in its room is built.</summary>
    private void Place(IReadOnlyList<EnemyDefinitionId> definitions, Func<RoomEnemyPlacement, PopulationEnemySpawn> bind)
    {
        foreach (EnemyDefinitionId definition in Ensure.NotNull(definitions))
        {
            Own(definition);
            Placed.Add(definition, bind);
        }
    }

    /// <summary>Records the registering species as <paramref name="definition"/>'s owner.</summary>
    /// <exception cref="InvalidOperationException">Another species owns it.</exception>
    private void Own(EnemyDefinitionId definition)
    {
        if (!Owners.TryAdd(definition, _offering))
            throw new InvalidOperationException($"Enemy definition {definition} has two factories: {Owners[definition]} and {_offering}.");
    }

    /// <summary>The rejection of an entry whose settings are not those its species takes.</summary>
    private static InvalidDataException Mismatch<TSettings>(RoomEnemyPlacement placement) =>
        new($"Enemy {placement.DefinitionId}'s population entry carries {placement.SpawnSettings}, not {typeof(TSettings).Name}.");
}
