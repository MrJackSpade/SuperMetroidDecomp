using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Room enemy population loading: the population and kill quota, and resolution of enemy definitions and
/// populations from the room state.
/// </summary>
internal sealed class EnemyPopulationLoader
{
    /// <summary>Supplies the selected enemy artwork and guards room-bound operations.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Receives the boss identity declared by an initialized population entry.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Holds the room's enemy slots and the extent the population fills.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies enemy definitions and runs each slot's initialization AI.</summary>
    private readonly IEnemyLifecycle _lifecycle;

    /// <summary>Binds room artwork, status, slot storage, and the enemy lifecycle for a population load.</summary>
    internal EnemyPopulationLoader(EnemyRoomBinding room, EnemyRoomStatus roomStatus, EnemySlotTable slotTable, IEnemyLifecycle lifecycle)
    {
        _room = room;
        _roomStatus = roomStatus;
        _slotTable = slotTable;
        _lifecycle = lifecycle;
    }
    /// <summary>
    /// The kills that clear the room for its grey doors and clearance watches: the room population's own quota
    /// (<see cref="RoomEnemyPopulationDefinition.DeathQuota"/>) when the room places enemies; a room that places none
    /// keeps the quota of the last room that did.
    /// </summary>
    public int DeathQuota
    {
        get => _room.Session.DeathQuota;
        private set => _room.Session.DeathQuota = value;
    }

    /// <summary>Initializes each population slot, runs its AI, and publishes count and kill quota for nonempty populations.</summary>
    internal void LoadPopulation(
        IRoomEnemyDefinitionSource definitions,
        RoomEnemyPopulationDefinition populationDefinition,
        EnemySpawnSurroundings surroundings)
    {
        ReadOnlySpan<RoomEnemyPlacement> placements = populationDefinition.Placements.Span;
        for (int slotIndex = 0; slotIndex < placements.Length; slotIndex++)
        {
            RoomEnemyPlacement population = placements[slotIndex];
            EnemyDefinitionId enemy = population.DefinitionId;
            if (slotIndex == EnemySlotOrder.Capacity)
            {
                throw new InvalidDataException(
                    $"Enemy population {populationDefinition.Name} has more placements than a room holds enemies.");
            }

            RoomEnemyDefinition definition = definitions.ReadEnemyDefinition(enemy);
            RoomEnemySlot slot = _slotTable.Slots[slotIndex];
            _lifecycle.InitializeSlotFromDefinition(slot, population, definition);
            if (definition.Boss is { } boss)
                _roomStatus.Boss = boss;
            _lifecycle.RunInitializationAi(slot, surroundings);

            // Loading replaces whatever frame the initialization AI showed: an animated enemy shows the empty frame until
            // its first animation tick replaces it, and one that collides by its frames' hitboxes collides at the point
            // at its origin meanwhile. An enemy without an animation shows no frame.
            slot.Frame = slot.Processing.HasAny(EnemyProcessing.Animates)
                ? EnemyFrameId.Empty
                : null;

        }

        // An empty population leaves the enemy count at zero and keeps the previous room's
        // death quota. A nonempty one publishes both after every initialization has run.
        if (placements.IsEmpty)
            return;
        DeathQuota = populationDefinition.DeathQuota;
        _slotTable.EnemyCount = placements.Length;
    }
}
