using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The species registry each fixture place binds its enemies through (#2554): a place knows only the participation its
/// registry shares, so a check's binding finds the registry that participation belongs to. Every registry a check binds
/// through is registered as it is built: the enemy system fixtures' and the standalone fixture registries.
/// </summary>
internal static class EnemyPlaceRegistries
{
    /// <summary>Each registered registry, by the participation its places share.</summary>
    private static readonly ConditionalWeakTable<EnemyCommonParticipation, EnemySpeciesRegistry> registries = new();

    /// <summary>Registers <paramref name="registry"/>, whose places a check binds enemies in.</summary>
    /// <param name="registry">The registry.</param>
    /// <returns>The registry.</returns>
    internal static EnemySpeciesRegistry Register(EnemySpeciesRegistry registry)
    {
        registries.AddOrUpdate(Ensure.NotNull(registry).Common, registry);
        return registry;
    }

    /// <summary>
    /// A registry for fixture places, owning the definitions <paramref name="factories"/> place, built as a room's registry
    /// is built (<see cref="EnemySpeciesRegistry.ForRoom"/>) for a room whose scenery lets no other species be built.
    /// </summary>
    /// <param name="factories">The species' factories.</param>
    /// <returns>The registered registry.</returns>
    internal static EnemySpeciesRegistry Of(params IEnemyEntityFactory[] factories)
    {
        var owned = new EnemySpeciesRegistrar();
        foreach (IEnemyEntityFactory factory in factories)
            owned.Add(factory);
        EnemyDefinitionId[] unbuilt = [.. RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Id)
            .Where(definition => !owned.Owners.ContainsKey(definition))];
        return Register(EnemySpeciesRegistry.ForRoom(
            new EnemyCommonParticipation(() => throw new InvalidOperationException("A standalone fixture slot has no enemy system to share reactions with.")),
            factories, unbuilt, new EnemyPartOffers()));
    }

    /// <summary>The registry <paramref name="place"/> binds its enemies through.</summary>
    /// <param name="place">A fixture place.</param>
    /// <exception cref="InvalidOperationException">No check registered the registry the place belongs to.</exception>
    internal static EnemySpeciesRegistry Of(RoomEnemyPlace place)
    {
        EnemyCommonParticipation common = Ensure.NotNull(place).Common;
        return registries.TryGetValue(common, out EnemySpeciesRegistry? registry)
            ? registry
            : throw new InvalidOperationException($"Enemy place {place.Ordinal} belongs to an enemy system whose registry no check registered.");
    }

    /// <summary>
    /// Binds <paramref name="entry"/> in <paramref name="place"/> with <paramref name="definition"/>, as a check authors the
    /// place's population entry (#2554): the place holds the occupant's identity and entry at once, and the entry is bound
    /// to its species' creation through the place's registry, as a population load binds it, where the enemy is built. A
    /// check authors the entry's settings, and sets up the body whose setup offers a part's creation, before it builds
    /// either; one that sets its enemy up as placed binds the entry then (<see cref="RoomEnemyPlaceInitializationAccess"/>),
    /// and a build from the binding itself, as a respawn builds it, binds it as it builds.
    /// </summary>
    /// <param name="place">The fixture place.</param>
    /// <param name="entry">The population entry.</param>
    /// <param name="definition">The occupant's definition.</param>
    /// <returns>The new occupant.</returns>
    internal static RoomEnemyOccupant BindEntry(this RoomEnemyPlace place, RoomEnemyPlacement entry, RoomEnemyDefinition definition) =>
        Ensure.NotNull(place).Bind(new OrdinaryEnemySpawn<EnemySpawnSettings>(Ensure.NotNull(entry), entry.SpawnSettings, BuildBound), definition);

    /// <summary>
    /// Builds <paramref name="occupant"/> from its entry bound through <paramref name="place"/>'s registry as it is built:
    /// the creation an earlier part offered it, or its species' own.
    /// </summary>
    /// <exception cref="InvalidOperationException">Only the population load builds the occupant's enemy.</exception>
    private static EnemyOccupant BuildBound(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) =>
        Of(place).Bind(occupant.Spawn).Respawn is { } build
            ? build.Create(occupant, place, common)
            : throw new InvalidOperationException($"Enemy {occupant.DefinitionId} is built only by its room's population load; a check sets it up as placed.");
}
