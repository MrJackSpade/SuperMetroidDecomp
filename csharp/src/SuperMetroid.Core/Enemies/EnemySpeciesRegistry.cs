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
            _toldGroups.Add(new EnemySettingsKey(typeof(TSettings), definition), factory);
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

    /// <summary>Builds the enemy of an entry that tells its species nothing, through the factory of its definition.</summary>
    /// <exception cref="InvalidOperationException">No species owns the place's definition with entries that tell it nothing.</exception>
    internal EnemyPlaced PlaceUntold(EnemyPlacing placing)
    {
        RoomEnemyPlace place = Ensure.NotNull(placing).Place;
        if (_untoldGroups.TryGetValue(place.DefinitionId, out IEnemyGroupFactory? group))
            return group.CreateGroup(new EnemyGroupPlaces(place, PartsOf(placing, group.PartCount)), Common);
        if (!_untold.TryGetValue(place.DefinitionId, out IEnemyEntityFactory? factory))
            throw new InvalidOperationException($"No enemy species owns definition {place.DefinitionId} with an entry that tells it nothing.");
        return EnemyPlaced.Single(factory.Create(place, Common, placing.Setup));
    }

    /// <summary>Builds the enemy of an entry that tells its species <paramref name="settings"/>, through the factory taking them.</summary>
    /// <exception cref="InvalidOperationException">No species in the room takes <typeparamref name="TSettings"/> for the place's definition.</exception>
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
        if (!_told.TryGetValue(key, out IEnemySpeciesFactory? owner))
            throw new InvalidOperationException($"No enemy species in the room takes {typeof(TSettings).Name} for {placing.Place.DefinitionId}.");
        return EnemyPlaced.Single(((IEnemySettingsFactory<TSettings>)owner).Create(placing.Place, Common, placing.Setup, settings));
    }

    /// <summary>The places of a group's <paramref name="count"/> parts, the first <paramref name="count"/> listed after its head.</summary>
    /// <exception cref="InvalidDataException">The room lists fewer places after the head than its group has parts.</exception>
    private static RoomEnemyPlace[] PartsOf(EnemyPlacing placing, int count) => placing.ListedAfter.Count >= count
        ? [.. placing.ListedAfter.Take(count)]
        : throw new InvalidDataException(
            $"Enemy {placing.Place.DefinitionId} heads a group of {count} parts, but its room lists {placing.ListedAfter.Count} enemies after it.");
}
