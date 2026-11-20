namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The species registry: which factory builds the enemy of each population entry (#2554). An entry that tells its species
/// nothing reaches the factory of its definition; an entry that tells it settings hands them to the factory that takes
/// those settings, so the factory receives them typed; a group's head brings its parts, which are built through it. An
/// enemy system's registry covers every definition its room can bind (<see cref="ForRoom"/>).
/// </summary>
internal sealed class EnemySpeciesRegistry
{
    /// <summary>The owner of every converted definition, for the registry's checks.</summary>
    private readonly Dictionary<EnemyDefinitionId, IEnemySpeciesFactory> _owners = [];

    /// <summary>The factory of each definition whose entries tell their species nothing.</summary>
    private readonly Dictionary<EnemyDefinitionId, IEnemyEntityFactory> _untold = [];

    /// <summary>The group factory of each head whose entry tells it nothing.</summary>
    private readonly Dictionary<EnemyDefinitionId, IEnemyGroupFactory> _untoldGroups = [];

    /// <summary>
    /// The factory taking each kind of settings for each of its definitions, keyed by the settings' type and the definition:
    /// only <see cref="Add{TSettings}(IEnemySettingsFactory{TSettings})"/> writes it, under the type the factory takes, so
    /// every value is a factory of its key's settings.
    /// </summary>
    private readonly Dictionary<EnemySettingsKey, IEnemySpeciesFactory> _told = [];

    /// <summary>
    /// The group factory taking each kind of head settings for each of its definitions, keyed by the settings' type and the
    /// definition: only <see cref="Add{TSettings}(IEnemySettingsGroupFactory{TSettings})"/> writes it, under the type the
    /// factory takes.
    /// </summary>
    private readonly Dictionary<EnemySettingsKey, IEnemySpeciesFactory> _toldGroups = [];

    /// <summary>How many parts each group taking settings heads, under the key its factory is recorded at.</summary>
    private readonly Dictionary<EnemySettingsKey, int> _toldGroupParts = [];

    /// <summary>The shared participation every entity composes.</summary>
    internal EnemyCommonParticipation Common { get; }

    /// <summary>Records each factory's definitions and how it builds them, rejecting a definition owned twice.</summary>
    /// <param name="common">The shared participation every entity composes.</param>
    /// <param name="factories">The species' factories.</param>
    /// <exception cref="InvalidOperationException">Two factories own one definition, or two take one kind of settings.</exception>
    internal EnemySpeciesRegistry(EnemyCommonParticipation common, IEnumerable<IEnemySpeciesFactory> factories)
    {
        Common = Ensure.NotNull(common);
        foreach (IEnemySpeciesFactory factory in Ensure.NotNull(factories))
        {
            foreach (EnemyDefinitionId definition in factory.Definitions)
            {
                if (!_owners.TryAdd(definition, factory))
                    throw new InvalidOperationException(
                        $"Enemy definition {definition} has two factories: {_owners[definition].GetType().Name} and {factory.GetType().Name}.");
            }
            factory.EnrollIn(this);
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
    internal static EnemySpeciesRegistry ForRoom(EnemyCommonParticipation common, IEnumerable<IEnemySpeciesFactory> factories,
        IReadOnlyCollection<EnemyDefinitionId> withoutScenery)
    {
        Ensure.NotNull(withoutScenery);
        var registry = new EnemySpeciesRegistry(common, factories);
        foreach (EnemyDefinitionId definition in withoutScenery)
        {
            if (registry._owners.ContainsKey(definition))
                throw new InvalidOperationException($"Enemy definition {definition} has a factory although the room's scenery builds none.");
        }
        var unowned = RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Id)
            .Where(definition => !registry._owners.ContainsKey(definition) && !withoutScenery.Contains(definition))
            .ToList();
        if (unowned.Count != 0)
            throw new InvalidOperationException(
                $"Bindable enemy definitions have no factory: {string.Join(", ", unowned)}.");
        return registry;
    }

    /// <summary>Records how <paramref name="factory"/> builds the enemies of its definitions, whose entries tell them nothing.</summary>
    internal void Add(IEnemyEntityFactory factory)
    {
        foreach (EnemyDefinitionId definition in Ensure.NotNull(factory).Definitions)
            _untold.Add(definition, factory);
    }

    /// <summary>
    /// Records <paramref name="factory"/> as the one building the enemies of its definitions whose entries tell
    /// <typeparamref name="TSettings"/>.
    /// </summary>
    internal void Add<TSettings>(IEnemySettingsFactory<TSettings> factory) where TSettings : EnemySpawnSettings
    {
        foreach (EnemyDefinitionId definition in Ensure.NotNull(factory).Definitions)
            _told.Add(new EnemySettingsKey(typeof(TSettings), definition), factory);
    }

    /// <summary>Records <paramref name="factory"/> as the one building the group its head's definition heads.</summary>
    internal void Add(IEnemyGroupFactory factory) => _untoldGroups.Add(Ensure.NotNull(factory).HeadDefinition, factory);

    /// <summary>
    /// Records <paramref name="factory"/> as the one building the groups headed by its definitions whose entries tell
    /// <typeparamref name="TSettings"/>.
    /// </summary>
    internal void Add<TSettings>(IEnemySettingsGroupFactory<TSettings> factory) where TSettings : EnemySpawnSettings
    {
        foreach (EnemyDefinitionId definition in Ensure.NotNull(factory).Definitions)
        {
            var key = new EnemySettingsKey(typeof(TSettings), definition);
            _toldGroups.Add(key, factory);
            _toldGroupParts.Add(key, factory.PartCount);
        }
    }

    /// <summary>
    /// Checks <paramref name="population"/>, the room's population, once as the room loads it (#2554): every entry the
    /// registry places has the factory its settings and definition name, and every group's head is followed by its parts.
    /// The placements then reach their factories without a lookup that can miss.
    /// </summary>
    /// <param name="population">The room's population.</param>
    /// <exception cref="InvalidDataException">An entry has no factory, or a head lists fewer parts after it than its group has.</exception>
    internal void Validate(RoomEnemyPopulationDefinition population)
    {
        Ensure.NotNull(population);
        ReadOnlySpan<RoomEnemyPlacement> placements = population.Placements.Span;
        int index = 0;
        while (index < placements.Length)
        {
            RoomEnemyPlacement entry = placements[index];
            int parts = entry.SpawnSettings.PartsHeadedIn(this, entry.DefinitionId);
            if (placements.Length - index - 1 < parts)
                throw new InvalidDataException(
                    $"Enemy {entry.DefinitionId} heads a group of {parts} parts, but its room lists {placements.Length - index - 1} enemies after it.");
            index += 1 + parts;
        }
    }

    /// <summary>How many parts an entry of <paramref name="definition"/> that tells its species nothing heads: none for a single enemy.</summary>
    /// <exception cref="InvalidDataException">No species owns the definition with entries that tell it nothing.</exception>
    internal int PartsHeadedUntold(EnemyDefinitionId definition)
    {
        if (_untoldGroups.TryGetValue(definition, out IEnemyGroupFactory? group))
            return group.PartCount;
        if (_untold.ContainsKey(definition))
            return 0;
        throw new InvalidDataException($"No enemy species owns definition {definition} with an entry that tells it nothing.");
    }

    /// <summary>
    /// How many parts an entry of <paramref name="definition"/> telling <typeparamref name="TSettings"/> heads: none for a
    /// single enemy.
    /// </summary>
    /// <exception cref="InvalidDataException">No species in the room takes the settings for the definition.</exception>
    internal int PartsHeadedBy<TSettings>(TSettings settings, EnemyDefinitionId definition) where TSettings : EnemySpawnSettings
    {
        Ensure.NotNull(settings);
        var key = new EnemySettingsKey(typeof(TSettings), definition);
        if (_toldGroupParts.TryGetValue(key, out int parts))
            return parts;
        if (_told.ContainsKey(key))
            return 0;
        throw new InvalidDataException($"No enemy species in the room takes {typeof(TSettings).Name} for {definition}.");
    }

    /// <summary>
    /// Builds the enemy for the occupant <paramref name="place"/> now holds, from the settings its entry tells it, through its
    /// species' factory; a group's head takes its parts' places from <paramref name="listedAfter"/>.
    /// </summary>
    /// <param name="place">The place, already holding its new identity and entry.</param>
    /// <param name="setup">How the occupant comes to be set up.</param>
    /// <param name="listedAfter">The places the room's population lists right after the occupant; empty for a spawn.</param>
    internal EnemyPlaced Place(RoomEnemyPlace place, EnemySetup setup, IReadOnlyList<RoomEnemyPlace> listedAfter) =>
        Ensure.NotNull(place).Spawn.Population.SpawnSettings.Place(this, new EnemyPlacing(place, setup, listedAfter));

    /// <summary>
    /// Builds the enemy of an entry that tells its species nothing, through the factory of its definition, which the room's
    /// load checked it has (<see cref="Validate"/>).
    /// </summary>
    internal EnemyPlaced PlaceUntold(EnemyPlacing placing)
    {
        RoomEnemyPlace place = Ensure.NotNull(placing).Place;
        if (_untoldGroups.TryGetValue(place.DefinitionId, out IEnemyGroupFactory? group))
            return group.CreateGroup(new EnemyGroupPlaces(place, PartsOf(placing, group.PartCount)), Common);
        return EnemyPlaced.Single(_untold[place.DefinitionId].Create(place, Common, placing.Setup));
    }

    /// <summary>
    /// Builds the enemy of an entry that tells its species <paramref name="settings"/>, through the factory taking them,
    /// which the room's load checked it has (<see cref="Validate"/>).
    /// </summary>
    internal EnemyPlaced Place<TSettings>(TSettings settings, EnemyPlacing placing) where TSettings : EnemySpawnSettings
    {
        Ensure.NotNull(settings);
        Ensure.NotNull(placing);
        var key = new EnemySettingsKey(typeof(TSettings), placing.Place.DefinitionId);
        // Each dictionary holds under a settings type only a factory registered as taking that type.
        if (_toldGroups.TryGetValue(key, out IEnemySpeciesFactory? groupOwner))
        {
            var group = (IEnemySettingsGroupFactory<TSettings>)groupOwner;
            return group.CreateGroup(new EnemyGroupPlaces(placing.Place, PartsOf(placing, group.PartCount)), Common, settings);
        }
        return EnemyPlaced.Single(((IEnemySettingsFactory<TSettings>)_told[key]).Create(placing.Place, Common, placing.Setup, settings));
    }

    /// <summary>
    /// The places of a group's <paramref name="count"/> parts, the first <paramref name="count"/> listed after its head,
    /// which the room's load checked it lists (<see cref="Validate"/>).
    /// </summary>
    private static RoomEnemyPlace[] PartsOf(EnemyPlacing placing, int count) => [.. placing.ListedAfter.Take(count)];
}
