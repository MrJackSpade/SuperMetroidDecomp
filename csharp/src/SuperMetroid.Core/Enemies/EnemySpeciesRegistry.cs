using SuperMetroid.Core.Enemies.Inert;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The species registry: which factory creates the entity for each definition. An enemy system's registry covers every
/// definition a retail room or the respawn placeholder can bind (<see cref="ForRetailRooms"/>), except the species no
/// code ports yet.
/// </summary>
internal sealed class EnemySpeciesRegistry
{
    /// <summary>
    /// Room-referenced definitions whose species AI is not ported: Reflec ($A3:DBD3/$A3:DC1B) and the second dead
    /// Sidehopper header, whose $A9:D7B6 initialization was ported only for the first. Binding one fails loudly.
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
                    $"Enemy definition ${(int)definition:X4} has two factories: {_factories[definition].GetType().Name} and {factory.GetType().Name}.");
        }
    }

    /// <summary>
    /// The registry of an enemy system: every definition a retail room places, and the respawn placeholder, has exactly
    /// one factory unless its species is listed as not ported.
    /// </summary>
    /// <param name="common">The shared participation every entity composes.</param>
    /// <param name="factories">The species' factories.</param>
    /// <exception cref="InvalidOperationException">Two factories own one definition, or none owns a bindable one.</exception>
    internal static EnemySpeciesRegistry ForRetailRooms(EnemyCommonParticipation common, IEnumerable<IEnemyEntityFactory> factories)
    {
        var registry = new EnemySpeciesRegistry(common, factories);
        var unowned = RoomEnemyDefinitionCatalog.Definitions.Select(entry => entry.Pointer).Append(EnemyDefinitionId.Respawn)
            .Where(definition => !registry._factories.ContainsKey(definition) && !UnportedDefinitions.Contains(definition))
            .ToList();
        if (unowned.Count != 0)
            throw new InvalidOperationException(
                $"Bindable enemy definitions have no factory: {string.Join(", ", unowned.Select(definition => $"{definition} (${(int)definition:X4})"))}.");
        return registry;
    }

    /// <summary>
    /// Creates the entity for the occupant <paramref name="slot"/> now holds. A header outside the retail placements
    /// (constructed cartridge data, or a labelled header no room places) has no species: it runs only the shared
    /// participation, and any AI it names beyond the shared $804C long returns fails at dispatch.
    /// </summary>
    /// <param name="slot">The slot, already holding its new identity.</param>
    /// <exception cref="InvalidDataException">The slot holds a definition whose species AI is not ported.</exception>
    internal IEnemyEntity Create(RoomEnemySlot slot)
    {
        Ensure.NotNull(slot);
        if (_factories.TryGetValue(slot.EnemyDefinitionPointer, out IEnemyEntityFactory? factory))
            return factory.Create(slot, _common);
        if (UnportedDefinitions.Contains(slot.EnemyDefinitionPointer))
            throw new InvalidDataException($"Enemy slot {slot.SlotIndex} holds {slot.EnemyDefinitionPointer}, whose species AI is not ported.");
        return new InertEnemyEntity(slot, _common);
    }
}
