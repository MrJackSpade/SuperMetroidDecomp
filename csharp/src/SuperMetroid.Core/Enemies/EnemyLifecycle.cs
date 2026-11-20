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
        // The occupant carries its placement into the entity its species creates, so a species that places several actors
        // creates the one the placement names (#1953).
        place.Bind(population, definition);
        place.Radius = new(definition.Radius.X, definition.Radius.Y);
        place.Health = definition.Health;
        place.Position = population.Placing.Place(place.Position);

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

    /// <summary>
    /// Runs the setup of the enemy the room's population load has just placed: its own, or the one it runs from the load's
    /// <paramref name="surroundings"/>, which the load has; nothing for an enemy that has none.
    /// </summary>
    /// <exception cref="InvalidDataException">The place holds no entity.</exception>
    private static void SetUpEnemy(RoomEnemyPlace place, EnemyPopulationSurroundings surroundings)
    {
        Ensure.NotNull(surroundings);
        EnemyOccupant occupant = OccupantOf(place);
        occupant.Initialization?.Initialize();
        occupant.SurroundingsInitialization?.Initialize(surroundings);
    }

    /// <summary>
    /// Runs the setup of an enemy spawned on another enemy's behalf, or rebuilt by a respawn (#2455): its own, and nothing
    /// for an enemy that has none. Such a spawn has no population load's surroundings, so a setup that reads them is the
    /// population load's alone.
    /// </summary>
    /// <exception cref="InvalidDataException">The place holds no entity.</exception>
    private static void SetUpSpawnedEnemy(RoomEnemyPlace place) =>
        OccupantOf(place).Initialization?.Initialize();

    /// <summary>The occupant <paramref name="place"/> holds.</summary>
    /// <exception cref="InvalidDataException">The place holds no entity.</exception>
    private static EnemyOccupant OccupantOf(RoomEnemyPlace place) =>
        Ensure.NotNull(place).BoundEntity ?? throw new InvalidDataException($"Enemy place {place.Ordinal} has no entity to set up.");

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
    private RoomEnemyPlace? Spawn(RoomEnemyPlacement population, Action<RoomEnemyPlace> joining)
    {
        if (_places.FindFreePlace() is not { } place)
            return null;
        InitializePlaceFromDefinition(place, population, _room.Definitions.ReadEnemyDefinition(population.DefinitionId));
        joining(place);
        SetUpSpawnedEnemy(place);
        // Spawning writes the empty frame after initialization, before the first animation tick.
        place.Frame = place.Processing.HasAny(EnemyProcessing.Animates)
            ? EnemyFrameId.PointAtOrigin
            : null;
        _places.CoverSpawn(place);
        return place;
    }

    /// <summary>Routes a scheduled enemy update through its species' main AI.</summary>
    void IEnemyLifecycle.RunMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunMainAi(place, frame);

    /// <summary>Exposes the enemies selected for Samus collision checks this frame.</summary>
    IReadOnlyList<RoomEnemyOccupant> IEnemyLifecycle.InteractiveEnemies => _processingWindow.InteractiveEnemies;

    /// <summary>
    /// Runs the main AI the entity bound in a loaded place carries, and nothing for one that has none, as a place awaiting
    /// its enemy's respawn has none: the enemy pass still counts its update.
    /// </summary>
    internal static void RunMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        Ensure.NotNull(place).BoundEntity?.MainAi?.RunMainAi(frame);
}
