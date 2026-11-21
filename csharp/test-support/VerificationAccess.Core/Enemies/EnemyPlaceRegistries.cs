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

    /// <summary>A registry for fixture places, owning the definitions <paramref name="factories"/> place.</summary>
    /// <param name="factories">The species' factories.</param>
    /// <returns>The registered registry.</returns>
    internal static EnemySpeciesRegistry Of(params IEnemyEntityFactory[] factories)
    {
        var registrar = new EnemySpeciesRegistrar();
        foreach (IEnemyEntityFactory factory in factories)
            registrar.Add(factory);
        return Register(new EnemySpeciesRegistry(
            new EnemyCommonParticipation(() => throw new InvalidOperationException("A standalone fixture slot has no enemy system to share reactions with.")),
            registrar,
            new EnemyPartOffers()));
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
    /// Binds <paramref name="entry"/> in <paramref name="place"/> through the place's registry, as a population load binds it,
    /// with <paramref name="definition"/>; the occupant's entity is built where the check sets it up.
    /// </summary>
    /// <param name="place">The fixture place.</param>
    /// <param name="entry">The population entry.</param>
    /// <param name="definition">The occupant's definition.</param>
    /// <returns>The new occupant.</returns>
    internal static RoomEnemyOccupant BindEntry(this RoomEnemyPlace place, RoomEnemyPlacement entry, RoomEnemyDefinition definition) =>
        Ensure.NotNull(place).Bind(Of(place).Bind(entry), definition);
}
