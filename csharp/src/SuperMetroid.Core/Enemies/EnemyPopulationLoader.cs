using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Room enemy population loading: the bank-$A1 population and kill quota, and resolution of enemy definitions,
/// populations and graphics sets from the room state.
/// </summary>
internal sealed class EnemyPopulationLoader
{
    /// <summary>Supplies the selected enemy artwork and guards room-bound operations.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Receives the boss identity declared by an initialized population entry.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Holds the 32 native enemy slots and their published occupied range.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Applies enemy definitions and runs each slot's initialization AI.</summary>
    private readonly IEnemyDispatch _dispatch;

    /// <summary>Binds room artwork, status, slot storage, and AI dispatch for a population load.</summary>
    internal EnemyPopulationLoader(EnemyRoomBinding room, EnemyRoomStatus roomStatus, EnemySlotTable slotTable, IEnemyDispatch dispatch)
    {
        _room = room;
        _roomStatus = roomStatus;
        _slotTable = slotTable;
        _dispatch = dispatch;
    }
    /// <summary>Required room kill count from the nonempty population's terminator for enemy-clear triggers; native empty-population initialization retains the previous quota. 0 through 255.</summary>
    public int DeathQuota
    {
        get => _room.Session.DeathQuota;
        private set => _room.Session.DeathQuota = value;
    }

    /// <summary>Initializes each bank-$A1 population slot, runs its AI, and publishes count and kill quota for nonempty populations.</summary>
    internal void LoadPopulation(
        IRoomEnemyDefinitionSource definitions,
        RoomEnemyPopulationDefinition populationDefinition,
        EnemySpawnSurroundings surroundings)
    {
        ReadOnlySpan<RoomEnemyPopulationRecord> records = populationDefinition.Records.Span;
        for (int slotIndex = 0; slotIndex < records.Length; slotIndex++)
        {
            RoomEnemyPopulationRecord population = records[slotIndex];
            EnemyDefinitionId definitionPointer = population.DefinitionId;
            if (slotIndex == EnemySlotTable.MaximumEnemyCount)
            {
                throw new InvalidDataException(
                    $"Enemy population {populationDefinition.Name} has more than 32 records.");
            }

            RoomEnemyDefinition definition = definitions.ReadEnemyDefinition(definitionPointer);
            RoomEnemySlot slot = _slotTable.Slots[slotIndex];
            _dispatch.InitializeSlotFromDefinition(slot, population, definition);
            if (definition.BossId != 0)
                _roomStatus.BossId = definition.BossId;
            _dispatch.RunInitializationAi(slot, surroundings);

            // InitializeEnemies deliberately clears the init routine's immediate map.
            // Disable-Samus-collision actors receive the canonical empty map until their
            // first instruction-list tick replaces it. Gunship properties contain $2000.
            slot.Frame = slot.Properties.HasAny(EnemyProperties.ProcessInstructions)
                ? slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap)
                    ? EnemyFrameId.EmptyExtended
                    : EnemyFrameId.Empty
                : null;

        }

        // InitializeEnemies returns early for an initially empty population. It has
        // already zeroed both enemy counters, but does not rewrite the previous
        // first-free index or death quota. Nonempty populations publish these after
        // all initialization callbacks, exactly as the native terminator path does.
        if (records.IsEmpty)
            return;
        DeathQuota = populationDefinition.DeathQuota;
        _slotTable.EnemyCount = unchecked((ushort)records.Length);
        _slotTable.FirstFreeEnemyIndex = unchecked((ushort)(records.Length * EnemySlotTable.NativeSlotSize));
    }
}
