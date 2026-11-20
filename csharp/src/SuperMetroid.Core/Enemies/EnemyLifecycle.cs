using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Puts populations into places and runs their AI: place setup from a definition, initialization and main AI
/// through each place's entity, spawns into the first free place, and the places Samus can touch this frame.
/// </summary>
[RoomLifetime]
internal sealed class EnemyLifecycle : IEnemyLifecycle
{
    /// <summary>The room whose definitions spawns read.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The room's enemy places.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>Selects the places that interact with Samus this frame.</summary>
    private readonly EnemyProcessingWindow _processingWindow;

    /// <summary>Binds the room, places and processing window the lifecycle works in.</summary>
    internal EnemyLifecycle(EnemyRoomBinding room, EnemyPlaces places, EnemyProcessingWindow processingWindow)
    {
        _room = room;
        _places = places;
        _processingWindow = processingWindow;
    }

    /// <summary>
    /// Copies population and definition fields into a place, showing its sheet's standard colours, and records a spawn
    /// snapshot before initialization AI runs.
    /// </summary>
    internal static void InitializePlaceFromDefinition(
        RoomEnemyPlace place,
        RoomEnemyPlacement population,
        RoomEnemyDefinition definition)
    {
        // The placement is recorded before the entity is bound, so a species that places several actors creates the one
        // the placement names (#1953).
        place.Spawn = new RoomEnemySpawnSnapshot(population);
        place.Bind(population.DefinitionId, definition);
        place.Radius = new(definition.Radius.X, definition.Radius.Y);
        place.Health = definition.Health;
        place.Position = place.Position.WithWhole(new(population.Position.X, population.Position.Y));

        place.Contact = population.Contact;
        place.Processing = population.Processing;
        place.Presence = population.Presence;
        place.CollidesByFrameHitboxes = population.CollidesByFrameHitboxes;
        place.AiStates = EnemyAiStates.None;
        place.AnimationClock.Restart();
        place.FrameCounter = 0;
        place.HurtFlashFrames = definition.HurtFlashFrames;
        place.Shading = PictureShading.Standard;
    }

    /// <summary>Runs the definition's initialization for a place the room's population load has just placed.</summary>
    internal static void SetUpEnemy(RoomEnemyPlace place, EnemyPopulationSurroundings surroundings)
    {
        Ensure.NotNull(surroundings);
        switch (Ensure.NotNull(place).Entity)
        {
            case IEnemySurroundingsInitialization entity:
                entity.Initialize(surroundings);
                break;
            case IEnemyInitialization entity:
                entity.Initialize();
                break;
            default:
                throw NoInitialization(place);
        }
    }

    /// <summary>
    /// Runs the definition's initialization for a place spawned on another enemy's behalf, or rebuilt by a respawn, which
    /// has no population load's surroundings (#2455).
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The place's enemy initializes from a population load's surroundings, which no such spawn has, or has no initialization.
    /// </exception>
    internal static void SetUpSpawnedEnemy(RoomEnemyPlace place)
    {
        switch (Ensure.NotNull(place).Entity)
        {
            case IEnemyInitialization entity:
                entity.Initialize();
                break;
            case IEnemySurroundingsInitialization:
                throw new InvalidDataException(
                    $"Enemy {place.DefinitionId} initializes from a population load's surroundings, but was spawned on another enemy's behalf.");
            default:
                throw NoInitialization(place);
        }
    }

    /// <summary>The failure for a place whose species has no initialization behaviour.</summary>
    private static InvalidDataException NoInitialization(RoomEnemyPlace place) =>
        new($"Enemy {place.DefinitionId} has no initialization behaviour.");

    /// <summary>Exposes setting up a listed enemy from its population entry to the enemy loader.</summary>
    void IEnemyLifecycle.InitializePlaceFromDefinition(
        RoomEnemyPlace place, RoomEnemyPlacement population, RoomEnemyDefinition definition) =>
        InitializePlaceFromDefinition(place, population, definition);

    /// <summary>Lets the population loader run the place's species initialization.</summary>
    void IEnemyLifecycle.SetUpEnemy(RoomEnemyPlace place, EnemyPopulationSurroundings surroundings) =>
        SetUpEnemy(place, surroundings);

    /// <summary>Lets a species run the initialization of an enemy it spawned on another enemy's behalf.</summary>
    void IEnemyLifecycle.SetUpSpawnedEnemy(RoomEnemyPlace place) => SetUpSpawnedEnemy(place);

    /// <inheritdoc/>
    RoomEnemyPlace? IEnemyLifecycle.SpawnEnemyJoining<TEncounter>(RoomEnemyPlacement population, TEncounter encounter) =>
        Spawn(population, place => place.SetSpeciesState(Ensure.NotNull(encounter)));

    /// <summary>
    /// Installs <paramref name="population"/> in the first free place, gives it what <paramref name="joining"/> installs (nothing for a plain spawn),
    /// runs its initialization AI, starts its animation when it runs one and extends the live enemy range.
    /// </summary>
    private RoomEnemyPlace? Spawn(RoomEnemyPlacement population, Action<RoomEnemyPlace> joining) =>
        _places.FindFreePlace() is { } place ? SpawnAt(place, population, joining) : null;

    /// <summary>
    /// Installs <paramref name="population"/> in <paramref name="place"/>, a free one, gives it what <paramref name="joining"/>
    /// installs, runs its initialization AI, starts its animation when it runs one and extends the live enemy range.
    /// </summary>
    private RoomEnemyPlace SpawnAt(RoomEnemyPlace place, RoomEnemyPlacement population, Action<RoomEnemyPlace> joining)
    {
        InitializePlaceFromDefinition(place, population, _room.Definitions.ReadEnemyDefinition(population.DefinitionId));
        joining(place);
        SetUpSpawnedEnemy(place);
        // Spawning writes the empty frame after initialization, before the first animation tick.
        place.Frame = place.Processing.HasAny(EnemyProcessing.Animates)
            ? EnemyFrameId.Empty
            : null;
        _places.CoverSpawn(place);
        return place;
    }

    /// <summary>Routes a scheduled enemy update through its species' main AI.</summary>
    void IEnemyLifecycle.RunMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunMainAi(place, frame);

    /// <summary>Exposes the enemies selected for Samus collision checks this frame.</summary>
    IReadOnlyList<RoomEnemyPlace> IEnemyLifecycle.InteractiveEnemies => _processingWindow.InteractiveEnemies;

    /// <summary>Runs a loaded place's main AI through its species' main behaviour.</summary>
    internal static void RunMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        // A place awaiting its enemy's respawn has no behaviour; the enemy pass still counts its update.
        if (place.AwaitsRespawn)
            return;
        if (place.Entity is not IEnemyAi entity)
            throw new InvalidDataException($"Enemy {place.DefinitionId} has no main behaviour.");
        entity.RunMainAi(frame);
    }
}
