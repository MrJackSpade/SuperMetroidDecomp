using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Puts populations into slots and runs their banked AI: slot setup from a definition, initialization and main AI
/// through each slot's entity, spawns into the first free slot, and the slots Samus can touch this frame.
/// </summary>
[RoomLifetime]
internal sealed class EnemySlotDispatch : IEnemyDispatch
{
    /// <summary>The room whose definitions spawns read.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The 32 native enemy slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The room's graphics set, which gives each enemy its tile and palette indexes.</summary>
    private readonly EnemyGraphicsSet _graphics;
    /// <summary>Selects the slots that interact with Samus this frame.</summary>
    private readonly EnemyProcessingWindow _processingWindow;

    /// <summary>Binds the room, slots, graphics set and processing window the dispatch works in.</summary>
    internal EnemySlotDispatch(EnemyRoomBinding room, EnemySlotTable slotTable, EnemyGraphicsSet graphics,
        EnemyProcessingWindow processingWindow)
    {
        _room = room;
        _slotTable = slotTable;
        _graphics = graphics;
        _processingWindow = processingWindow;
    }

    /// <summary>
    /// Copies population and definition fields into a physical slot, resolving room graphics
    /// indexes and recording a spawn snapshot before initialization AI runs.
    /// </summary>
    internal void InitializeSlotFromDefinition(
        RoomEnemySlot slot,
        RoomEnemyPopulationRecord population,
        RoomEnemyDefinition definition)
    {
        (int tileIndex, int paletteIndex) = _graphics.FindGraphicsIndexes(population.DefinitionId);

        slot.Bind(population.DefinitionId, definition);
        slot.Radius = new(definition.Radius.X, definition.Radius.Y);
        slot.Health = definition.Health;
        slot.Layer = definition.Layer;
        slot.Position = slot.Position.WithWhole(new(population.Position.X, population.Position.Y));
        slot.CurrentInstruction = population.InitializationParameter;
        slot.Properties = population.Properties;
        slot.ExtraProperties = population.ExtraProperties;
        slot.AiStates = EnemyAiStates.None;
        slot.Timer = WrappingCountdown.Start(0);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.FrameCounter = 0;
        slot.HurtAiTime = definition.HurtAiTime;
        slot.PaletteIndex = paletteIndex;
        slot.TileIndexBase = tileIndex;
        slot.Spawn = new RoomEnemySpawnSnapshot(population);
    }

    /// <summary>Dispatches the definition's banked initialization pointer for the newly loaded slot.</summary>
    internal static void RunInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        Ensure.NotNull(surroundings);
        switch (slot.Entity)
        {
            case IEnemySurroundingsInitialization entity:
                (entity.FindSurroundingsInitialization() ?? throw Untranslated(slot))(slot, surroundings);
                break;
            case IEnemyInitialization entity:
                (entity.FindInitialization() ?? throw Untranslated(slot))(slot);
                break;
            default:
                throw Untranslated(slot);
        }
    }

    /// <summary>The failure for a slot whose definition names an initialization AI this build does not translate.</summary>
    private static InvalidDataException Untranslated(RoomEnemySlot slot) =>
        new($"Enemy {slot.DefinitionId} initialization AI is not translated.");

    /// <summary>Exposes physical-slot population setup to the enemy loader.</summary>
    void IEnemyDispatch.InitializeSlotFromDefinition(
        RoomEnemySlot slot, RoomEnemyPopulationRecord population, RoomEnemyDefinition definition) =>
        InitializeSlotFromDefinition(slot, population, definition);

    /// <summary>Lets the population loader invoke the slot's compiled initialization handler.</summary>
    void IEnemyDispatch.RunInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings) =>
        RunInitializationAi(slot, surroundings);

    /// <inheritdoc/>
    RoomEnemySlot? IEnemyDispatch.SpawnEnemy(RoomEnemyPopulationRecord population)
    {
        int slotIndex = _slotTable.FindFreeSlotIndex();
        if (slotIndex < 0)
            return null;
        RoomEnemySlot slot = _slotTable.Slots[slotIndex];
        InitializeSlotFromDefinition(slot, population, _room.LoadedDefinitions.ReadEnemyDefinition(population.DefinitionId));
        RunInitializationAi(slot, EnemySpawnSurroundings.None);
        // SpawnEnemy's post-init $A0:93D9 write precedes the first instruction tick.
        slot.Frame = slot.Properties.HasAny(EnemyProperties.ProcessInstructions)
            ? EnemyFrameId.Empty
            : null;
        _slotTable.EnemyCount = unchecked((ushort)Math.Max(_slotTable.EnemyCount, slotIndex + 1));
        _slotTable.FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * EnemySlotTable.EnemyIndexStride));
        return slot;
    }

    /// <summary>Routes a scheduled enemy update through its banked main AI handler.</summary>
    void IEnemyDispatch.RunMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunMainAi(slot, frame);

    /// <summary>Exposes native slot offsets selected for Samus collision checks this frame.</summary>
    IReadOnlyList<int> IEnemyDispatch.InteractiveEnemyIndexes => _processingWindow.InteractiveEnemyIndexes;

    /// <summary>Dispatches a loaded slot's banked main AI pointer through the compiled table.</summary>
    internal static void RunMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        EnemyMainAiHandler handler = (slot.Entity as IEnemyMainAi)?.FindMain() ?? throw new InvalidDataException(
            $"Enemy {slot.DefinitionId} main AI is not translated.");
        handler(slot, frame);
    }
}
