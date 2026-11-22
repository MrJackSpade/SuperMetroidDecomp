using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The species registry: how each definition a room's population places is built (#2554). An enemy system's registry
/// covers every definition its room can bind (<see cref="ForRoom"/>); the population load binds each entry to its
/// species' creation once, checking the entry's settings against the type that creation takes.
/// </summary>
[RoomLifetime]
internal sealed partial class EnemySpeciesRegistry
{
    /// <summary>How each definition a population places binds its entries.</summary>
    private readonly Dictionary<EnemyDefinitionId, Func<RoomEnemyPlacement, PopulationEnemySpawn>> _placed;

    /// <summary>The creations parts already placed offer the parts the room lists after them.</summary>
    private readonly EnemyPartOffers _offers;

    /// <summary>Composes the registry from <paramref name="registrar"/>'s registrations.</summary>
    private EnemySpeciesRegistry(EnemyCommonParticipation common, EnemySpeciesRegistrar registrar, EnemyPartOffers offers)
    {
        Common = Ensure.NotNull(common);
        _placed = registrar.Placed;
        _offers = Ensure.NotNull(offers);
    }

    /// <summary>The shared participation every entity composes.</summary>
    internal EnemyCommonParticipation Common { get; }

    /// <summary>
    /// The registry of a room's enemy system: every definition the room can place has exactly one owner. A definition
    /// whose owner needs scenery the room lacks (<paramref name="withoutScenery"/>: the statue room's statue, the turning
    /// floor's elevator door) has none, and placing it fails where its entry is bound.
    /// </summary>
    /// <param name="common">The shared participation every entity composes.</param>
    /// <param name="factories">The species' factories.</param>
    /// <param name="withoutScenery">The definitions whose owners the room's scenery does not let it build.</param>
    /// <param name="offers">The creations parts already placed offer the parts the room lists after them.</param>
    /// <exception cref="InvalidOperationException">
    /// Two factories own one definition, none owns a bindable one the room's scenery allows, or one owns a definition the
    /// room's scenery does not allow.
    /// </exception>
    internal static EnemySpeciesRegistry ForRoom(EnemyCommonParticipation common, IEnumerable<IEnemyEntityFactory> factories,
        IReadOnlyCollection<EnemyDefinitionId> withoutScenery, EnemyPartOffers offers)
    {
        Ensure.NotNull(withoutScenery);
        var registrar = new EnemySpeciesRegistrar();
        foreach (IEnemyEntityFactory factory in Ensure.NotNull(factories))
            registrar.Add(factory);
        foreach (EnemyDefinitionId definition in withoutScenery)
        {
            if (registrar.Owners.ContainsKey(definition))
                throw new InvalidOperationException($"Enemy definition {definition} has a factory although the room's scenery builds none.");
        }
        var unowned = RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Id)
            .Where(definition => !registrar.Owners.ContainsKey(definition) && !withoutScenery.Contains(definition))
            .ToList();
        if (unowned.Count != 0)
            throw new InvalidOperationException(
                $"Bindable enemy definitions have no factory: {string.Join(", ", unowned)}.");
        return new EnemySpeciesRegistry(common, registrar, offers);
    }

    /// <summary>
    /// Binds <paramref name="placement"/>, an entry of the room's population, to its species' creation, checking once that
    /// its settings are the type that creation takes: the creation an earlier part offered it (<see cref="EnemyPartOffers"/>),
    /// or its species' own.
    /// </summary>
    /// <param name="placement">The population entry.</param>
    /// <returns>The bound entry, which builds its enemy with no further check.</returns>
    /// <exception cref="InvalidDataException">
    /// No species places the entry's enemy from a population, or the entry carries other settings than its species takes.
    /// </exception>
    internal PopulationEnemySpawn Bind(RoomEnemyPlacement placement)
    {
        if (_offers.TryBind(Ensure.NotNull(placement), out PopulationEnemySpawn? offered))
            return offered;
        return _placed.TryGetValue(placement.DefinitionId, out Func<RoomEnemyPlacement, PopulationEnemySpawn>? bind)
            ? bind(placement)
            : throw new InvalidDataException($"No species in this room places enemy {placement.DefinitionId} from its population, and no part placed before it offered one.");
    }
}
