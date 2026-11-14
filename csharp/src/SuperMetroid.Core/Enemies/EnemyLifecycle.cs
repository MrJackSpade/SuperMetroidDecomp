using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Puts populations into slots and runs their AI: slot setup from a definition, initialization and main AI
/// through each slot's entity, spawns into the first free slot, and the slots Samus can touch this frame.
/// </summary>
[RoomLifetime]
internal sealed class EnemyLifecycle : IEnemyLifecycle
{
    /// <summary>The room whose definitions spawns read.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy slots.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Selects the slots that interact with Samus this frame.</summary>
    private readonly EnemyProcessingWindow _processingWindow;

    /// <summary>Binds the room, slots and processing window the lifecycle works in.</summary>
    internal EnemyLifecycle(EnemyRoomBinding room, EnemySlotTable slotTable, EnemyProcessingWindow processingWindow)
    {
        _room = room;
        _slotTable = slotTable;
        _processingWindow = processingWindow;
    }

    /// <summary>
    /// Copies population and definition fields into a slot, showing its sheet's standard colours, and records a spawn
    /// snapshot before initialization AI runs.
    /// </summary>
    internal static void InitializeSlotFromDefinition(
        RoomEnemySlot slot,
        RoomEnemyPlacement population,
        RoomEnemyDefinition definition)
    {
        slot.Bind(population.DefinitionId, definition);
        slot.Radius = new(definition.Radius.X, definition.Radius.Y);
        slot.Health = definition.Health;
        slot.Position = slot.Position.WithWhole(new(population.Position.X, population.Position.Y));

        slot.Contact = population.Contact;
        slot.Processing = population.Processing;
        slot.Presence = population.Presence;
        slot.CollidesByFrameHitboxes = population.CollidesByFrameHitboxes;
        slot.AiStates = EnemyAiStates.None;
        slot.AnimationClock.Restart();
        slot.FrameCounter = 0;
        slot.HurtDuration = definition.HurtFlashFrames;
        slot.Shading = PictureShading.Standard;
        slot.Spawn = new RoomEnemySpawnSnapshot(population);
    }

    /// <summary>Runs the definition's initialization for the newly loaded slot.</summary>
    internal static void RunInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings)
    {
        Ensure.NotNull(surroundings);
        switch (slot.Entity)
        {
            case IEnemySurroundingsInitialization entity:
                entity.Initialize(surroundings);
                break;
            case IEnemyInitialization entity:
                entity.Initialize();
                break;
            default:
                throw NoInitialization(slot);
        }
    }

    /// <summary>The failure for a slot whose species has no initialization behaviour.</summary>
    private static InvalidDataException NoInitialization(RoomEnemySlot slot) =>
        new($"Enemy {slot.DefinitionId} has no initialization behaviour.");

    /// <summary>Exposes setting up a listed enemy from its population entry to the enemy loader.</summary>
    void IEnemyLifecycle.InitializeSlotFromDefinition(
        RoomEnemySlot slot, RoomEnemyPlacement population, RoomEnemyDefinition definition) =>
        InitializeSlotFromDefinition(slot, population, definition);

    /// <summary>Lets the population loader run the slot's species initialization.</summary>
    void IEnemyLifecycle.RunInitializationAi(RoomEnemySlot slot, EnemySpawnSurroundings surroundings) =>
        RunInitializationAi(slot, surroundings);

    /// <inheritdoc/>
    RoomEnemySlot? IEnemyLifecycle.SpawnEnemy(RoomEnemyPlacement population)
    {
        if (_slotTable.FindFreeSlot() is not { } slot)
            return null;
        InitializeSlotFromDefinition(slot, population, _room.LoadedDefinitions.ReadEnemyDefinition(population.DefinitionId));
        RunInitializationAi(slot, EnemySpawnSurroundings.None);
        // Spawning writes the empty frame after initialization, before the first animation tick.
        slot.Frame = slot.Processing.HasAny(EnemyProcessing.Animates)
            ? EnemyFrameId.Empty
            : null;
        _slotTable.CoverSpawn(slot);
        return slot;
    }

    /// <summary>Routes a scheduled enemy update through its species' main AI.</summary>
    void IEnemyLifecycle.RunMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunMainAi(slot, frame);

    /// <summary>Exposes the enemies selected for Samus collision checks this frame.</summary>
    IReadOnlyList<RoomEnemySlot> IEnemyLifecycle.InteractiveEnemies => _processingWindow.InteractiveEnemies;

    /// <summary>Runs a loaded slot's main AI through its species' main behaviour.</summary>
    internal static void RunMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        if (slot.Entity is not IEnemyMainAi entity)
            throw new InvalidDataException($"Enemy {slot.DefinitionId} has no main behaviour.");
        entity.RunMainAi(frame);
    }
}
